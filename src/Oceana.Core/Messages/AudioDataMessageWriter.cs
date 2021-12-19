using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Oceana
{
    /// <summary>
    /// Represents an audio data message.
    /// </summary>
    public class AudioDataMessageWriter : IMessageWriter
    {
        private readonly MessageCode MessageCode = MessageCode.AudioData;

        private readonly byte[] AudioData;

        /// <summary>
        /// Initialises a new instance of the <see cref="AudioDataMessageWriter"/> class.
        /// </summary>
        /// <param name="audioData">Audio data to be sent.</param>
        public AudioDataMessageWriter(byte[] audioData)
        {
            AudioData = audioData;
        }

        /// <inheritdoc/>
        public async Task SendAsync(Stream writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));

            var header = new byte[9];
            header[0] = (byte)MessageCode;
            var channelBytes = BitConverter.GetBytes(1);
            Buffer.BlockCopy(channelBytes, 0, header, 5, 4);
            var lengthBytes = BitConverter.GetBytes(AudioData.Length);
            Buffer.BlockCopy(lengthBytes, 0, header, 1, 4);

            await writer.WriteAsync(header, 0, header.Length)
                .ConfigureAwait(false);

            await writer.WriteAsync(AudioData, 0, AudioData.Length)
                .ConfigureAwait(false);
        }
    }
}
