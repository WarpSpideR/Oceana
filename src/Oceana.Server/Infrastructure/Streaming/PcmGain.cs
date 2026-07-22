using System.Buffers.Binary;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Applies a linear gain to interleaved little-endian 16-bit PCM samples.
/// </summary>
public static class PcmGain
{
    /// <summary>
    /// Scales each 16-bit sample in <paramref name="source"/> by <paramref name="gain"/>, writing the
    /// result to <paramref name="destination"/> with clamping to the 16-bit range.
    /// </summary>
    /// <param name="source">The source PCM bytes (whole 16-bit samples).</param>
    /// <param name="destination">The destination buffer; must be at least as long as the source.</param>
    /// <param name="gain">The linear gain to apply.</param>
    public static void ApplyInt16(ReadOnlySpan<byte> source, Span<byte> destination, double gain)
    {
        for (var offset = 0; offset + 1 < source.Length; offset += 2)
        {
            var sample = BinaryPrimitives.ReadInt16LittleEndian(source[offset..]);
            var scaled = (int)Math.Round(sample * gain);
            var clamped = (short)Math.Clamp(scaled, short.MinValue, short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(destination[offset..], clamped);
        }
    }
}
