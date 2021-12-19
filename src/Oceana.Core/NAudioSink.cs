using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Oceana
{
    /// <summary>
    /// An Audio sink implemented by the NAudio library.
    /// </summary>
    public class NAudioSink : IDisposable
    {
        private readonly WaveOutEvent Output;

        private readonly ISampleProvider Source;

        /// <summary>
        /// Initialises a new instance of the <see cref="NAudioSink"/> class.
        /// </summary>
        /// <param name="source">Input audio source.</param>
        public NAudioSink(IAudioSource source)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source), "No audio source supplied");
            Source = new NAudioSourceWaveProvider(source);

            Output = new WaveOutEvent();
            Output.DeviceNumber = 0;
            Output.PlaybackStopped += DevicePlaybackStopped;
            Output.Init(Source);
        }

        /// <summary>
        /// Starts playback to the sink.
        /// </summary>
        public void Start()
        {
            Output.Play();
        }

        private void DevicePlaybackStopped(object sender, StoppedEventArgs e)
        {
            Console.WriteLine(e.Exception?.Message ?? string.Empty);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Determines if this is being disposed manually.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Output.Stop();
                Output.Dispose();
            }
        }
    }
}
