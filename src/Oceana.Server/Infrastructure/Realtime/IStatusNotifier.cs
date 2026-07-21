using Oceana.Server.Features.Agents;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// Broadcasts agent state changes to connected clients.
/// </summary>
public interface IStatusNotifier
{
    /// <summary>
    /// Notifies clients that an agent was registered or changed state.
    /// </summary>
    /// <param name="agent">The updated agent snapshot.</param>
    /// <returns>A task that completes once the notification has been dispatched.</returns>
    Task NotifyAgentChangedAsync(AgentInfo agent);

    /// <summary>
    /// Notifies clients that an agent was removed.
    /// </summary>
    /// <param name="agentId">The identifier of the removed agent.</param>
    /// <returns>A task that completes once the notification has been dispatched.</returns>
    Task NotifyAgentRemovedAsync(Guid agentId);
}
