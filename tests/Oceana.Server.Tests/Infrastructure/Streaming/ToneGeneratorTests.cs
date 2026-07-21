using System.Buffers.Binary;

namespace Oceana.Server.Infrastructure.Streaming;

public class ToneGeneratorTests
{
    [Fact]
    public void Fill_WritesWholeFramesOnly()
    {
        var generator = new ToneGenerator(440.0, 2);
        var buffer = new byte[(10 * generator.BytesPerFrame) + 3];

        var written = generator.Fill(buffer);

        written.Should().Be(10 * generator.BytesPerFrame);
    }

    [Fact]
    public void Fill_ProducesNonSilentAudio()
    {
        var generator = new ToneGenerator(440.0, 2);
        var buffer = new byte[generator.BytesPerFrame * (ToneGenerator.SampleRate / 10)];

        generator.Fill(buffer);

        buffer.Should().Contain(b => b != 0);
    }

    [Fact]
    public void Constructor_SetsChannelCountAndFrameSize()
    {
        var generator = new ToneGenerator(440.0, 4);

        generator.Channels.Should().Be(4);
        generator.BytesPerFrame.Should().Be(4 * (ToneGenerator.BitsPerSample / 8));
    }

    [Fact]
    public void Fill_ProducesADistinctFrequencyPerChannel()
    {
        var generator = new ToneGenerator(440.0, 2);
        var buffer = new byte[generator.BytesPerFrame * 100];

        generator.Fill(buffer);

        // At a non-zero position, channel 0 (440 Hz) and channel 1 (880 Hz) produce different samples.
        var frame = 50 * generator.BytesPerFrame;
        var channel0 = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(frame));
        var channel1 = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(frame + 2));
        channel0.Should().NotBe(channel1);
    }
}
