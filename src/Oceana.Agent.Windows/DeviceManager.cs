using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Oceana.Agent.Windows
{
    public class DeviceManager
    {
        private readonly ILogger Logger;

        private readonly List<AudioDeviceInfo> RegisteredSinks;

        public DeviceManager(ILogger<DeviceManager> logger)
        {
            RegisteredSinks = new List<AudioDeviceInfo>();
            Logger = logger;
        }

        public async Task RegisterAudioSink(Guid deviceId)
        {
            Logger.LogInformation("Registering device with id {DeviceId}", deviceId);
        }

        public void QueueAudioSamples(int channels, byte[] audio)
        {
            Logger.LogInformation("Queuing {Length} audio samples to channel {Channel}", audio.Length, channels);
        }
    }
}
