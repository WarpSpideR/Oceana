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
using Oceana.Core.NAudio;

namespace Oceana.Core
{
    /// <summary>
    /// Captures audio samples from an input device.
    /// </summary>
    public class NAudioInput : IAudioSource, IDisposable
    {
        private WaveInEvent Device;

        private ISampleProvider Provider;

        private bool Disposed = false;

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioInput"/> class.
        /// </summary>
        public NAudioInput()
        {
            Device = new WaveInEvent();
            Device.WaveFormat = new WaveFormat();
            Device.RecordingStopped += Device_RecordingStopped;

            var waveInProvider = new WaveInProvider(Device);

            Provider = new Pcm16BitToSampleProvider(waveInProvider);

            Format = Device.WaveFormat.ToAudioFormat();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioInput"/> class.
        /// </summary>
        /// <param name="deviceId">Id of the device to capture audio from.</param>
        public NAudioInput(int deviceId)
            : this()
        {
            Device.DeviceNumber = deviceId;
            Device.StartRecording();
        }

        /// <inheritdoc/>
        public AudioFormat Format { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public float[] Read(int samples)
        {
            var requestedSamples = samples * Format.Channels;

            var buffer = new float[requestedSamples];

            int samplesRead = Provider.Read(buffer, 0, requestedSamples);

            if (samplesRead == requestedSamples)
            {
                return buffer;
            }

            var result = new float[samplesRead];

            for (int i = 0; i < samplesRead; i++)
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
