using Oceana.Contracts;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// Pushes control commands to connected agents.
/// </summary>
public interface IAgentRoutingCommander
{
    /// <summary>
    /// Pushes routing to a specific agent. No-op if the agent is not currently connected.
    /// </summary>
    /// <param name="agentId">The identifier of the target agent.</param>
    /// <param name="routing">The routing to push.</param>
    /// <returns>A task that completes once the push has been dispatched.</returns>
    Task PushRoutingAsync(Guid agentId, AgentRouting routing);
}
