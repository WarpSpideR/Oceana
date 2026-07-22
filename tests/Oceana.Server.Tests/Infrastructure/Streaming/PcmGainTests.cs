using System.Buffers.Binary;

namespace Oceana.Server.Infrastructure.Streaming;

public class PcmGainTests
{
    [Fact]
    public void ApplyInt16_AtUnity_LeavesSamplesUnchanged()
    {
        var source = Pcm(100, -200, 32767, -32768);
        var destination = new byte[source.Length];

        PcmGain.ApplyInt16(source, destination, 1.0);

        Samples(destination).Should().Equal(100, -200, 32767, -32768);
    }

    [Fact]
    public void ApplyInt16_AtZero_SilencesSamples()
    {
        var source = Pcm(100, -200, 32767, -32768);
        var destination = new byte[source.Length];

        PcmGain.ApplyInt16(source, destination, 0.0);

        Samples(destination).Should().Equal(0, 0, 0, 0);
    }

    [Fact]
    public void ApplyInt16_AtHalf_HalvesSamples()
    {
        var source = Pcm(1000, -1000, 20000);
        var destination = new byte[source.Length];

        PcmGain.ApplyInt16(source, destination, 0.5);

        Samples(destination).Should().Equal(500, -500, 10000);
    }

    [Fact]
    public void ApplyInt16_ClampsToInt16Range()
    {
        // Gain > 1 is not used by the app, but the helper must still clamp rather than overflow.
        var source = Pcm(20000, -20000);
        var destination = new byte[source.Length];

        PcmGain.ApplyInt16(source, destination, 4.0);

        Samples(destination).Should().Equal(32767, -32768);
    }

    private static byte[] Pcm(params short[] samples)
    {
        var bytes = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), samples[i]);
        }

        return bytes;
    }

    private static short[] Samples(byte[] bytes)
    {
        var samples = new short[bytes.Length / 2];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(i * 2));
        }

        return samples;
    }
}
