using System.Buffers.Binary;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Generates interleaved 16-bit PCM stereo samples for a continuous sine tone.
/// </summary>
public sealed class ToneGenerator
{
    /// <summary>
    /// The sample rate, in hertz, of the generated tone.
    /// </summary>
    public const int SampleRate = 48000;

    /// <summary>
    /// The number of interleaved channels in the generated tone.
    /// </summary>
    public const int Channels = 2;

    /// <summary>
    /// The number of bits per sample in the generated tone.
    /// </summary>
    public const int BitsPerSample = 16;

    /// <summary>
    /// The number of bytes occupied by a single frame (one sample per channel).
    /// </summary>
    public const int BytesPerFrame = Channels * (BitsPerSample / 8);

    private const double Amplitude = 0.25;

    private readonly double frequency;
    private long frame;

    /// <summary>
    /// Initialises a new instance of the <see cref="ToneGenerator"/> class.
    /// </summary>
    /// <param name="frequency">The tone frequency, in hertz.</param>
    public ToneGenerator(double frequency)
    {
        this.frequency = frequency;
    }

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
            var sample = (short)(Math.Sin(2.0 * Math.PI * frequency * t) * Amplitude * short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(buffer[offset..], sample);
            offset += 2;
            BinaryPrimitives.WriteInt16LittleEndian(buffer[offset..], sample);
            offset += 2;
            frame++;
        }

        return offset;
    }
}
