using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// A source audio stream provider.
    /// </summary>
    public interface IAudioSource
    {
        /// <summary>
        /// Reads audio samples from the source.
        /// </summary>
        /// <param name="count">The number of samples requested.</param>
        /// <returns>A array of audio samples.</returns>
        float[] Read(int count);
    }
}
