namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// The current playback state of a zone: what (if anything) is playing and to which agents.
/// </summary>
/// <param name="ZoneId">The zone.</param>
/// <param name="Playing">Whether audio is currently playing to the zone.</param>
/// <param name="SourceName">The name of the source being played, when playing.</param>
/// <param name="StartedAtUtc">When playback started, when playing.</param>
/// <param name="Targeted">The agents being played to.</param>
/// <param name="Skipped">The agents that were skipped when playback started, with reasons.</param>
public sealed record ZonePlaybackState(
    Guid ZoneId,
    bool Playing,
    string? SourceName,
    DateTimeOffset? StartedAtUtc,
    IReadOnlyList<BroadcastTarget> Targeted,
    IReadOnlyList<BroadcastSkip> Skipped)
{
    /// <summary>
    /// Creates an idle (nothing playing) state for a zone.
    /// </summary>
    /// <param name="zoneId">The zone.</param>
    /// <returns>An idle state.</returns>
    public static ZonePlaybackState Idle(Guid zoneId) =>
        new ZonePlaybackState(zoneId, false, null, null, Array.Empty<BroadcastTarget>(), Array.Empty<BroadcastSkip>());
}
