using System.Collections.Concurrent;
using Oceana.Contracts;

namespace Oceana.Server.Features.Agents;

/// <summary>
/// An in-memory <see cref="IAgentRegistry"/> backed by concurrent dictionaries.
/// </summary>
public sealed class AgentRegistry : IAgentRegistry
{
    private static readonly AgentRouting EmptyRouting = new AgentRouting(Array.Empty<AudioOutput>());

    private readonly ConcurrentDictionary<Guid, AgentInfo> agents = new ConcurrentDictionary<Guid, AgentInfo>();
    private readonly ConcurrentDictionary<string, Guid> connectionAgents = new ConcurrentDictionary<string, Guid>();
    private readonly ConcurrentDictionary<Guid, string> currentConnections = new ConcurrentDictionary<Guid, string>();

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
        return agents.TryRemove(id, out _);
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
                return updated;
            }
        }

        return null;
    }
}
