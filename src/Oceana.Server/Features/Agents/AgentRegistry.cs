using System.Collections.Concurrent;
using Oceana.Contracts;
using Oceana.Server.Infrastructure.Persistence;

namespace Oceana.Server.Features.Agents;

/// <summary>
/// An in-memory <see cref="IAgentRegistry"/> backed by concurrent dictionaries, with optional JSON
/// persistence of agent <em>configuration</em> (identity, last-seen devices and desired routing).
/// Known agents are loaded on startup as offline; their live state is re-derived on reconnect.
/// </summary>
public sealed class AgentRegistry : IAgentRegistry
{
    private static readonly AgentRouting EmptyRouting = new AgentRouting(Array.Empty<AudioOutput>());

    private readonly ConcurrentDictionary<Guid, AgentInfo> agents = new ConcurrentDictionary<Guid, AgentInfo>();
    private readonly ConcurrentDictionary<string, Guid> connectionAgents = new ConcurrentDictionary<string, Guid>();
    private readonly ConcurrentDictionary<Guid, string> currentConnections = new ConcurrentDictionary<Guid, string>();
    private readonly IStateStore<AgentsState>? store;

    /// <summary>
    /// Initialises a new instance of the <see cref="AgentRegistry"/> class.
    /// </summary>
    /// <param name="store">The store to persist agent configuration to; null disables persistence.</param>
    public AgentRegistry(IStateStore<AgentsState>? store = null)
    {
        this.store = store;

        var state = store?.Load();
        if (state is not null)
        {
            foreach (var config in state.Agents)
            {
                this.agents[config.Id] = new AgentInfo
                {
                    Id = config.Id,
                    Name = config.Name,
                    Host = string.Empty,
                    Port = 0,
                    Connected = false,
                    Status = AgentStatus.Idle,
                    Devices = config.Devices,
                    Routing = config.Routing,
                };
            }
        }
    }

    /// <inheritdoc/>
    public AgentInfo RegisterOrUpdate(AgentRegistration registration, string host, string connectionId)
    {
        var updated = agents.AddOrUpdate(
            registration.AgentId,
            _ => new AgentInfo
            {
                Id = registration.AgentId,
                Name = registration.Name,
                Host = host,
                Port = registration.AudioPort,
                Connected = true,
                Status = AgentStatus.Idle,
                Devices = registration.Devices,
                Routing = EmptyRouting,
            },
            (_, existing) => existing with
            {
                Name = registration.Name,
                Host = host,
                Port = registration.AudioPort,
                Connected = true,
                Devices = registration.Devices,
            });

        connectionAgents[connectionId] = registration.AgentId;
        currentConnections[registration.AgentId] = connectionId;
        PersistConfig();
        return updated;
    }

    /// <inheritdoc/>
    public AgentInfo? MarkOffline(string connectionId)
    {
        if (!connectionAgents.TryRemove(connectionId, out var agentId))
        {
            return null;
        }

        // Ignore the drop if the agent has already reconnected on a newer connection.
        if (!currentConnections.TryGetValue(agentId, out var current) || current != connectionId)
        {
            return null;
        }

        currentConnections.TryRemove(agentId, out _);

        while (agents.TryGetValue(agentId, out var existing))
        {
            var updated = existing with { Connected = false };
            if (agents.TryUpdate(agentId, updated, existing))
            {
                return updated;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AgentInfo> GetAll()
    {
        return agents.Values.ToArray();
    }

    /// <inheritdoc/>
    public AgentInfo? Get(Guid id)
    {
        return agents.TryGetValue(id, out var agent) ? agent : null;
    }

    /// <inheritdoc/>
    public bool Remove(Guid id)
    {
        currentConnections.TryRemove(id, out _);
        if (agents.TryRemove(id, out _))
        {
            PersistConfig();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public AgentInfo? UpdateStatus(Guid id, AgentStatus status, string? lastError = null)
    {
        while (agents.TryGetValue(id, out var existing))
        {
            var updated = existing with { Status = status, LastError = lastError };
            if (agents.TryUpdate(id, updated, existing))
            {
                return updated;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public AgentInfo? SetDesiredRouting(Guid id, AgentRouting routing)
    {
        while (agents.TryGetValue(id, out var existing))
        {
            var updated = existing with { Routing = routing };
            if (agents.TryUpdate(id, updated, existing))
            {
                PersistConfig();
                return updated;
            }
        }

        return null;
    }

    // Persists agent configuration (identity, last-seen devices, routing) — not live state.
    private void PersistConfig()
    {
        if (store is null)
        {
            return;
        }

        var configs = agents.Values
            .Select(agent => new AgentConfig(agent.Id, agent.Name, agent.Devices, agent.Routing))
            .ToArray();
        store.Save(new AgentsState(configs));
    }
}
