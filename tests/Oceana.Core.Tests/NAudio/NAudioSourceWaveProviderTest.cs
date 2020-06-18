using Moq;
using NAudio.Wave;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests.NAudio
{
    public class NAudioSourceWaveProviderTest
    {
        private readonly Mock<IAudioSource> AudioSource;

        public NAudioSourceWaveProviderTest()
        {
            AudioSource = new Mock<IAudioSource>();
            AudioSource.ConfigureDefaults();
        }

        [Fact]
        public void WaveFormatShouldReturnCorrectFormat()
        {
            var provider = new NAudioSourceWaveProvider(AudioSource.Object);

            provider.WaveFormat.ShouldBe(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
        }

        [Fact]
        public void ReadShouldSimplyReadAudioSource()
        {
            AudioSource.Setup(m => m.Read(4))
                .Returns(new float[] { 1f, 2f, 3f, 4f });
            var provider = new NAudioSourceWaveProvider(AudioSource.Object);

            float[] buffer = new float[4];
            var samplesRead = provider.Read(buffer, 0, 4);

            buffer.ShouldBe(new float[] { 1f, 2f, 3f, 4f });
            samplesRead.ShouldBe(4);
        }

        [Fact]
        public void ReadShouldReturnCorrectCountIfSourceProvidesLessSamplesThanRequested()
        {
            AudioSource.Setup(m => m.Read(4))
                .Returns(new float[] { 1f, 2f });
            var provider = new NAudioSourceWaveProvider(AudioSource.Object);

            float[] buffer = new float[4];
            var samplesRead = provider.Read(buffer, 0, 4);

            buffer.ShouldBe(new float[] { 1f, 2f, 0f, 0f });
            samplesRead.ShouldBe(2);
        }
    }
}
