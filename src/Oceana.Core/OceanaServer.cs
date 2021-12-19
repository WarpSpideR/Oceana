using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Oceana
{
    /// <summary>
    /// Oceana server instance.
    /// </summary>
    public class OceanaServer
    {
        private readonly ILogger Logger;
        private readonly IAudioHardware Hardware;

        /// <summary>
        /// Initialises a new instance of the <see cref="OceanaServer"/> class.
        /// </summary>
        /// <param name="hardware">Access to audio hardware.</param>
        /// <param name="logger">Logger instance to write log messages to.</param>
        public OceanaServer(IAudioHardware hardware, ILogger<OceanaServer> logger)
        {
            Hardware = hardware;
            Logger = logger;
        }

        /// <summary>
        /// Gets the output devices available to the server.
        /// </summary>
        /// <returns>Collection of available devices.</returns>
        public Task<IEnumerable<DeviceInfo>> GetOutputDevicesAsync()
            => Task.FromResult(Hardware.GetOutputDevices());

        /// <summary>
        /// Gets the output devices available to the server.
        /// </summary>
        /// <returns>Collection of available devices.</returns>
        public Task<IEnumerable<DeviceInfo>> GetInputDevicesAsync()
            => Task.FromResult(Hardware.GetInputDevices());
    }
}
