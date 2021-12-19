using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Oceana.Core.Tests
{
    public partial class OceanaServerTest
    {
        [Fact]
        public async Task GetInputDevicesShouldQueryHardware()
        {
            var server = new OceanaServer(HardwareMock.Object, LogMock.Object);

            _ = await server.GetInputDevicesAsync();

            HardwareMock.Verify(m => m.GetInputDevices(), Times.Once);
        }

        [Fact]
        public async Task GetOutputDevicesShouldQueryHardware()
        {
            var server = new OceanaServer(HardwareMock.Object, LogMock.Object);

            _ = await server.GetOutputDevicesAsync();

            HardwareMock.Verify(m => m.GetOutputDevices(), Times.Once);
        }
    }
}
