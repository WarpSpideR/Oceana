using System;
using NAudio.Wave;

namespace Oceana
{
    /// <summary>
    /// An <see cref="IAudioSource"/> that will continue to send empty samples
    /// if the supplied <see cref="IAudioSource"/> has no longer got any samples
    /// to read.
    /// </summary>
    public class InfiniteAudioSource : IAudioSource, ISampleProvider
    {
        private IAudioSource Source;

        /// <summary>
        /// Initialises a new instance of the <see cref="InfiniteAudioSource"/> class.
        /// </summary>
        /// <param name="source">The <see cref="IAudioSource"/> to fade in.</param>
        public InfiniteAudioSource(IAudioSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Format = source.Format;
        }

        /// <summary>
        /// Gets the format of this <see cref="IAudioSource"/>.
        /// </summary>
        public AudioFormat Format { get; private set; }

        /// <summary>
        /// Gets the format of this <see cref="IAudioSource"/>.
        /// </summary>
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(Format.SampleRate, Format.Channels);

        /// <summary>
        /// Changes the output format of the stream.
        /// </summary>
        /// <param name="format">New format to use.</param>
        public void SetFormat(AudioFormat format)
        {
            Format = format;
        }

        /// <summary>
        /// Changes the input <see cref="IAudioSource"/> that is being read from.
        /// </summary>
        /// <param name="source">New <see cref="IAudioSource"/>.</param>
        public void SetAudioSource(IAudioSource source)
        {
            Source = source;
        }

        /// <inheritdoc/>
        public float[] Read(int samples)
        {
            if (Source == null)
            {
                return new float[samples];
            }

            var buffer = Source.Read(samples);

            if (buffer == null || buffer.Length == 0)
            {
                return new float[samples];
            }

            return buffer;
        }

        /// <inheritdoc/>
        public int Read(float[] buffer, int offset, int count)
        {
            _ = buffer ?? throw new ArgumentNullException(nameof(buffer), "No buffer was given to write to.");

            if (Source == null)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[offset + i] = 0;
                }

                return count;
            }

            var data = Source.Read(count);

            if (data == null || data.Length == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[offset + i] = 0;
                }

                return count;
            }

            for (int i = 0; i < data.Length; i++)
            {
                buffer[offset + i] = data[i];
            }

            return data.Length;
        }
    }
}
