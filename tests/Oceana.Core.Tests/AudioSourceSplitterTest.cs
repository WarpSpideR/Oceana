using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public class AudioSourceSplitterTest
    {
        private readonly Mock<IAudioSource> AudioSource;

        public AudioSourceSplitterTest()
        {
            AudioSource = new Mock<IAudioSource>();
            AudioSource.ConfigureDefaults();
        }

        [Fact]
        public void CreateAudioSourceShouldIncreaseCount()
        {
            var splitter = new AudioSourceSplitter(AudioSource.Object);

            splitter.Count.ShouldBe(0);

            splitter.CreateAudioSource(0..0);

            splitter.Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(-1, 1)]
        [InlineData(-5, 5)]
        public void CreateAudioSourceShouldThrowWhenChannelsOutOfBounds(int start, int end)
        {
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(2, 10));

            var splitter = new AudioSourceSplitter(AudioSource.Object);

            Should.Throw<ArgumentOutOfRangeException>(() => splitter.CreateAudioSource(start..end));
        }

        public void GetAudioSourceShouldReturnSourceWithCorrectChannelWidth()
        {
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(2, 10));

            var splitter = new AudioSourceSplitter(AudioSource.Object);

            splitter.CreateAudioSource(0);

            var result = splitter.GetAudioSource(0);

            ((int)result.Format.Channels).ShouldBe(1);
        } 
    }
}
