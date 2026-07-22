using Oceana.Server.Infrastructure.Persistence;

namespace Oceana.Server.Features.Zones;

public class ZoneRegistryPersistenceTests
{
    [Fact]
    public void Constructor_LoadsPersistedZones_PreservingIds()
    {
        var id = Guid.NewGuid();
        var store = new InMemoryStateStore<ZonesState>(new ZonesState(new[]
        {
            new ZoneInfo { Id = id, Name = "Kitchen", Devices = Array.Empty<ZoneDevice>() },
        }));

        var registry = new ZoneRegistry(store);

        registry.Get(id).Should().NotBeNull();
        registry.Get(id)!.Name.Should().Be("Kitchen");
    }

    [Fact]
    public void Create_PersistsTheNewZone()
    {
        var store = new InMemoryStateStore<ZonesState>();
        var registry = new ZoneRegistry(store);

        registry.Create("Kitchen", Array.Empty<ZoneDevice>());

        store.Current.Should().NotBeNull();
        store.Current!.Zones.Should().ContainSingle(z => z.Name == "Kitchen");
    }

    [Fact]
    public void Update_PersistsTheChange()
    {
        var store = new InMemoryStateStore<ZonesState>();
        var registry = new ZoneRegistry(store);
        var zone = registry.Create("Kitchen", Array.Empty<ZoneDevice>())!;

        registry.Update(zone.Id, "Kitchen & Diner", Array.Empty<ZoneDevice>());

        store.Current!.Zones.Should().ContainSingle(z => z.Name == "Kitchen & Diner");
    }

    [Fact]
    public void Remove_PersistsTheRemoval()
    {
        var store = new InMemoryStateStore<ZonesState>();
        var registry = new ZoneRegistry(store);
        var zone = registry.Create("Kitchen", Array.Empty<ZoneDevice>())!;

        registry.Remove(zone.Id);

        store.Current!.Zones.Should().BeEmpty();
    }

    [Fact]
    public void PersistedZones_SurviveIntoANewRegistryInstance()
    {
        var store = new InMemoryStateStore<ZonesState>();
        new ZoneRegistry(store).Create("Kitchen", Array.Empty<ZoneDevice>());

        var reloaded = new ZoneRegistry(store);

        reloaded.GetAll().Should().ContainSingle(z => z.Name == "Kitchen");
    }
}
