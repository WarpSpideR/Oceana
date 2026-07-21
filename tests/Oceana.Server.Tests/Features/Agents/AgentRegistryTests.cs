namespace Oceana.Server.Features.Agents;

public class AgentRegistryTests
{
    [Fact]
    public void Register_AssignsIdAndIdleStatus()
    {
        var registry = new AgentRegistry();

        var agent = registry.Register("living-room", "192.168.1.50", 8090);

        agent.Id.Should().NotBe(Guid.Empty);
        agent.Name.Should().Be("living-room");
        agent.Host.Should().Be("192.168.1.50");
        agent.Port.Should().Be(8090);
        agent.Status.Should().Be(AgentStatus.Idle);
    }

    [Fact]
    public void GetAll_ReturnsEveryRegisteredAgent()
    {
        var registry = new AgentRegistry();
        registry.Register("a", "host-a", 8090);
        registry.Register("b", "host-b", 8091);

        registry.GetAll().Should().HaveCount(2);
    }

    [Fact]
    public void Get_ReturnsNull_ForUnknownId()
    {
        var registry = new AgentRegistry();

        registry.Get(Guid.NewGuid()).Should().BeNull();
    }

    [Fact]
    public void Remove_RemovesAgent()
    {
        var registry = new AgentRegistry();
        var agent = registry.Register("a", "host-a", 8090);

        registry.Remove(agent.Id).Should().BeTrue();
        registry.Get(agent.Id).Should().BeNull();
        registry.Remove(agent.Id).Should().BeFalse();
    }

    [Fact]
    public void UpdateStatus_ChangesStatusAndError()
    {
        var registry = new AgentRegistry();
        var agent = registry.Register("a", "host-a", 8090);

        var updated = registry.UpdateStatus(agent.Id, AgentStatus.Faulted, "boom");

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(AgentStatus.Faulted);
        updated.LastError.Should().Be("boom");
        registry.Get(agent.Id)!.Status.Should().Be(AgentStatus.Faulted);
    }

    [Fact]
    public void UpdateStatus_ReturnsNull_ForUnknownId()
    {
        var registry = new AgentRegistry();

        registry.UpdateStatus(Guid.NewGuid(), AgentStatus.Streaming).Should().BeNull();
    }
}
