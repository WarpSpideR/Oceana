using Oceana.Protocol;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Describes the PCM (or float) format of an audio stream sent to an agent.
/// </summary>
/// <param name="Encoding">The sample encoding.</param>
/// <param name="Channels">The number of interleaved channels.</param>
/// <param name="SampleRate">The sample rate, in hertz.</param>
/// <param name="BitsPerSample">The number of bits per sample.</param>
public sealed record StreamFormat(AudioEncoding Encoding, int Channels, int SampleRate, int BitsPerSample)
{
    /// <summary>
    /// Gets the number of bytes occupied by a single frame (one sample per channel).
    /// </summary>
    public int BytesPerFrame => Channels * (BitsPerSample / 8);

    /// <summary>
    /// Builds the OCAP handshake header describing this format.
    /// </summary>
    /// <returns>The audio stream header.</returns>
    public AudioStreamHeader ToHeader() => new AudioStreamHeader(Encoding, Channels, SampleRate, BitsPerSample);
}
