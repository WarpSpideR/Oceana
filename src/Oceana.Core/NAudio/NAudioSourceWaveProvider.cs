using NAudio.Wave;

namespace Oceana.Core
{
    /// <summary>
    /// Adapter for an <see cref="IAudioSource"/> to be compatible
    /// with an <see cref="IWaveProvider"/>.
    /// </summary>
    internal class NAudioSourceWaveProvider : ISampleProvider
    {
        private readonly IAudioSource Source;

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioSourceWaveProvider"/> class.
        /// </summary>
        /// <param name="source"><see cref="IAudioSource"/> to adapt.</param>
        public NAudioSourceWaveProvider(IAudioSource source)
        {
            Source = source;
        }

        /// <inheritdoc/>
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        /// <inheritdoc/>
        public int Read(float[] buffer, int offset, int count)
        {
            var result = Source.Read(count);

            return result.CopyTo(buffer, offset, count);
        }
    }
}
