using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using NAudio.Wave;

namespace Oceana.Core
{
    /// <summary>
    /// Represents an output for audio.
    /// </summary>
    public class AudioOutput : IAudioSink, IDisposable
    {
        private readonly IAudioSource Source;
        private readonly WaveOutEvent Device;

        private bool disposedValue = false;

        /// <summary>
        /// Initialises a new instance of the <see cref="AudioOutput"/> class.
        /// </summary>
        /// <param name="source">Source of audio data.</param>
        public AudioOutput(IAudioSource source)
        {
            Device = new WaveOutEvent();
            Device.DeviceNumber = 0;
            Device.Init(new AudioSourceWaveProvider(source));
            Source = source;
            Device.Play();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
