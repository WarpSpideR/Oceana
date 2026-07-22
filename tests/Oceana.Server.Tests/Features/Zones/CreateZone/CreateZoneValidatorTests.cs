using Oceana.Server.Features.Zones;

namespace Oceana.Server.Features.Zones.CreateZone;

public class CreateZoneValidatorTests
{
    private readonly CreateZoneValidator validator = new CreateZoneValidator();

    [Fact]
    public void Passes_ForAValidRequest()
    {
        var request = new CreateZoneRequest
        {
            Name = "Kitchen",
            Devices = new List<ZoneDevice> { Device("device-1"), Device("device-2") },
        };

        this.validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Passes_ForAZoneWithNoDevices()
    {
        var request = new CreateZoneRequest { Name = "Empty Zone" };

        this.validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Fails_WhenNameIsEmpty()
    {
        var result = this.validator.Validate(new CreateZoneRequest { Name = string.Empty });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "A zone must have a name.");
    }

    [Fact]
    public void Fails_WhenADeviceHasNoDeviceId()
    {
        var request = new CreateZoneRequest
        {
            Name = "Kitchen",
            Devices = new List<ZoneDevice> { new ZoneDevice { AgentId = Guid.NewGuid(), DeviceId = string.Empty } },
        };

        this.validator.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Fails_WhenADeviceHasNoAgent()
    {
        var request = new CreateZoneRequest
        {
            Name = "Kitchen",
            Devices = new List<ZoneDevice> { new ZoneDevice { AgentId = Guid.Empty, DeviceId = "device-1" } },
        };

        this.validator.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Fails_WhenADeviceIsListedTwice()
    {
        var agentId = Guid.NewGuid();
        var request = new CreateZoneRequest
        {
            Name = "Kitchen",
            Devices = new List<ZoneDevice>
            {
                new ZoneDevice { AgentId = agentId, DeviceId = "device-1" },
                new ZoneDevice { AgentId = agentId, DeviceId = "device-1" },
            },
        };

        var result = this.validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "A device cannot be listed more than once in a zone.");
    }

    private static ZoneDevice Device(string deviceId)
    {
        return new ZoneDevice { AgentId = Guid.NewGuid(), DeviceId = deviceId };
    }
}
