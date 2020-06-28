using System;
using System.Collections.Generic;

namespace Oceana.Core
{
    /// <summary>
    /// Helper methods to ease working with <see cref="Range"/>s.
    /// </summary>
    public static class RangeExtensions
    {
        /// <summary>
        /// Converts a <see cref="Range"/> into a <see cref="List{T}"/>
        /// containing the values in that <see cref="Range"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to convert.</param>
        /// <returns>A <see cref="List{T}"/> of the numbers.</returns>
        public static List<int> ToList(this Range range)
        {
            var number = range.Start.Value;
            var count = range.End.Value - range.Start.Value;
            var increment = 1;

            if (count < 0)
            {
                count = Math.Abs(count);
                increment = -1;
            }

            var result = new List<int>(count);

            for (var i = 0; i <= count; i++)
            {
                result.Add(number);

                number += increment;
            }

            return result;
        }
    }
}
