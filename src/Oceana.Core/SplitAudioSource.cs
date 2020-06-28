using System;

namespace Oceana.Core
{
    /// <summary>
    /// An <see cref="IAudioSource"/> that has been split
    /// from a wider <see cref="IAudioSource"/>.
    /// </summary>
    internal class SplitAudioSource : IAudioSource
    {
        private readonly AudioSourceSplitter Parent;
        private readonly CircularBuffer<float> Buffer;

        /// <summary>
        /// Initialises a new instance of the <see cref="SplitAudioSource"/> class.
        /// </summary>
        /// <param name="parent">The <see cref="AudioSourceSplitter"/> that
        /// this audio source is split from.</param>
        /// <param name="channels">The number of channels for this source.</param>
        public SplitAudioSource(AudioSourceSplitter parent, int channels)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Format = new AudioFormat((short)channels, Parent.Format.SampleRate);
            Buffer = new CircularBuffer<float>(Format.SampleRate);
        }

        /// <inheritdoc/>
        public AudioFormat Format { get; }

        /// <inheritdoc/>
        public float[] Read(int samples)
        {
            if (Buffer.ItemsAvailable < samples)
            {
                Parent.RequestRead(samples);
            }

            return Buffer.Read(samples * Format.Channels);
        }

        /// <summary>
        /// Writes the given samples to the <see cref="SplitAudioSource"/>.
        /// </summary>
        /// <param name="samples">Samples to be written.</param>
        public void Write(float[] samples)
        {
            Buffer.Write(samples);
        }

        /// <summary>
        /// Writes a sample to the <see cref="SplitAudioSource"/>.
        /// </summary>
        /// <param name="sample">Sample to be written.</param>
        public void Write(float sample)
        {
            Buffer.Write(sample);
        }
    }
}
