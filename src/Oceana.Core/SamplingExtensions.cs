using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Oceana.Core
{
    /// <summary>
    /// Collection of helper methods to ease working with samples.
    /// </summary>
    public static class SamplingExtensions
    {
        /// <summary>
        /// Converts a <see cref="byte"/> array to an array of
        /// floating point numbers.
        /// </summary>
        /// <param name="bytes">Byte array to convert.</param>
        /// <returns>An array of floating point samples.</returns>
        public static float[] ToSamples(this byte[] bytes)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length % 2 != 0)
            {
                throw new ArgumentException(string.Empty, nameof(bytes));
            }

            var samples = new float[bytes.Length / 2];

            for (int i = 0; i < bytes.Length / 2; i++)
            {
                samples[i] = bytes[i] / 32768f;
            }

            return samples;
        }

        /// <summary>
        /// Converts an array of floating point numbers to a
        /// <see cref="byte"/> array.
        /// </summary>
        /// <param name="samples">Floating point samples array to convert.</param>
        /// <returns>An array of the corresponding byte values.</returns>
        public static byte[] ToBytes(this float[] samples)
        {
            _ = samples ?? throw new ArgumentNullException(nameof(samples));

            var bytes = new byte[samples.Length * sizeof(float)];

            Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

            return bytes;
        }
    }
}
