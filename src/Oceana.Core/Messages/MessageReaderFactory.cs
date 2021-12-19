using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Factory class for message readers.
    /// </summary>
    public static class MessageReaderFactory
    {
        private static readonly Dictionary<MessageCode, Type> Readers = new Dictionary<MessageCode, Type>
        {
            [MessageCode.AudioData] = typeof(AudioDataMessageReader),
        };

        /// <summary>
        /// Gets the approriate reader for the given <see cref="MessageCode"/>.
        /// </summary>
        /// <param name="code">Code describing the message to be read.</param>
        /// <returns>Class capable of reading the message.</returns>
        public static IMessageReader? GetReader(MessageCode code)
        {
            if (Readers.TryGetValue(code, out Type readerType))
            {
                return (IMessageReader)Activator.CreateInstance(readerType);
            }

            return null;
        }
    }
}
