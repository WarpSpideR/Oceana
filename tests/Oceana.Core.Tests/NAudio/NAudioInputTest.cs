using Moq;
using NAudio.Wave;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests.NAudio
{
    public class NAudioInputTest
    {
        private readonly Mock<IWaveIn> WaveIn;
        private readonly Mock<ISampleProvider> Provider;

        public NAudioInputTest()
        {
            Provider = new Mock<ISampleProvider>();
            WaveIn = new Mock<IWaveIn>();
            WaveIn.Setup(m => m.WaveFormat)
                .Returns(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
        }

        [Fact]
        public void AudioFormatShouldReportAsFloatingPoint()
        {
            using var input = new NAudioInput(WaveIn.Object, Provider.Object);

            input.Format.ShouldBe(new AudioFormat(2, 44100));
        }

        [Fact]
        public void ReadShouldReturnRequestedAmountOfDataIfAvailable()
        {
            Provider.Setup(m => m.Read(It.IsAny<float[]>(), 0, 2))
                .Callback((float[] data, int offset, int count) =>
                {
                    data[0] = 1f;
                    data[1] = 1f;
                })
                .Returns(2);

            using var input = new NAudioInput(WaveIn.Object, Provider.Object);

            var result = input.Read(1);

            result.ShouldBe(new float[] { 1f, 1f });
        }

        [Fact]
        public void ReadShouldReturnAsMuchDataAsPossible()
        {
            // Provider can only return 2 samples not 4
            Provider.Setup(m => m.Read(It.IsAny<float[]>(), 0, 4))
                .Callback((float[] data, int offset, int count) =>
                {
                    data[0] = 1f;
                    data[1] = 1f;
                })
                .Returns(2);

            using var input = new NAudioInput(WaveIn.Object, Provider.Object);

            var result = input.Read(2);

            result.ShouldBe(new float[] { 1f, 1f });
        }
    }
}
