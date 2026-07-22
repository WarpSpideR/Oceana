using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Oceana.Contracts;
using Oceana.Server.Features.Agents;
using Oceana.Server.Features.Zones;

namespace Oceana.Server.Infrastructure.Persistence;

public sealed class JsonStateStoreTests : IDisposable
{
    private readonly string directory;

    public JsonStateStoreTests()
    {
        directory = Path.Combine(Path.GetTempPath(), $"oceana-store-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
            // Best-effort cleanup of the temp directory.
        }
    }

    [Fact]
    public void Load_ReturnsNull_WhenFileDoesNotExist()
    {
        Store<ZonesState>("missing.json").Load().Should().BeNull();
    }

    [Fact]
    public void Save_ThenLoad_RoundTripsZones()
    {
        var store = Store<ZonesState>("zones.json");
        var state = new ZonesState(new[]
        {
            new ZoneInfo
            {
                Id = Guid.NewGuid(),
                Name = "Kitchen",
                Devices = new[] { new ZoneDevice { AgentId = Guid.NewGuid(), DeviceId = "dev-1" } },
            },
        });

        store.Save(state);
        var loaded = store.Load();

        loaded.Should().NotBeNull();
        loaded!.Zones.Should().ContainSingle();
        loaded.Zones[0].Name.Should().Be("Kitchen");
        loaded.Zones[0].Devices.Should().ContainSingle(d => d.DeviceId == "dev-1");
    }

    [Fact]
    public void Save_ThenLoad_RoundTripsAgentConfigIncludingRouting()
    {
        var store = Store<AgentsState>("agents.json");
        var id = Guid.NewGuid();
        var state = new AgentsState(new[]
        {
            new AgentConfig(
                id,
                "living-room",
                new[] { new AudioDevice("dev-1", "Speakers") },
                new AgentRouting(new[] { new AudioOutput("dev-1", new[] { 0 }) })),
        });

        store.Save(state);
        var loaded = store.Load();

        loaded.Should().NotBeNull();
        var agent = loaded!.Agents.Should().ContainSingle().Subject;
        agent.Id.Should().Be(id);
        agent.Name.Should().Be("living-room");
        agent.Devices.Should().ContainSingle(d => d.Id == "dev-1" && d.Name == "Speakers");
        agent.Routing.Outputs.Should().ContainSingle();
        agent.Routing.Outputs[0].Device.Should().Be("dev-1");
        agent.Routing.Outputs[0].Channels.Should().Equal(0);
    }

    [Fact]
    public void Save_OverwritesPreviousState()
    {
        var store = Store<ZonesState>("zones.json");
        store.Save(new ZonesState(new[] { Zone("Kitchen") }));
        store.Save(new ZonesState(new[] { Zone("Lounge") }));

        store.Load()!.Zones.Should().ContainSingle(z => z.Name == "Lounge");
    }

    [Fact]
    public void Load_ReturnsNull_WhenFileIsCorrupt()
    {
        var path = Path.Combine(directory, "zones.json");
        File.WriteAllText(path, "{ not valid json");

        Store<ZonesState>("zones.json").Load().Should().BeNull();
    }

    private static ZoneInfo Zone(string name) =>
        new ZoneInfo { Id = Guid.NewGuid(), Name = name, Devices = Array.Empty<ZoneDevice>() };

    private JsonStateStore<T> Store<T>(string fileName)
        where T : class =>
        new JsonStateStore<T>(Path.Combine(directory, fileName), NullLogger<JsonStateStore<T>>.Instance);
}
