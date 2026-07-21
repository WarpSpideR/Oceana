using Microsoft.Extensions.Logging.Abstractions;
using Oceana.Protocol;
using Oceana.Server.Features.Agents;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Infrastructure.Streaming;

public class AudioStreamManagerTests
{
    [Fact]
    public void TryStartStream_ReturnsFalse_ForUnknownAgent()
    {
        var manager = CreateManager(new AgentRegistry(), out _, out _);

        manager.TryStartStream(Guid.NewGuid(), new ToneOptions()).Should().BeFalse();
    }

    [Fact]
    public void StopStream_ReturnsFalse_WhenNotStreaming()
    {
        var registry = new AgentRegistry();
        var agent = registry.Register("a", "127.0.0.1", 8090);
        var manager = CreateManager(registry, out _, out _);

        manager.StopStream(agent.Id).Should().BeFalse();
    }

    [Fact]
    public async Task TryStartStream_WritesValidHeaderAndReportsStreaming()
    {
        var registry = new AgentRegistry();
        var agent = registry.Register("a", "127.0.0.1", 8090);
        var manager = CreateManager(registry, out var capture, out var notifier);

        manager.TryStartStream(agent.Id, new ToneOptions { Frequency = 440.0 }).Should().BeTrue();

        (await WaitUntilAsync(() => capture.Length >= AudioStreamHeader.Size, TimeSpan.FromSeconds(2)))
            .Should().BeTrue();

        var header = AudioStreamHeader.Parse(capture.Snapshot());
        header.Encoding.Should().Be(AudioEncoding.Pcm);
        header.SampleRate.Should().Be(48000);
        header.Channels.Should().Be(2);
        header.BitsPerSample.Should().Be(16);

        (await WaitUntilAsync(() => registry.Get(agent.Id)!.Status == AgentStatus.Streaming, TimeSpan.FromSeconds(2)))
            .Should().BeTrue();
        await notifier.ReceivedWithAnyArgs().NotifyAgentChangedAsync(default!);

        manager.StopStream(agent.Id).Should().BeTrue();
        (await WaitUntilAsync(() => registry.Get(agent.Id)!.Status == AgentStatus.Idle, TimeSpan.FromSeconds(2)))
            .Should().BeTrue();
        manager.IsStreaming(agent.Id).Should().BeFalse();
    }

    [Fact]
    public async Task TryStartStream_ReturnsFalse_WhenAlreadyStreaming()
    {
        var registry = new AgentRegistry();
        var agent = registry.Register("a", "127.0.0.1", 8090);
        var manager = CreateManager(registry, out _, out _);

        manager.TryStartStream(agent.Id, new ToneOptions()).Should().BeTrue();
        try
        {
            manager.TryStartStream(agent.Id, new ToneOptions()).Should().BeFalse();
        }
        finally
        {
            manager.StopStream(agent.Id);
            await WaitUntilAsync(() => !manager.IsStreaming(agent.Id), TimeSpan.FromSeconds(2));
        }
    }

    private static AudioStreamManager CreateManager(IAgentRegistry registry, out CaptureStream capture, out IStatusNotifier notifier)
    {
        capture = new CaptureStream();
        var connection = Substitute.For<IAgentConnection>();
        connection.Stream.Returns(capture);
        var factory = Substitute.For<IAgentConnectionFactory>();
        factory.ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(connection);
        notifier = Substitute.For<IStatusNotifier>();
        return new AudioStreamManager(factory, registry, notifier, NullLogger<AudioStreamManager>.Instance);
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(20);
        }

        return condition();
    }

    private sealed class CaptureStream : Stream
    {
        private readonly object gate = new object();
        private readonly MemoryStream inner = new MemoryStream();

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                lock (gate)
                {
                    return inner.Length;
                }
            }
        }

        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }

        public byte[] Snapshot()
        {
            lock (gate)
            {
                return inner.ToArray();
            }
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            lock (gate)
            {
                inner.Write(buffer.Span);
            }

            return ValueTask.CompletedTask;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (gate)
            {
                inner.Write(buffer, offset, count);
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
