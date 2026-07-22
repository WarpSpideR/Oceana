using Oceana.Server.Features.Zones;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Plays a finite PCM source to every reachable device in a zone, with start/stop and now-playing
/// state. At most one playback is active per zone; starting a new one replaces the current.
/// </summary>
public interface IZonePlaybackService
{
    /// <summary>
    /// Starts playing the source to the zone (replacing any current playback) and returns the
    /// resulting state immediately; streaming continues in the background.
    /// </summary>
    /// <param name="zone">The zone to play to.</param>
    /// <param name="sourceName">A human-readable name for the source (e.g. the file name).</param>
    /// <param name="pcm">The interleaved PCM samples to play.</param>
    /// <param name="format">The format of the PCM buffer.</param>
    /// <returns>The playback state (agents targeted and skipped).</returns>
    ZonePlaybackState Start(ZoneInfo zone, string sourceName, ReadOnlyMemory<byte> pcm, StreamFormat format);

    /// <summary>
    /// Stops the zone's playback, if any.
    /// </summary>
    /// <param name="zoneId">The zone.</param>
    /// <returns>True when playback was stopped; false when nothing was playing.</returns>
    bool Stop(Guid zoneId);

    /// <summary>
    /// Gets the zone's current playback state, or null when nothing is playing.
    /// </summary>
    /// <param name="zoneId">The zone.</param>
    /// <returns>The current playback state, or null.</returns>
    ZonePlaybackState? Get(Guid zoneId);
}
