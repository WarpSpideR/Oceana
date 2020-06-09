using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using NAudio.Utils;
using NAudio.Wave;

namespace Oceana.Core
{
    /// <summary>
    /// Adapter for an <see cref="IAudioSource"/> to be compatible
    /// with an <see cref="IWaveProvider"/>.
    /// </summary>
    internal class AudioSourceWaveProvider : ISampleProvider
    {
        private readonly IAudioSource Source;

        /// <summary>
        /// Initialises a new instance of the <see cref="AudioSourceWaveProvider"/> class.
        /// </summary>
        /// <param name="source"><see cref="IAudioSource"/> to adapt.</param>
        public AudioSourceWaveProvider(IAudioSource source)
        {
            Source = source;
        }

        /// <inheritdoc/>
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        /// <inheritdoc/>
        public int Read(float[] buffer, int offset, int count)
        {
            var result = Source.Read(count);

            return result.CopyTo(buffer, offset, count);
        }
    }
}
