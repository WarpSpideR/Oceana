using Oceana.Contracts;
using Oceana.Server.Infrastructure.Persistence;

namespace Oceana.Server.Features.Agents;

public class AgentRegistryPersistenceTests
{
    [Fact]
    public void Constructor_LoadsKnownAgentsAsOffline_WithRouting()
    {
        var id = Guid.NewGuid();
        var routing = new AgentRouting(new[] { new AudioOutput("dev-1", new[] { 0 }) });
        var store = new InMemoryStateStore<AgentsState>(new AgentsState(new[]
        {
            new AgentConfig(id, "living-room", new[] { new AudioDevice("dev-1", "Speakers") }, routing),
        }));

        var registry = new AgentRegistry(store);

        var agent = registry.Get(id);
        agent.Should().NotBeNull();
        agent!.Connected.Should().BeFalse();
        agent.Name.Should().Be("living-room");
        agent.Routing.Outputs.Should().ContainSingle(o => o.Device == "dev-1");
    }

    [Fact]
    public void RegisterOrUpdate_PersistsTheAgentConfig()
    {
        var store = new InMemoryStateStore<AgentsState>();
        var registry = new AgentRegistry(store);
        var id = Guid.NewGuid();

        registry.RegisterOrUpdate(Registration(id, "living-room"), "192.168.1.5", "conn-1");

        store.Current.Should().NotBeNull();
        store.Current!.Agents.Should().ContainSingle(a => a.Id == id && a.Name == "living-room");
    }

    [Fact]
    public void SetDesiredRouting_PersistsTheRouting()
    {
        var store = new InMemoryStateStore<AgentsState>();
        var registry = new AgentRegistry(store);
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");

        registry.SetDesiredRouting(id, new AgentRouting(new[] { new AudioOutput("dev-1", new[] { 0 }) }));

        store.Current!.Agents.Should().ContainSingle(a => a.Id == id && a.Routing.Outputs.Count == 1);
    }

    [Fact]
    public void ReconnectingAKnownAgent_PreservesPersistedRouting()
    {
        var id = Guid.NewGuid();
        var routing = new AgentRouting(new[] { new AudioOutput("dev-1", new[] { 0, 1 }) });
        var store = new InMemoryStateStore<AgentsState>(new AgentsState(new[]
        {
            new AgentConfig(id, "living-room", Array.Empty<AudioDevice>(), routing),
        }));
        var registry = new AgentRegistry(store);

        var reconnected = registry.RegisterOrUpdate(Registration(id, "renamed"), "host", "conn-1");

        reconnected.Connected.Should().BeTrue();
        reconnected.Routing.Outputs.Should().ContainSingle(o => o.Device == "dev-1");
    }

    [Fact]
    public void Remove_PersistsTheRemoval()
    {
        var store = new InMemoryStateStore<AgentsState>();
        var registry = new AgentRegistry(store);
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");

        registry.Remove(id);

        store.Current!.Agents.Should().BeEmpty();
    }

    private static AgentRegistration Registration(Guid id, string name = "agent", int port = 8090)
    {
        return new AgentRegistration(id, name, port, Array.Empty<AudioDevice>());
    }
}
