using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public class BufferedAudioSourceTest
    {
        private readonly Mock<IAudioSource> AudioSource;

        public BufferedAudioSourceTest()
        {
            AudioSource = new Mock<IAudioSource>();
            AudioSource.ConfigureDefaults();
        }

        [Fact]
        [SuppressMessage("", "CS8625", Justification = "Testing for null reference.")]
        public void ConstructorShouldThrowExceptionWhenAudioSourceNull()
        {
            _ = Should.Throw<ArgumentNullException>(() => _ = new BufferedAudioSource(null));
        }

        [Fact]
        public void FormatShouldReturnSameFormatAsSource()
        {
            AudioSource.Setup(m => m.Format)
                .Returns(new AudioFormat(2, 10));

            var filter = new BufferedAudioSource(AudioSource.Object);

            filter.Format.ShouldBe(new AudioFormat(2, 10));
        }

        [Fact]
        public void ReadShouldReturnAllSamplesIfAvailable()
        {
            var filter = new BufferedAudioSource(AudioSource.Object);

            filter.Write(new float[] { 1f, 2f, 3f });

            var result = filter.Read(2);

            result.SequenceEqual(new float[] { 1f, 2f }).ShouldBeTrue();
        }

        [Fact]
        public void ReadShouldNotQuerySourceIfAllSamplesAvailable()
        {
            var filter = new BufferedAudioSource(AudioSource.Object);

            filter.Write(new float[] { 1f, 2f, 3f });

            _ = filter.Read(2);

            AudioSource.Verify(m => m.Read(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ReadShouldQueryAudioSourceIfBufferNotSufficient()
        {
            var filter = new BufferedAudioSource(AudioSource.Object);

            filter.Write(new float[] { 1f, 2f, 3f });

            _ = filter.Read(10);

            AudioSource.Verify(m => m.Read(7), Times.Once);
        }
    }
}
