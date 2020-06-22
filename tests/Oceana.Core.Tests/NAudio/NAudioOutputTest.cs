using Moq;
using NAudio.Wave;
using Xunit;

namespace Oceana.Core.Tests.NAudio
{
    public class NAudioOutputTest
    {
        private readonly Mock<IAudioSource> AudioSource;
        private readonly Mock<IWavePlayer> WavePlayer;

        public NAudioOutputTest()
        {
            AudioSource = new Mock<IAudioSource>();
            AudioSource.ConfigureDefaults();

            WavePlayer = new Mock<IWavePlayer>();
        }

        [Fact]
        public void DeviceShouldStartPlayingWhenCreated()
        {
            using var output = new NAudioOutput(WavePlayer.Object, AudioSource.Object);

            WavePlayer.Verify(m => m.Play(), Times.Once);
        }
    }
}
