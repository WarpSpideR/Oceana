namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// The result of classifying a zone's agents for a fan-out: those to play to and those skipped.
/// </summary>
/// <param name="Targets">The reachable agents and their target devices.</param>
/// <param name="Skips">The agents that were skipped, with reasons.</param>
public sealed record ZoneTargetingResult(
    IReadOnlyList<ZoneTarget> Targets,
    IReadOnlyList<BroadcastSkip> Skips);
