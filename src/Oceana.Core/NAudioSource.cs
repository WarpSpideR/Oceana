using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Oceana
{
    /// <summary>
    /// Captures audio samples from an input device.
    /// </summary>
    public class NAudioSource : IAudioSource, IDisposable
    {
        private readonly IWaveIn Device;

        private readonly ISampleProvider Provider;

        private bool Disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioSource"/> class.
        /// </summary>
        public NAudioSource()
            : this(0)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioSource"/> class.
        /// </summary>
        /// <param name="deviceId">Id of the device to capture audio from.</param>
        public NAudioSource(int deviceId)
        {
            Device = new WaveInEvent
            {
                DeviceNumber = deviceId,
            };
            Provider = new Pcm16BitToSampleProvider(new WaveInProvider(Device));
            Format = Device.WaveFormat.ToAudioFormat();
            Device.StartRecording();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioSource"/> class.
        /// </summary>
        /// <param name="provider">Provider to use.</param>
        /// <param name="waveIn">The wave in device.</param>
        internal NAudioSource(IWaveIn waveIn, ISampleProvider provider)
        {
            Device = waveIn;
            Provider = provider;
            Format = Device.WaveFormat.ToAudioFormat();
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
    }
}
