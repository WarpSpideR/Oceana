using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana.Core
{
    /// <summary>
    /// Smoothly fades in the output from an <see cref="IAudioSource"/>.
    /// </summary>
    public class FadeInFilter : IAudioSource
    {
        private readonly IAudioSource Source;
        private int SampleDuration;
        private int CurrentSample;
        private TimeSpan DurationLocal;

        /// <summary>
        /// Initialises a new instance of the <see cref="FadeInFilter"/> class.
        /// </summary>
        /// <param name="source">The <see cref="IAudioSource"/> to fade in.</param>
        public FadeInFilter(IAudioSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Format = source.Format;

            SampleDuration = (int)(Format.SampleRate * Duration.TotalSeconds);

            Reset();
        }

        /// <inheritdoc/>
        public AudioFormat Format { get; }

        /// <summary>
        /// Gets or sets the duration of the fade effect.
        /// </summary>
        public TimeSpan Duration
        {
            get => DurationLocal;
            set
            {
                DurationLocal = value;
                SampleDuration = (int)(Format.SampleRate * Duration.TotalSeconds);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the filter has completed
        /// its fade effect.
        /// </summary>
        public bool FadingComplete { get; private set; }

        /// <inheritdoc/>
        public float[] Read(int samples)
        {
            var buffer = Source.Read(samples);

            if (FadingComplete)
            {
                return buffer;
            }

            for (int i = 0; i < buffer.Length; i += Format.Channels)
            {
                var fadeLevel = FadeLevel();

                for (int channel = 0; channel < Format.Channels; channel++)
                {
                    buffer[i + channel] = buffer[i + channel] * fadeLevel;
                }

                if (++CurrentSample >= SampleDuration)
                {
                    FadingComplete = true;
                    break;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Resets the filter to its initial state.
        /// </summary>
        public void Reset()
        {
            FadingComplete = false;
            CurrentSample = 0;
        }

        private float FadeLevel()
            => Math.Max(
                0f,
                Math.Min(
                    CurrentSample / (float)SampleDuration,
                    1f));
    }
}
