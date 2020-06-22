using System;
using NAudio.Wave;

namespace Oceana.Core
{
    /// <summary>
    /// Represents an output for audio.
    /// </summary>
    public class NAudioOutput : IAudioSink, IDisposable
    {
        private readonly IWavePlayer Device;

        private bool disposedValue;

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioOutput"/> class.
        /// </summary>
        /// <param name="source">Source of audio data.</param>
        public NAudioOutput(IAudioSource source)
            : this(new WaveOutEvent(), source)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioOutput"/> class.
        /// </summary>
        /// <param name="device">Output device.</param>
        /// <param name="source">Audio source.</param>
        internal NAudioOutput(IWavePlayer device, IAudioSource source)
        {
            Device = device;
            Device.Init(new NAudioSourceWaveProvider(source));
            Device.Play();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void RegisterSource(IAudioSource source, int channelStart)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes any owned resources.
        /// </summary>
        /// <param name="disposing">Is dispose being triggered.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Device.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
