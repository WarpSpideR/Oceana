using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Oceana
{
    /// <summary>
    /// Represents a message that can be transmitted.
    /// </summary>
    public interface IMessageWriter
    {
        /// <summary>
        /// Sends the contents of the message.
        /// </summary>
        /// <param name="output">Output to write the message to.</param>
        /// <returns>Task representing the result of the asynchronous operation.</returns>
        Task SendAsync(Stream output);
    }
}
