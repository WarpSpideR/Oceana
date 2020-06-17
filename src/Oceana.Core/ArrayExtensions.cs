using System;

namespace Oceana.Core
{
    /// <summary>
    /// Helper methods to aid working with <see cref="Array"/>s.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Copies the data from the input array to the output array.
        /// </summary>
        /// <param name="input">Input data to copy from.</param>
        /// <param name="output">Output array to copy to.</param>
        /// <param name="offset">Position to start writing to in the
        /// output array.</param>
        /// <param name="count">The requested number of entries to copy.</param>
        /// <returns>The actual number of entries that were copied.</returns>
        public static int CopyTo(this float[] input, float[] output, int offset, int count)
        {
            _ = input ?? throw new ArgumentNullException(nameof(input));
            _ = output ?? throw new ArgumentNullException(nameof(output));

            if (count > input.Length)
            {
                count = input.Length;
            }

            for (int i = 0; i < count; i++)
            {
                output[i + offset] = input[i];
            }

            return count;
        }
    }
}
