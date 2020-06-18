using System;
using NAudio.Wave;

namespace Oceana.Core
{
    /// <summary>
    /// Represents an output for audio.
    /// </summary>
    public class NAudioOutput : IAudioSink, IDisposable
    {
        private readonly WaveOutEvent Device;

        private bool disposedValue;

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioOutput"/> class.
        /// </summary>
        /// <param name="source">Source of audio data.</param>
        public NAudioOutput(IAudioSource source)
        {
            Device = new WaveOutEvent
            {
                DeviceNumber = 0,
            };
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
