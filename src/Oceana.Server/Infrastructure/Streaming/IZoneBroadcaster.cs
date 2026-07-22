using Oceana.Server.Features.Zones;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Broadcasts a recorded PCM message to every reachable device in a zone.
/// </summary>
public interface IZoneBroadcaster
{
    /// <summary>
    /// Plans the broadcast (deciding which agents to play to and which to skip) and starts
    /// streaming to the reachable agents in the background.
    /// </summary>
    /// <param name="zone">The zone to broadcast to.</param>
    /// <param name="pcm">The interleaved PCM samples of the message.</param>
    /// <param name="format">The format of the PCM buffer.</param>
    /// <returns>A summary of the agents targeted and skipped.</returns>
    ZoneBroadcastResult PlanAndStart(ZoneInfo zone, ReadOnlyMemory<byte> pcm, StreamFormat format);
}
