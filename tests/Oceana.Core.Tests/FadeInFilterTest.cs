using System;
using Moq;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public class FadeInFilterTest
    {
        private readonly Mock<IAudioSource> AudioSourceMock;

        public FadeInFilterTest()
        {
            AudioSourceMock = new Mock<IAudioSource>();
            AudioSourceMock.ConfigureDefaults();
        }

        [Fact]
        public void ShouldBeCreatedWithSameAudioFormat()
        {
            AudioSourceMock.Setup(m => m.Format)
                .Returns(new AudioFormat(2, 44100));

            var filter = new FadeInFilter(AudioSourceMock.Object);

            filter.Format.ShouldBe(new AudioFormat(2, 44100));
        }

        [Fact]
        public void ShouldFadeInAsExpected()
        {
            AudioSourceMock.Setup(m => m.Format)
                .Returns(new AudioFormat(1, 1));

            var filter = new FadeInFilter(AudioSourceMock.Object);
            filter.Duration = TimeSpan.FromSeconds(5);

            var result = filter.Read(10);

            result[0].ShouldBe(0f);
            result[1].ShouldBe(0.2f);
            result[2].ShouldBe(0.4f);
            result[3].ShouldBe(0.6f);
            result[4].ShouldBe(0.8f);
            result[5].ShouldBe(1f);
        }

        [Fact]
        public void ShouldFadeInMultipleChannels()
        {
            AudioSourceMock.Setup(m => m.Format)
                .Returns(new AudioFormat(2, 1));

            var filter = new FadeInFilter(AudioSourceMock.Object);
            filter.Duration = TimeSpan.FromSeconds(5);

            var result = filter.Read(10);

            result[0].ShouldBe(0f);
            result[1].ShouldBe(0f);
            result[2].ShouldBe(0.2f);
            result[3].ShouldBe(0.2f);
            result[4].ShouldBe(0.4f);
            result[5].ShouldBe(0.4f);
            result[6].ShouldBe(0.6f);
            result[7].ShouldBe(0.6f);
            result[8].ShouldBe(0.8f);
            result[9].ShouldBe(0.8f);
            result[10].ShouldBe(1f);
            result[11].ShouldBe(1f);
        }

        [Fact]
        public void ResetShouldWorkCorrectly()
        {
            AudioSourceMock.Setup(m => m.Format)
                .Returns(new AudioFormat(1, 1));

            var filter = new FadeInFilter(AudioSourceMock.Object);
            filter.Duration = TimeSpan.FromSeconds(5);

            var result = filter.Read(10);
            result[9].ShouldBe(1f);

            filter.Reset();
            result = filter.Read(10);

            result[0].ShouldBe(0f);
            result[1].ShouldBe(0.2f);
            result[2].ShouldBe(0.4f);
            result[3].ShouldBe(0.6f);
            result[4].ShouldBe(0.8f);
            result[5].ShouldBe(1f);
        }
    }
}
