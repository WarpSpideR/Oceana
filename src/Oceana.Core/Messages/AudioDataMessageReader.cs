using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Oceana
{
    /// <summary>
    /// Represents an audio data message.
    /// </summary>
    public class AudioDataMessageReader : IMessageReader
    {
        private byte[] AudioData = Array.Empty<byte>();

        /// <summary>
        /// Gets a value indicating whether the message has completed.
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <inheritdoc/>
        public async Task ReadAsync(Stream reader)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            var dataLengthBuffer = new byte[sizeof(int)];
            var bytesRead = await reader.ReadAsync(dataLengthBuffer, 0, dataLengthBuffer.Length)
                .ConfigureAwait(false);

            var dataLength = BitConverter.ToInt32(dataLengthBuffer, 0);

            bytesRead = 0;

            AudioData = new byte[dataLength];
            while (bytesRead < dataLength)
            {
                bytesRead += await reader.ReadAsync(AudioData, bytesRead, dataLength - bytesRead)
                    .ConfigureAwait(false);
            }

            IsComplete = true;
        }

        /// <summary>
        /// Gets the read audio data.
        /// </summary>
        /// <returns>Audio data.</returns>
        public byte[] GetAudioData()
            => AudioData;
    }
}
