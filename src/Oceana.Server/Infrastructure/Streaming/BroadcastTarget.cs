namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// An agent the broadcast is playing to.
/// </summary>
/// <param name="AgentId">The agent identifier.</param>
/// <param name="AgentName">The agent name.</param>
/// <param name="DeviceCount">The number of the agent's devices the message plays on.</param>
public sealed record BroadcastTarget(Guid AgentId, string AgentName, int DeviceCount);
