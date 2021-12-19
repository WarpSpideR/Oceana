using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Oceana.Agent.Windows
{
    public static class AudioCapabilities
    {
        public static IEnumerable<AudioSinkInfo> GetAudioSinks()
        {
            var wave = new List<AudioSinkInfo>();

            for (int index = 0; index < WaveOut.DeviceCount; index++)
            {
                var caps = WaveOut.GetCapabilities(index);

                wave.Add(new AudioSinkInfo
                {
                    DeviceId = caps.ProductGuid,
                    Name = caps.ProductName,
                    Channels = caps.Channels,
                });
            }

            return wave;
        }

        public static IEnumerable<AudioSourceInfo> GetAudioSources()
        {
            var wave = new List<AudioSourceInfo>();

            for (int index = 0; index < WaveIn.DeviceCount; index++)
            {
                var caps = WaveIn.GetCapabilities(index);

                wave.Add(new AudioSourceInfo
                {
                    DeviceId = caps.ProductGuid,
                    Name = caps.ProductName,
                    Channels = caps.Channels,
                });
            }

            return wave;
        }
    }
}
