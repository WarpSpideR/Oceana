using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Passes event information when new Samples are available.
    /// </summary>
    public class SamplesAvailableEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="SamplesAvailableEventArgs"/> class.
        /// </summary>
        /// <param name="samples">Newly available samples.</param>
        public SamplesAvailableEventArgs(float[] samples)
        {
            Samples = samples;
        }

        /// <summary>
        /// Gets the newly available samples.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Data is not accessed elsewhere.")]
        public float[] Samples { get; }
    }
}
