using NAudio.Wave;
using Oceana.Protocol;

namespace Oceana.Agent.Windows.Playback;

public class AudioStreamHeaderExtensionsTests
{
    [Fact]
    public void ToWaveFormat_Pcm_MapsToPcmWaveFormat()
    {
        var header = new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16);

        var format = header.ToWaveFormat();

        format.Encoding.Should().Be(WaveFormatEncoding.Pcm);
        format.SampleRate.Should().Be(48000);
        format.Channels.Should().Be(2);
        format.BitsPerSample.Should().Be(16);
    }

    [Fact]
    public void ToWaveFormat_IeeeFloat_MapsToFloatWaveFormat()
    {
        var header = new AudioStreamHeader(AudioEncoding.IeeeFloat, 2, 48000, 32);

        var format = header.ToWaveFormat();

        format.Encoding.Should().Be(WaveFormatEncoding.IeeeFloat);
        format.SampleRate.Should().Be(48000);
        format.Channels.Should().Be(2);
    }
}
