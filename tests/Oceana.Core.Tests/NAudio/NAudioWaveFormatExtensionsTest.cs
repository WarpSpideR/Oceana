using NAudio.Wave;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests.NAudio
{
    public class NAudioWaveFormatExtensionsTest
    {
        [Fact]
        public void ToAudioFormatShouldReturnCorrectSampleRate()
        {
            var result = WaveFormat.CreateIeeeFloatWaveFormat(100, 2).ToAudioFormat();

            result.SampleRate.ShouldBe(100);
        }

        [Fact]
        public void ToAudioFormatShouldReturnCorrectChannelCount()
        {
            var result = WaveFormat.CreateIeeeFloatWaveFormat(100, 2).ToAudioFormat();

            ((int)result.Channels).ShouldBe(2);
        }
    }
}
