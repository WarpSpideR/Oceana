using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Oceana
{
    /// <summary>
    /// Represents a message that can be read.
    /// </summary>
    public interface IMessageReader
    {
        /// <summary>
        /// Reads message data from a stream.
        /// </summary>
        /// <param name="input">Input to read from.</param>
        /// <returns>Task representing the result of an asynchronus operation.</returns>
        Task ReadAsync(Stream input);
    }
}
