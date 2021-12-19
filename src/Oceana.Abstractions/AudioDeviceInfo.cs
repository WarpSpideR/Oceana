using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Base class for audio device information classes.
    /// </summary>
    public abstract class AudioDeviceInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the device.
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the device.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of audio channels supported by the device.
        /// </summary>
        public int Channels { get; set; }
    }
}
