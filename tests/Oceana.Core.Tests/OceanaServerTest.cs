using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public partial class OceanaServerTest
    {
        private readonly Mock<IAudioHardware> HardwareMock;
        private readonly Mock<ILogger<OceanaServer>> LogMock;

        public OceanaServerTest()
        {
            HardwareMock = new Mock<IAudioHardware>();
            LogMock = new Mock<ILogger<OceanaServer>>();
        }
    }
}
