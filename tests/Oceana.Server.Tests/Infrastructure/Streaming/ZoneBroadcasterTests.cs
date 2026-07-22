using Microsoft.Extensions.Logging.Abstractions;
using Oceana.Contracts;
using Oceana.Protocol;
using Oceana.Server.Features.Agents;
using Oceana.Server.Features.Zones;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Infrastructure.Streaming;

public class ZoneBroadcasterTests
{
    private static readonly StreamFormat Format = new StreamFormat(AudioEncoding.Pcm, 1, 48000, 16);
    private static readonly ReadOnlyMemory<byte> Pcm = new byte[] { 0, 0, 0, 0 };

    private readonly IAgentRegistry registry = Substitute.For<IAgentRegistry>();
    private readonly IAudioStreamManager streamManager = Substitute.For<IAudioStreamManager>();
    private readonly IAgentRoutingCommander commander = Substitute.For<IAgentRoutingCommander>();

    private ZoneBroadcaster CreateBroadcaster() =>
        new ZoneBroadcaster(registry, streamManager, commander, NullLogger<ZoneBroadcaster>.Instance);

    [Fact]
    public void PlanAndStart_SkipsOfflineAgent()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: false, "d1"));
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        var result = CreateBroadcaster().PlanAndStart(zone, Pcm, Format);

        result.Targeted.Should().BeEmpty();
        result.Skipped.Should().ContainSingle(s => s.AgentId == agentId && s.Reason == BroadcastSkipReason.Offline);
    }

    [Fact]
    public void PlanAndStart_SkipsUnknownAgentAsOffline()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns((AgentInfo?)null);
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        var result = CreateBroadcaster().PlanAndStart(zone, Pcm, Format);

        result.Skipped.Should().ContainSingle(s => s.Reason == BroadcastSkipReason.Offline);
    }

    [Fact]
    public void PlanAndStart_SkipsBusyAgent()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true, "d1"));
        streamManager.IsStreaming(agentId).Returns(true);
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        var result = CreateBroadcaster().PlanAndStart(zone, Pcm, Format);

        result.Skipped.Should().ContainSingle(s => s.AgentId == agentId && s.Reason == BroadcastSkipReason.Busy);
    }

    [Fact]
    public void PlanAndStart_SkipsAgentWithNoReportedDevice()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true)); // reports no devices
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        var result = CreateBroadcaster().PlanAndStart(zone, Pcm, Format);

        result.Skipped.Should().ContainSingle(s => s.Reason == BroadcastSkipReason.NoActiveDevices);
    }

    [Fact]
    public void PlanAndStart_TargetsReachableAgent()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true, "d1", "d2"));
        streamManager.IsStreaming(agentId).Returns(false);
        var zone = Zone(
            new ZoneDevice { AgentId = agentId, DeviceId = "d1" },
            new ZoneDevice { AgentId = agentId, DeviceId = "d2" });

        var result = CreateBroadcaster().PlanAndStart(zone, Pcm, Format);

        result.Skipped.Should().BeEmpty();
        result.Targeted.Should().ContainSingle(t => t.AgentId == agentId && t.DeviceCount == 2);
    }

    [Fact]
    public void PlanAndStart_TargetsOneAgentAndSkipsAnother()
    {
        var reachable = Guid.NewGuid();
        var offline = Guid.NewGuid();
        registry.Get(reachable).Returns(Agent(reachable, connected: true, "d1"));
        registry.Get(offline).Returns(Agent(offline, connected: false, "d2"));
        var zone = Zone(
            new ZoneDevice { AgentId = reachable, DeviceId = "d1" },
            new ZoneDevice { AgentId = offline, DeviceId = "d2" });

        var result = CreateBroadcaster().PlanAndStart(zone, Pcm, Format);

        result.Targeted.Should().ContainSingle(t => t.AgentId == reachable);
        result.Skipped.Should().ContainSingle(s => s.AgentId == offline && s.Reason == BroadcastSkipReason.Offline);
    }

    [Fact]
    public async Task StreamToAgentAsync_PushesTargetRouting_Streams_ThenRestores()
    {
        var agentId = Guid.NewGuid();
        var priorRouting = new AgentRouting(new[] { new AudioOutput(null, new[] { 0, 1 }) });
        var agent = Agent(agentId, connected: true, "d1", "d2") with { Routing = priorRouting };
        registry.Get(agentId).Returns(agent);

        await CreateBroadcaster().StreamToAgentAsync(agent, new[] { "d1", "d2" }, Pcm, Format);

        await commander.Received(1).PushRoutingAsync(
            agentId,
            Arg.Is<AgentRouting>(r =>
                r.Outputs.Count == 2
                && r.Outputs.All(o => o.Channels.Length == 1 && o.Channels[0] == 0)
                && r.Outputs.Any(o => o.Device == "d1")
                && r.Outputs.Any(o => o.Device == "d2")));

        await streamManager.Received(1).TryStreamPcmAsync(
            agentId,
            Arg.Any<ReadOnlyMemory<byte>>(),
            Format,
            Arg.Any<CancellationToken>());

        await commander.Received(1).PushRoutingAsync(agentId, priorRouting);
    }

    private static AgentInfo Agent(Guid id, bool connected, params string[] deviceIds)
    {
        return new AgentInfo
        {
            Id = id,
            Name = $"agent-{id:N}",
            Host = "127.0.0.1",
            Port = 8090,
            Connected = connected,
            Devices = deviceIds.Select(d => new AudioDevice(d, d)).ToArray(),
            Routing = new AgentRouting(Array.Empty<AudioOutput>()),
        };
    }

    private static ZoneInfo Zone(params ZoneDevice[] devices)
    {
        return new ZoneInfo { Id = Guid.NewGuid(), Name = "zone", Devices = devices };
    }
}
