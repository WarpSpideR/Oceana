namespace Oceana.Server.Features.Zones;

public class ZoneRegistryTests
{
    [Fact]
    public void Create_GeneratesIdAndStoresDevices()
    {
        var registry = new ZoneRegistry();

        var zone = registry.Create("Kitchen", new[] { Device() });

        zone.Should().NotBeNull();
        zone!.Id.Should().NotBe(Guid.Empty);
        zone.Name.Should().Be("Kitchen");
        zone.Devices.Should().ContainSingle();
        registry.Get(zone.Id).Should().NotBeNull();
    }

    [Fact]
    public void Create_TrimsTheName()
    {
        var registry = new ZoneRegistry();

        var zone = registry.Create("  Lounge  ", Array.Empty<ZoneDevice>());

        zone!.Name.Should().Be("Lounge");
    }

    [Fact]
    public void Create_RejectsADuplicateName_CaseInsensitively()
    {
        var registry = new ZoneRegistry();
        registry.Create("Kitchen", Array.Empty<ZoneDevice>());

        registry.Create("kitchen", Array.Empty<ZoneDevice>()).Should().BeNull();
    }

    [Fact]
    public void GetAll_ReturnsEveryZone()
    {
        var registry = new ZoneRegistry();
        registry.Create("Kitchen", Array.Empty<ZoneDevice>());
        registry.Create("Lounge", Array.Empty<ZoneDevice>());

        registry.GetAll().Should().HaveCount(2);
    }

    [Fact]
    public void Get_ReturnsNull_ForUnknownId()
    {
        new ZoneRegistry().Get(Guid.NewGuid()).Should().BeNull();
    }

    [Fact]
    public void Update_ReplacesNameAndDevices()
    {
        var registry = new ZoneRegistry();
        var zone = registry.Create("Kitchen", Array.Empty<ZoneDevice>())!;

        var result = registry.Update(zone.Id, "Kitchen & Diner", new[] { Device() });

        result.Status.Should().Be(ZoneUpdateStatus.Updated);
        result.Zone!.Name.Should().Be("Kitchen & Diner");
        result.Zone.Devices.Should().ContainSingle();
        registry.Get(zone.Id)!.Name.Should().Be("Kitchen & Diner");
    }

    [Fact]
    public void Update_ReturnsNotFound_ForUnknownId()
    {
        var result = new ZoneRegistry().Update(Guid.NewGuid(), "Kitchen", Array.Empty<ZoneDevice>());

        result.Status.Should().Be(ZoneUpdateStatus.NotFound);
        result.Zone.Should().BeNull();
    }

    [Fact]
    public void Update_AllowsAZoneToKeepItsOwnName()
    {
        var registry = new ZoneRegistry();
        var zone = registry.Create("Kitchen", Array.Empty<ZoneDevice>())!;

        var result = registry.Update(zone.Id, "kitchen", new[] { Device() });

        result.Status.Should().Be(ZoneUpdateStatus.Updated);
        result.Zone!.Devices.Should().ContainSingle();
    }

    [Fact]
    public void Update_ReturnsNameConflict_WhenTakingAnotherZonesName()
    {
        var registry = new ZoneRegistry();
        registry.Create("Kitchen", Array.Empty<ZoneDevice>());
        var lounge = registry.Create("Lounge", Array.Empty<ZoneDevice>())!;

        var result = registry.Update(lounge.Id, "kitchen", Array.Empty<ZoneDevice>());

        result.Status.Should().Be(ZoneUpdateStatus.NameConflict);
        registry.Get(lounge.Id)!.Name.Should().Be("Lounge");
    }

    [Fact]
    public void Remove_RemovesZone()
    {
        var registry = new ZoneRegistry();
        var zone = registry.Create("Kitchen", Array.Empty<ZoneDevice>())!;

        registry.Remove(zone.Id).Should().BeTrue();
        registry.Get(zone.Id).Should().BeNull();
        registry.Remove(zone.Id).Should().BeFalse();
    }

    [Fact]
    public void Remove_FreesTheNameForReuse()
    {
        var registry = new ZoneRegistry();
        var zone = registry.Create("Kitchen", Array.Empty<ZoneDevice>())!;
        registry.Remove(zone.Id);

        registry.Create("Kitchen", Array.Empty<ZoneDevice>()).Should().NotBeNull();
    }

    private static ZoneDevice Device(string deviceId = "device-1")
    {
        return new ZoneDevice { AgentId = Guid.NewGuid(), DeviceId = deviceId };
    }
}
