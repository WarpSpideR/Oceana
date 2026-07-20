using System.Buffers.Binary;
using NAudio.Wave;

namespace Oceana.Agent.Windows.Protocol;

/// <summary>
/// The fixed-size handshake header that the server sends once, immediately after connecting,
/// to describe the raw audio stream that follows it on the same connection.
/// </summary>
public readonly struct AudioStreamHeader
{
    /// <summary>
    /// The total size, in bytes, of the serialised header.
    /// </summary>
    public const int Size = 16;

    /// <summary>
    /// The protocol version emitted and accepted by this build.
    /// </summary>
    public const byte CurrentVersion = 1;

    private static readonly byte[] MagicBytes = "OCAP"u8.ToArray();

    /// <summary>
    /// Initialises a new instance of the <see cref="AudioStreamHeader"/> struct.
    /// </summary>
    /// <param name="encoding">How the samples following the header are encoded.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="sampleRate">The sample rate, in hertz.</param>
    /// <param name="bitsPerSample">The number of bits in a single sample.</param>
    public AudioStreamHeader(AudioEncoding encoding, int channels, int sampleRate, int bitsPerSample)
    {
        Encoding = encoding;
        Channels = channels;
        SampleRate = sampleRate;
        BitsPerSample = bitsPerSample;
    }

    /// <summary>
    /// Gets a value indicating how the samples are encoded.
    /// </summary>
    public AudioEncoding Encoding { get; }

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Gets the sample rate, in hertz.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the number of bits in a single sample.
    /// </summary>
    public int BitsPerSample { get; }

    /// <summary>
    /// Parses a header from a buffer that already contains the full <see cref="Size"/> bytes.
    /// </summary>
    /// <param name="buffer">The buffer holding the serialised header.</param>
    /// <returns>The parsed <see cref="AudioStreamHeader"/>.</returns>
    /// <exception cref="ArgumentException">The buffer is shorter than <see cref="Size"/>.</exception>
    /// <exception cref="InvalidDataException">The magic bytes or protocol version are not recognised.</exception>
    public static AudioStreamHeader Parse(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < Size)
        {
            throw new ArgumentException($"The header buffer must be at least {Size} bytes.", nameof(buffer));
        }

        if (!buffer[..MagicBytes.Length].SequenceEqual(MagicBytes))
        {
            throw new InvalidDataException("The audio stream header did not start with the expected 'OCAP' signature.");
        }

        var version = buffer[4];
        if (version != CurrentVersion)
        {
            throw new InvalidDataException($"Unsupported audio protocol version {version}; this build expects version {CurrentVersion}.");
        }

        var encoding = (AudioEncoding)buffer[5];
        var channels = BinaryPrimitives.ReadUInt16LittleEndian(buffer[6..]);
        var sampleRate = BinaryPrimitives.ReadInt32LittleEndian(buffer[8..]);
        var bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(buffer[12..]);
        return new AudioStreamHeader(encoding, channels, sampleRate, bitsPerSample);
    }

    /// <summary>
    /// Reads a complete header from a stream, reassembling it across as many reads as the transport requires.
    /// </summary>
    /// <param name="stream">The stream to read the header from.</param>
    /// <param name="cancellationToken">A token used to cancel the read.</param>
    /// <returns>The parsed <see cref="AudioStreamHeader"/>.</returns>
    /// <exception cref="EndOfStreamException">The stream ended before a full header was received.</exception>
    /// <exception cref="InvalidDataException">The magic bytes or protocol version are not recognised.</exception>
    public static async ValueTask<AudioStreamHeader> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[Size];
        await stream.ReadExactlyAsync(buffer, cancellationToken);
        return Parse(buffer);
    }

    /// <summary>
    /// Serialises this header into the start of the supplied buffer.
    /// </summary>
    /// <param name="destination">The buffer to write into; must be at least <see cref="Size"/> bytes.</param>
    /// <exception cref="ArgumentException">The destination is shorter than <see cref="Size"/>.</exception>
    public void Write(Span<byte> destination)
    {
        if (destination.Length < Size)
        {
            throw new ArgumentException($"The destination must be at least {Size} bytes.", nameof(destination));
        }

        MagicBytes.CopyTo(destination);
        destination[4] = CurrentVersion;
        destination[5] = (byte)Encoding;
        BinaryPrimitives.WriteUInt16LittleEndian(destination[6..], (ushort)Channels);
        BinaryPrimitives.WriteInt32LittleEndian(destination[8..], SampleRate);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[12..], (ushort)BitsPerSample);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[14..], 0);
    }

    /// <summary>
    /// Serialises this header and writes it to a stream.
    /// </summary>
    /// <param name="stream">The stream to write the header to.</param>
    /// <param name="cancellationToken">A token used to cancel the write.</param>
    /// <returns>A task that completes once the header has been written.</returns>
    public async ValueTask WriteToAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[Size];
        Write(buffer);
        await stream.WriteAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Maps this header to the equivalent NAudio <see cref="WaveFormat"/>.
    /// </summary>
    /// <returns>The wave format that describes the incoming samples.</returns>
    /// <exception cref="InvalidDataException">The encoding is not supported.</exception>
    public WaveFormat ToWaveFormat()
    {
        return Encoding switch
        {
            AudioEncoding.Pcm => new WaveFormat(SampleRate, BitsPerSample, Channels),
            AudioEncoding.IeeeFloat => WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, Channels),
            _ => throw new InvalidDataException($"Unsupported audio encoding '{Encoding}'."),
        };
    }
}
