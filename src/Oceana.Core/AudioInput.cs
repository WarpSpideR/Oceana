using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Oceana.Core
{
    /// <summary>
    /// Captures audio samples from an input device.
    /// </summary>
    public class AudioInput : IAudioSource, IDisposable
    {
        private WaveInEvent Device;

        private ISampleProvider Provider;

        private bool Disposed = false;

        /// <summary>
        /// Initialises a new instance of the <see cref="AudioInput"/> class.
        /// </summary>
        public AudioInput()
        {
            Device = new WaveInEvent();
            Device.WaveFormat = new WaveFormat();
            Device.RecordingStopped += Device_RecordingStopped;

            var waveInProvider = new WaveInProvider(Device);

            Provider = new Pcm16BitToSampleProvider(waveInProvider);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="AudioInput"/> class.
        /// </summary>
        /// <param name="deviceId">Id of the device to capture audio from.</param>
        public AudioInput(int deviceId)
            : this()
        {
            Device.DeviceNumber = deviceId;
            Device.StartRecording();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public float[] Read(int count)
        {
            var buffer = new float[count];

            int actual = Provider.Read(buffer, 0, count);

            var result = new float[count];

            for (int i = 0; i < actual; i++)
            {
                result[i] = buffer[i];
            }

            return result;
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        /// <param name="disposing">Indicates if the object is being
        /// manually disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Device.Dispose();
                }

                Disposed = true;
            }
        }

        private void Device_RecordingStopped(object sender, StoppedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
