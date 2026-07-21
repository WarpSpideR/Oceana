namespace Oceana.Contracts;

/// <summary>
/// The server→agent control methods a connected agent implements (via its SignalR client).
/// </summary>
public interface IAgentControlClient
{
    /// <summary>
    /// Pushes updated routing to the agent, applied on its next stream.
    /// </summary>
    /// <param name="routing">The routing the agent should adopt.</param>
    /// <returns>A task representing the client invocation.</returns>
    Task SetRouting(AgentRouting routing);
}
