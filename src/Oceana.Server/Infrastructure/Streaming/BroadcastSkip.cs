namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// An agent in the zone that the broadcast skipped, with the reason.
/// </summary>
/// <param name="AgentId">The agent identifier.</param>
/// <param name="AgentName">The agent name, or its id when unknown.</param>
/// <param name="Reason">Why the agent was skipped.</param>
public sealed record BroadcastSkip(Guid AgentId, string AgentName, BroadcastSkipReason Reason);
