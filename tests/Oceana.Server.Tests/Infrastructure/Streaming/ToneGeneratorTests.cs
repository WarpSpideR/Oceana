namespace Oceana.Server.Infrastructure.Streaming;

public class ToneGeneratorTests
{
    [Fact]
    public void Fill_WritesWholeFramesOnly()
    {
        var generator = new ToneGenerator(440.0);
        var buffer = new byte[(10 * ToneGenerator.BytesPerFrame) + 3];

        var written = generator.Fill(buffer);

        written.Should().Be(10 * ToneGenerator.BytesPerFrame);
    }

    [Fact]
    public void Fill_ProducesNonSilentAudio()
    {
        var generator = new ToneGenerator(440.0);
        var buffer = new byte[ToneGenerator.SampleRate / 10 * ToneGenerator.BytesPerFrame];

        generator.Fill(buffer);

        buffer.Should().Contain(b => b != 0);
    }

    [Fact]
    public void Fill_AdvancesPhaseAcrossCalls()
    {
        var generator = new ToneGenerator(440.0);
        var first = new byte[100 * ToneGenerator.BytesPerFrame];
        var second = new byte[100 * ToneGenerator.BytesPerFrame];

        generator.Fill(first);
        generator.Fill(second);

        // Different phase windows of a sine wave should not be byte-identical.
        second.Should().NotEqual(first);
    }
}
