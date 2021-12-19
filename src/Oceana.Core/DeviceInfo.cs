using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Describes an audio device.
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="DeviceInfo"/> class.
        /// </summary>
        /// <param name="deviceId">Identified for the device.</param>
        /// <param name="productName">Product name for the device.</param>
        /// <param name="channels">Number of channels suported by the device.</param>
        public DeviceInfo(
            int deviceId,
            string productName,
            int channels)
        {
            DeviceId = deviceId;
            ProductName = productName;
            Channels = channels;
        }

        /// <summary>
        /// Gets the device indentifier.
        /// </summary>
        public int DeviceId { get; private set; }

        /// <summary>
        /// Gets the product name of the device.
        /// </summary>
        public string ProductName { get; private set; }

        /// <summary>
        /// Gets the number of channels supported by the device.
        /// </summary>
        public int Channels { get; private set; }
    }
}
