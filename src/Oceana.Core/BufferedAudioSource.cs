using System;

namespace Oceana
{
    /// <summary>
    /// Provides a means to buffer an incomming <see cref="IAudioSource"/>.
    /// </summary>
    public class BufferedAudioSource : IAudioSource
    {
        private readonly IAudioSource Source;
        private readonly CircularBuffer<float> Buffer;

        /// <summary>
        /// Initialises a new instance of the <see cref="BufferedAudioSource"/> class.
        /// </summary>
        /// <param name="source">The <see cref="IAudioSource"/> to buffer.</param>
        public BufferedAudioSource(IAudioSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Format = source.Format;

            Buffer = new CircularBuffer<float>(source.Format.SampleRate * source.Format.Channels);
        }

        /// <inheritdoc/>
        public AudioFormat Format { get; }

        /// <inheritdoc/>
        public float[] Read(int samples)
        {
            if (samples > Buffer.ItemsAvailable)
            {
                var data = Source.Read(samples - Buffer.ItemsAvailable);
                Buffer.Write(data);
            }

            return Buffer.Read(samples);
        }

        /// <summary>
        /// Write data to the buffer.
        /// </summary>
        /// <param name="data">The data to write to the buffer.</param>
        /// <returns>The number of samples that were written to the buffer.</returns>
        public int Write(float[] data)
            => Buffer.Write(data);
    }
}
