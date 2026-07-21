using NAudio.Wave;
using Oceana.Protocol;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// NAudio-specific extensions that bridge the transport-level <see cref="AudioStreamHeader"/> to a playback format.
/// </summary>
public static class AudioStreamHeaderExtensions
{
    /// <summary>
    /// Maps a handshake header to the equivalent NAudio <see cref="WaveFormat"/>.
    /// </summary>
    /// <param name="header">The negotiated stream header.</param>
    /// <returns>The wave format that describes the incoming samples.</returns>
    /// <exception cref="InvalidDataException">The encoding is not supported.</exception>
    public static WaveFormat ToWaveFormat(this AudioStreamHeader header)
    {
        return header.Encoding switch
        {
            AudioEncoding.Pcm => new WaveFormat(header.SampleRate, header.BitsPerSample, header.Channels),
            AudioEncoding.IeeeFloat => WaveFormat.CreateIeeeFloatWaveFormat(header.SampleRate, header.Channels),
            _ => throw new InvalidDataException($"Unsupported audio encoding '{header.Encoding}'."),
        };
    }
}
