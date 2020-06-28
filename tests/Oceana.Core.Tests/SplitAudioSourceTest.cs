using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public class SplitAudioSourceTest
    {
        private readonly Mock<IAudioSource> AudioSource;
        private readonly AudioSourceSplitter AudioSplitter;

        public SplitAudioSourceTest()
        {
            AudioSource = new Mock<IAudioSource>();
            AudioSource.ConfigureDefaults();
            AudioSplitter = new AudioSourceSplitter(AudioSource.Object);
        }

        [Fact]
        public void FormatShouldHaveSameSampleRateAsParentSource()
        {
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(8, 100));
            var audioSplitter = new AudioSourceSplitter(AudioSource.Object);

            var splitter = new SplitAudioSource(audioSplitter, 2);

            splitter.Format.SampleRate.ShouldBe(100);
        }

        [Fact]
        public void FormatShouldHaveCorrectChannelCount()
        {
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(8, 100));
            var audioSplitter = new AudioSourceSplitter(AudioSource.Object);

            var splitter = new SplitAudioSource(audioSplitter, 2);

            ((int)splitter.Format.Channels).ShouldBe(2);
        }

        [Fact]
        public void WriteShouldWriteToBuffer()
        {
            var splitter = new SplitAudioSource(AudioSplitter, 2);
            
            splitter.Write(new float[] { 1f, 2f, 3f });

            var result = splitter.Read(3);
            result.ShouldBe(new float[] { 1f, 2f, 3f });
        }

        [Fact]
        public void ReadShouldNotRequestSamplesFromParentIfBufferIsPopulated()
        {
            var splitter = new SplitAudioSource(AudioSplitter, 1);
            splitter.Write(new float[] { 1f, 2f, 3f, 4f, 5f, 6f });

            var result = splitter.Read(2);

            result.ShouldBe(new float[] { 1f, 2f });
            AudioSource.Verify(m => m.Read(2), Times.Never);
        }

        [Fact]
        public void ReadShouldRequestSamplesFromParentSource()
        {
            var splitter = new SplitAudioSource(AudioSplitter, 2);

            _ = splitter.Read(5);

            AudioSource.Verify(m => m.Read(5), Times.Once);
        }

        [Fact]
        public void ReadShouldReturnCorrectNumberOfSamplesForChannels()
        {
            var splitter = new SplitAudioSource(AudioSplitter, 2);
            splitter.Write(new float[20]);

            var result = splitter.Read(5);

            result.Length.ShouldBe(10);
        }
    }
}
