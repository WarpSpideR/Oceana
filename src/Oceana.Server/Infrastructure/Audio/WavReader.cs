using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Oceana.Protocol;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Infrastructure.Audio;

/// <summary>
/// Parses uncompressed PCM WAV (RIFF/WAVE) audio. Only the format the agent can play —
/// mono or stereo, 48 kHz, 16-bit PCM — is accepted, so the server needs no audio codec.
/// </summary>
public static class WavReader
{
    private const int MaxChannels = 2;
    private const int RequiredSampleRate = 48000;
    private const int RequiredBitsPerSample = 16;
    private const ushort WaveFormatPcm = 1;

    /// <summary>
    /// Attempts to parse and validate a WAV buffer.
    /// </summary>
    /// <param name="bytes">The complete WAV file bytes.</param>
    /// <param name="audio">The decoded audio when parsing succeeds.</param>
    /// <param name="error">A human-readable reason when parsing fails.</param>
    /// <returns>True when the buffer is a supported WAV file; otherwise false.</returns>
    public static bool TryRead(
        ReadOnlySpan<byte> bytes,
        [NotNullWhen(true)] out WavAudio? audio,
        [NotNullWhen(false)] out string? error)
    {
        audio = null;

        if (bytes.Length < 12 || !bytes[..4].SequenceEqual("RIFF"u8) || !bytes.Slice(8, 4).SequenceEqual("WAVE"u8))
        {
            error = "The uploaded audio is not a WAV file.";
            return false;
        }

        var haveFormat = false;
        ushort audioFormat = 0;
        int channels = 0;
        int sampleRate = 0;
        int bitsPerSample = 0;
        byte[]? pcm = null;

        var offset = 12;
        while (offset + 8 <= bytes.Length)
        {
            var chunkId = bytes.Slice(offset, 4);
            var chunkSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset + 4, 4));
            var chunkStart = offset + 8;
            if (chunkStart + chunkSize > (uint)bytes.Length)
            {
                // Truncated chunk; use whatever data remains for a data chunk.
                chunkSize = (uint)(bytes.Length - chunkStart);
            }

            if (chunkId.SequenceEqual("fmt "u8) && chunkSize >= 16)
            {
                var fmt = bytes.Slice(chunkStart, 16);
                audioFormat = BinaryPrimitives.ReadUInt16LittleEndian(fmt[..2]);
                channels = BinaryPrimitives.ReadUInt16LittleEndian(fmt.Slice(2, 2));
                sampleRate = BinaryPrimitives.ReadInt32LittleEndian(fmt.Slice(4, 4));
                bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(fmt.Slice(14, 2));
                haveFormat = true;
            }
            else if (chunkId.SequenceEqual("data"u8))
            {
                pcm = bytes.Slice(chunkStart, (int)chunkSize).ToArray();
            }

            // Chunks are word-aligned: an odd size is followed by a pad byte.
            offset = chunkStart + (int)chunkSize + ((chunkSize % 2 == 0) ? 0 : 1);
        }

        if (!haveFormat || pcm is null)
        {
            error = "The WAV file is missing its format or data.";
            return false;
        }

        if (audioFormat != WaveFormatPcm
            || channels < 1
            || channels > MaxChannels
            || sampleRate != RequiredSampleRate
            || bitsPerSample != RequiredBitsPerSample)
        {
            error = "Only mono or stereo, 48 kHz, 16-bit PCM audio is supported.";
            return false;
        }

        if (pcm.Length == 0)
        {
            error = "The audio contains no samples.";
            return false;
        }

        audio = new WavAudio(
            new StreamFormat(AudioEncoding.Pcm, channels, RequiredSampleRate, RequiredBitsPerSample),
            pcm);
        error = null;
        return true;
    }
}
