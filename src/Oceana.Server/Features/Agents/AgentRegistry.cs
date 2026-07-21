using System.Collections.Concurrent;

namespace Oceana.Server.Features.Agents;

/// <summary>
/// An in-memory <see cref="IAgentRegistry"/> backed by a concurrent dictionary.
/// </summary>
public sealed class AgentRegistry : IAgentRegistry
{
    private readonly ConcurrentDictionary<Guid, AgentInfo> agents = new ConcurrentDictionary<Guid, AgentInfo>();

    /// <inheritdoc/>
    public AgentInfo Register(string name, string host, int port)
    {
        var agent = new AgentInfo
        {
            Id = Guid.NewGuid(),
            Name = name,
            Host = host,
            Port = port,
            Status = AgentStatus.Idle,
        };

        agents[agent.Id] = agent;
        return agent;
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
}
