using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Provides access to audio hardware of the server.
    /// </summary>
    public interface IAudioHardware
    {
        /// <summary>
        /// Gets a collection of availabe input devices found on the server.
        /// </summary>
        /// <returns>A collection of available input devices.</returns>
        IEnumerable<DeviceInfo> GetInputDevices();

        /// <summary>
        /// Gets a collection of available output devices found on the server.
        /// </summary>
        /// <returns>A collection of available output devices.</returns>
        IEnumerable<DeviceInfo> GetOutputDevices();
    }
}
