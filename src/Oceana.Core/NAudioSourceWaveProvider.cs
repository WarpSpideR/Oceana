using NAudio.Wave;

namespace Oceana
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
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(source.Format.SampleRate, source.Format.Channels);
        }

        /// <inheritdoc/>
        public WaveFormat WaveFormat { get; }

        /// <inheritdoc/>
        public int Read(float[] buffer, int offset, int count)
        {
            var result = Source.Read(count);

            return result.CopyTo(buffer, offset, count);
        }
    }
}
