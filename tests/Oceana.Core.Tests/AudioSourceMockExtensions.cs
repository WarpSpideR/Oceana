using System;
using System.Collections.Generic;
using System.Text;
using Moq;

namespace Oceana.Core.Tests
{
    public static class AudioSourceMockExtensions
    {
        public static Mock<IAudioSource> ConfigureDefaults(
            this Mock<IAudioSource> mock,
            short channels = 1,
            int sampleRate = 10,
            float defaultReadValue = 1f)
        {
            mock.Setup(m => m.Format)
                .Returns(new AudioFormat(channels, sampleRate));
            mock.Setup(m => m.Read(It.IsAny<int>()))
                .Returns((int count) => PopulateArray(count, mock.Object.Format.Channels, defaultReadValue));

            return mock;
        }


        private static float[] PopulateArray(int count, int channels, float value)
        {
            count *= channels;

            var result = new float[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = value;
            }

            return result;
        }
    }
}
