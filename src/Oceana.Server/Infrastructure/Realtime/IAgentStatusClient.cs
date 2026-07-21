using Oceana.Server.Features.Agents;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// The strongly-typed client methods a connected front end implements to receive agent updates.
/// </summary>
public interface IAgentStatusClient
{
    /// <summary>
    /// Called when an agent is registered or changes state.
    /// </summary>
    /// <param name="agent">The updated agent snapshot.</param>
    /// <returns>A task representing the client invocation.</returns>
    Task AgentChanged(AgentInfo agent);

    /// <summary>
    /// Called when an agent is removed from the registry.
    /// </summary>
    /// <param name="agentId">The identifier of the removed agent.</param>
    /// <returns>A task representing the client invocation.</returns>
    Task AgentRemoved(Guid agentId);
}
