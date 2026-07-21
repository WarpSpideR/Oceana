using System.Buffers.Binary;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Generates interleaved 16-bit PCM samples for a continuous multi-channel sine tone, with a
/// distinct frequency on each channel so individual channels can be told apart on playback.
/// </summary>
public sealed class ToneGenerator
{
    /// <summary>
    /// The sample rate, in hertz, of the generated tone.
    /// </summary>
    public const int SampleRate = 48000;

    /// <summary>
    /// The number of bits per sample in the generated tone.
    /// </summary>
    public const int BitsPerSample = 16;

    private const int BytesPerSample = BitsPerSample / 8;
    private const double Amplitude = 0.25;

    private readonly double baseFrequency;
    private readonly int channels;
    private long frame;

    /// <summary>
    /// Initialises a new instance of the <see cref="ToneGenerator"/> class.
    /// </summary>
    /// <param name="baseFrequency">The frequency, in hertz, of channel 0; channel n uses <paramref name="baseFrequency"/> multiplied by (n + 1).</param>
    /// <param name="channels">The number of interleaved channels to generate.</param>
    public ToneGenerator(double baseFrequency, int channels)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        this.baseFrequency = baseFrequency;
        this.channels = channels;
    }

    /// <summary>
    /// Gets the number of interleaved channels produced.
    /// </summary>
    public int Channels => channels;

    /// <summary>
    /// Gets the number of bytes occupied by a single frame (one sample per channel).
    /// </summary>
    public int BytesPerFrame => channels * BytesPerSample;

    /// <summary>
    /// Fills the buffer with the next block of samples, advancing the generator's position.
    /// </summary>
    /// <param name="buffer">The destination buffer; whole frames only are written.</param>
    /// <returns>The number of bytes written, always a whole number of frames.</returns>
    public int Fill(Span<byte> buffer)
    {
        var frames = buffer.Length / BytesPerFrame;
        var offset = 0;
        for (var i = 0; i < frames; i++)
        {
            var t = (double)frame / SampleRate;
            for (var channel = 0; channel < channels; channel++)
            {
                var frequency = baseFrequency * (channel + 1);
                var sample = (short)(Math.Sin(2.0 * Math.PI * frequency * t) * Amplitude * short.MaxValue);
                BinaryPrimitives.WriteInt16LittleEndian(buffer[offset..], sample);
                offset += BytesPerSample;
            }

            frame++;
        }

        return offset;
    }
}
