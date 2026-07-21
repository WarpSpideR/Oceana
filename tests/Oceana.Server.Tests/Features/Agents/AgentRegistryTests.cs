using Oceana.Contracts;

namespace Oceana.Server.Features.Agents;

public class AgentRegistryTests
{
    [Fact]
    public void RegisterOrUpdate_CreatesConnectedAgent()
    {
        var registry = new AgentRegistry();
        var id = Guid.NewGuid();

        var agent = registry.RegisterOrUpdate(Registration(id, "living-room"), "192.168.1.50", "conn-1");

        agent.Id.Should().Be(id);
        agent.Name.Should().Be("living-room");
        agent.Host.Should().Be("192.168.1.50");
        agent.Port.Should().Be(8090);
        agent.Connected.Should().BeTrue();
        agent.Status.Should().Be(AgentStatus.Idle);
    }

    [Fact]
    public void RegisterOrUpdate_PreservesRoutingAcrossReRegistration()
    {
        var registry = new AgentRegistry();
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");
        registry.SetDesiredRouting(id, new AgentRouting(new[] { new AudioOutput("A", new[] { 0, 1 }) }));

        var reRegistered = registry.RegisterOrUpdate(Registration(id, "renamed"), "host-2", "conn-2");

        reRegistered.Name.Should().Be("renamed");
        reRegistered.Host.Should().Be("host-2");
        reRegistered.Routing.Outputs.Should().ContainSingle();
    }

    [Fact]
    public void MarkOffline_MarksTheAgentOffline()
    {
        var registry = new AgentRegistry();
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");

        var offline = registry.MarkOffline("conn-1");

        offline.Should().NotBeNull();
        offline!.Connected.Should().BeFalse();
        registry.Get(id)!.Connected.Should().BeFalse();
    }

    [Fact]
    public void MarkOffline_IgnoresAStaleConnectionAfterReconnect()
    {
        var registry = new AgentRegistry();
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");
        registry.RegisterOrUpdate(Registration(id), "host", "conn-2"); // reconnected on a new connection

        // The old connection dropping must not knock the reconnected agent offline.
        registry.MarkOffline("conn-1").Should().BeNull();
        registry.Get(id)!.Connected.Should().BeTrue();

        // The current connection dropping does.
        registry.MarkOffline("conn-2").Should().NotBeNull();
        registry.Get(id)!.Connected.Should().BeFalse();
    }

    [Fact]
    public void SetDesiredRouting_StoresRouting()
    {
        var registry = new AgentRegistry();
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");

        var updated = registry.SetDesiredRouting(id, new AgentRouting(new[] { new AudioOutput(null, new[] { 0 }) }));

        updated.Should().NotBeNull();
        updated!.Routing.Outputs.Should().ContainSingle();
        registry.Get(id)!.Routing.Outputs.Should().ContainSingle();
    }

    [Fact]
    public void SetDesiredRouting_ReturnsNull_ForUnknownId()
    {
        new AgentRegistry()
            .SetDesiredRouting(Guid.NewGuid(), new AgentRouting(Array.Empty<AudioOutput>()))
            .Should().BeNull();
    }

    [Fact]
    public void GetAll_ReturnsEveryRegisteredAgent()
    {
        var registry = new AgentRegistry();
        registry.RegisterOrUpdate(Registration(Guid.NewGuid(), "a"), "host-a", "conn-1");
        registry.RegisterOrUpdate(Registration(Guid.NewGuid(), "b"), "host-b", "conn-2");

        registry.GetAll().Should().HaveCount(2);
    }

    [Fact]
    public void Remove_RemovesAgent()
    {
        var registry = new AgentRegistry();
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");

        registry.Remove(id).Should().BeTrue();
        registry.Get(id).Should().BeNull();
        registry.Remove(id).Should().BeFalse();
    }

    [Fact]
    public void UpdateStatus_ChangesStatusAndError()
    {
        var registry = new AgentRegistry();
        var id = Guid.NewGuid();
        registry.RegisterOrUpdate(Registration(id), "host", "conn-1");

        var updated = registry.UpdateStatus(id, AgentStatus.Faulted, "boom");

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(AgentStatus.Faulted);
        updated.LastError.Should().Be("boom");
    }

    private static AgentRegistration Registration(Guid id, string name = "agent", int port = 8090)
    {
        return new AgentRegistration(id, name, port, Array.Empty<AudioDevice>());
    }
}
