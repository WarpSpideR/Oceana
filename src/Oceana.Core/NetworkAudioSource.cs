using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oceana
{
    /// <summary>
    /// An <see cref="IAudioSource"/> that gets it's audio intput
    /// from a network stream.
    /// </summary>
    public class NetworkAudioSource : IAudioSource
    {
        private readonly Stream Input;

        private readonly CircularBuffer<float> Buffer;

        /// <summary>
        /// Initialises a new instance of the <see cref="NetworkAudioSource"/> class.
        /// </summary>
        /// <param name="stream">Incomming network stream.</param>
        public NetworkAudioSource(Stream stream)
        {
            Input = stream;
            Buffer = new CircularBuffer<float>(32000);
            ThreadPool.QueueUserWorkItem((state) => ReadFromNetwork(), null);
        }

        /// <summary>
        /// Gets the ormat of audio being produced.
        /// </summary>
        public AudioFormat Format => new AudioFormat(1, 8000);

        /// <inheritdoc/>
        public float[] Read(int samples)
        {
            while (Buffer.ItemsAvailable < 1600)
            {
                Thread.Sleep(10);
            }

            return Buffer.Read(samples);
        }

        private void ReadFromNetwork()
        {
            while (true)
            {
                var buffer = new byte[1200];
                var bytes = Input.Read(buffer, 0, buffer.Length);

                var result = new float[bytes / sizeof(float)];
                System.Buffer.BlockCopy(buffer, 0, result, 0, bytes);

                Buffer.Write(result);
            }
        }
    }
}
