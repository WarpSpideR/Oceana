using Microsoft.Extensions.Logging.Abstractions;
using Oceana.Contracts;
using Oceana.Protocol;
using Oceana.Server.Features.Agents;
using Oceana.Server.Features.Zones;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Infrastructure.Streaming;

public class ZonePlaybackServiceTests
{
    private static readonly StreamFormat StereoFormat = new StreamFormat(AudioEncoding.Pcm, 2, 48000, 16);
    private static readonly ReadOnlyMemory<byte> Pcm = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

    private readonly IAgentRegistry registry = Substitute.For<IAgentRegistry>();
    private readonly IZoneRegistry zoneRegistry = Substitute.For<IZoneRegistry>();
    private readonly IAudioStreamManager streamManager = Substitute.For<IAudioStreamManager>();
    private readonly IAgentRoutingCommander commander = Substitute.For<IAgentRoutingCommander>();
    private readonly IZoneStatusNotifier notifier = Substitute.For<IZoneStatusNotifier>();

    private ZonePlaybackService CreateService() =>
        new ZonePlaybackService(registry, zoneRegistry, streamManager, commander, notifier, NullLogger<ZonePlaybackService>.Instance);

    [Fact]
    public void Start_SkipsOfflineAgent_AndReportsNotPlaying()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: false, "d1"));
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        var state = CreateService().Start(zone, "track.wav", Pcm, StereoFormat);

        state.Playing.Should().BeFalse();
        state.Targeted.Should().BeEmpty();
        state.Skipped.Should().ContainSingle(s => s.Reason == BroadcastSkipReason.Offline);
    }

    [Fact]
    public void Start_SkipsBusyAgent()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true, "d1"));
        streamManager.IsStreaming(agentId).Returns(true);
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        var state = CreateService().Start(zone, "track.wav", Pcm, StereoFormat);

        state.Skipped.Should().ContainSingle(s => s.Reason == BroadcastSkipReason.Busy);
    }

    [Fact]
    public void Start_SkipsAgentWithNoReportedDevice()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true)); // reports no devices
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        var state = CreateService().Start(zone, "track.wav", Pcm, StereoFormat);

        state.Skipped.Should().ContainSingle(s => s.Reason == BroadcastSkipReason.NoActiveDevices);
    }

    [Fact]
    public void Start_TargetsReachableAgent_AndIsNowPlaying()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true, "d1"));
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });
        var service = CreateService();

        var state = service.Start(zone, "track.wav", Pcm, StereoFormat);

        state.Playing.Should().BeTrue();
        state.SourceName.Should().Be("track.wav");
        state.Targeted.Should().ContainSingle(t => t.AgentId == agentId);
        service.Get(zone.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task Start_PushesAllSourceChannelsToDevices_ThenStreams()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true, "d1"));
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });

        CreateService().Start(zone, "track.wav", Pcm, StereoFormat);
        await Task.Delay(500);

        await commander.Received().PushRoutingAsync(
            agentId,
            Arg.Is<AgentRouting>(r =>
                r.Outputs.Count == 1
                && r.Outputs[0].Device == "d1"
                && r.Outputs[0].Channels.Length == 2
                && r.Outputs[0].Channels[0] == 0
                && r.Outputs[0].Channels[1] == 1));
        await streamManager.Received().TryStreamPcmAsync(
            agentId,
            Arg.Any<ReadOnlyMemory<byte>>(),
            StereoFormat,
            Arg.Any<Func<double>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Stop_CancelsAndClearsPlayback()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true, "d1"));
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });
        var service = CreateService();
        service.Start(zone, "track.wav", Pcm, StereoFormat);

        service.Stop(zone.Id).Should().BeTrue();
        service.Get(zone.Id).Should().BeNull();
        service.Stop(zone.Id).Should().BeFalse();
    }

    [Fact]
    public void Start_ReplacesAnExistingPlayback()
    {
        var agentId = Guid.NewGuid();
        registry.Get(agentId).Returns(Agent(agentId, connected: true, "d1"));
        var zone = Zone(new ZoneDevice { AgentId = agentId, DeviceId = "d1" });
        var service = CreateService();

        service.Start(zone, "first.wav", Pcm, StereoFormat);
        var second = service.Start(zone, "second.wav", Pcm, StereoFormat);

        second.SourceName.Should().Be("second.wav");
        service.Get(zone.Id)!.SourceName.Should().Be("second.wav");
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
