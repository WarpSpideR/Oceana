using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Describes the format of an audio source.
    /// </summary>
    public readonly struct AudioFormat : IEquatable<AudioFormat>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="AudioFormat"/> struct.
        /// </summary>
        /// <param name="channels">The number of channels in the audio source.</param>
        /// <param name="sampleRate">The number of samples per second in the audio source.</param>
        public AudioFormat(short channels, int sampleRate)
        {
            Channels = channels;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// Gets the number of channels in the audio source.
        /// </summary>
        public short Channels { get; }

        /// <summary>
        /// Gets the number of samples per second in the audio source.
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Determines whether two <see cref="AudioFormat"/> instances
        /// are equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns>True if the two instances are equal, false otherwise.</returns>
        public static bool operator ==(AudioFormat left, AudioFormat right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="AudioFormat"/> instances
        /// are different.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns>True if the two instances are different, false otherwise.</returns>
        public static bool operator !=(AudioFormat left, AudioFormat right)
            => !left.Equals(right);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is AudioFormat other)
            {
                return Channels == other.Channels
                    && SampleRate == other.SampleRate;
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => Channels ^ SampleRate;

        /// <inheritdoc/>
        public bool Equals(AudioFormat other)
            => Equals((object)other);
    }
}
