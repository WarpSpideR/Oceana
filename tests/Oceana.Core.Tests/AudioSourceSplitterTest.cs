using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(2, 10));
        }

        [Fact]
        [SuppressMessage("", "CS8625", Justification = "Testing for null reference")]
        public void ShouldThrowExceptionWhenAudioSourceIsNull()
        {
            Should.Throw<ArgumentNullException>(() => _ = new AudioSourceSplitter(null));
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
        [InlineData(-2, -4)]
        public void CreateAudioSourceShouldThrowWhenChannelsOutOfBounds(int start, int end)
        {
            var splitter = new AudioSourceSplitter(AudioSource.Object);

            Should.Throw<ArgumentOutOfRangeException>(() => splitter.CreateAudioSource(start..end));
        }

        [Fact]
        public void CreateAudioSourceShouldReturnNewAudioSource()
        {
            var splitter = new AudioSourceSplitter(AudioSource.Object);

            var result = splitter.CreateAudioSource(0);

            result.ShouldBeOfType<SplitAudioSource>();
        }

        [Fact]
        public void RequestReadShouldWriteDataToSplitSources()
        {
            AudioSource.Setup(m => m.Read(2))
                .Returns(new float[] { 1f, 2f, 3f, 4f });
            var splitter = new AudioSourceSplitter(AudioSource.Object);

            var split = splitter.CreateAudioSource(0);

            splitter.RequestRead(2);

            var result = split.Read(2);

            result.Length.ShouldBe(2);
        }

        [Fact]
        public void RequestReadShouldWriteDataToCorrectChannelsForSingleChannelSource()
        {
            AudioSource.Setup(m => m.Read(2))
                .Returns(new float[] { 1f, 2f, 3f, 4f });
            var splitter = new AudioSourceSplitter(AudioSource.Object);

            var split = splitter.CreateAudioSource(0);

            splitter.RequestRead(2);

            var result = split.Read(2);

            result.ShouldBe(new float[] { 1f, 3f });
        }

        [Fact]
        public void RequestReadShouldWriteDataToCorrectChannelsForMultiChannelSource()
        {
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(4, 10));
            AudioSource.Setup(m => m.Read(2))
                .Returns(new float[] { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f });
            var splitter = new AudioSourceSplitter(AudioSource.Object);

            var split = splitter.CreateAudioSource(0..1);

            splitter.RequestRead(2);

            var result = split.Read(2);

            result.ShouldBe(new float[] { 1f, 2f, 5f, 6f });
        }

        [Fact]
        public void RequestReadShouldWriteDataToCorrectChannelsForSwappedMultiChannelSource()
        {
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(4, 10));
            AudioSource.Setup(m => m.Read(2))
                .Returns(new float[] { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f });
            var splitter = new AudioSourceSplitter(AudioSource.Object);

            var split = splitter.CreateAudioSource(new List<int> { 1, 0 });

            splitter.RequestRead(2);

            var result = split.Read(2);

            result.ShouldBe(new float[] { 2f, 1f, 6f, 5f });
        }
    }
}
