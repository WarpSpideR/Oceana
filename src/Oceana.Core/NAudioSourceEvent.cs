using System;
using NAudio.Wave;

namespace Oceana
{
    /// <summary>
    /// Captures audio samples from an input device.
    /// </summary>
    public class NAudioSourceEvent : IDisposable
    {
        private readonly IWaveIn Device;

        private bool Disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioSourceEvent"/> class.
        /// </summary>
        /// <param name="deviceId">Id of the device to capture audio from.</param>
        public NAudioSourceEvent(int deviceId)
        {
            Device = new WaveInEvent
            {
                DeviceNumber = deviceId,
            };
            Device.DataAvailable += DeviceDataAvailable;
            Format = Device.WaveFormat.ToAudioFormat();
            Device.StartRecording();
        }

        /// <summary>
        /// Raised when new samples have been made available from the audio source.
        /// </summary>
        public event EventHandler<SamplesAvailableEventArgs>? SamplesAvailable;

        /// <summary>
        /// Gets the format of this Audio source.
        /// </summary>
        public AudioFormat Format { get; }

        /// <summary>
        /// Used to rase the SamplesAvailable enent.
        /// </summary>
        /// <param name="args">Arguments to be passed to the event.</param>
        protected virtual void OnSamplesAvailable(SamplesAvailableEventArgs args)
        {
            SamplesAvailable?.Invoke(this, args);
        }

        private void DeviceDataAvailable(object sender, WaveInEventArgs e)
        {
            var samples = new float[e.BytesRecorded / 2];

            int index = 0;
            for (int n = 0; n < e.BytesRecorded; n += 2)
            {
                samples[index++] = BitConverter.ToInt16(e.Buffer, n) / 32768f;
            }

            OnSamplesAvailable(new SamplesAvailableEventArgs(samples));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
    }
}
