namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// The outcome of planning a zone broadcast: which agents it is playing to and which were skipped.
/// </summary>
/// <param name="ZoneId">The zone that was broadcast to.</param>
/// <param name="Targeted">The agents the message is playing to.</param>
/// <param name="Skipped">The agents that were skipped, with reasons.</param>
public sealed record ZoneBroadcastResult(
    Guid ZoneId,
    IReadOnlyList<BroadcastTarget> Targeted,
    IReadOnlyList<BroadcastSkip> Skipped);
