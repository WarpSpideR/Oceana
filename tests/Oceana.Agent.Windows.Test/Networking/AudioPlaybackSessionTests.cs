using NAudio.Wave;
using Oceana.Agent.Windows.Networking;
using Oceana.Agent.Windows.Playback;
using Oceana.Agent.Windows.Protocol;
using Serilog;

namespace Oceana.Agent.Windows.Test.Networking;

public class AudioPlaybackSessionTests
{
    [Fact]
    public async Task RunAsync_PlaysHeaderThenPumpsAllBytesIntoBuffer()
    {
        var audio = CreateSampleBytes(4000);
        using var stream = CreateStream(new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16), audio);

        IWaveProvider? captured = null;
        var player = Substitute.For<IAudioPlayer>();
        player.When(p => p.Init(Arg.Any<IWaveProvider>())).Do(call => captured = call.Arg<IWaveProvider>());
        var factory = Substitute.For<IAudioPlayerFactory>();
        factory.Create().Returns(player);

        var session = new AudioPlaybackSession(factory, Substitute.For<ILogger>());

        await session.RunAsync(stream, CancellationToken.None);

        player.Received(1).Init(Arg.Any<IWaveProvider>());
        player.Received(1).Play();
        player.Received(1).Stop();
        player.Received(1).Dispose();
        captured.Should().BeOfType<BufferedWaveProvider>();
        ((BufferedWaveProvider)captured!).BufferedBytes.Should().Be(audio.Length);
    }

    [Fact]
    public async Task RunAsync_WhenAlreadyCancelled_ThrowsAndNeverCreatesPlayer()
    {
        var audio = CreateSampleBytes(1000);
        using var stream = CreateStream(new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16), audio);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var factory = Substitute.For<IAudioPlayerFactory>();
        var session = new AudioPlaybackSession(factory, Substitute.For<ILogger>());

        var act = () => session.RunAsync(stream, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        factory.DidNotReceive().Create();
    }

    private static byte[] CreateSampleBytes(int length)
    {
        var data = new byte[length];
        for (var i = 0; i < length; i++)
        {
            data[i] = (byte)(i % 256);
        }

        return data;
    }

    private static MemoryStream CreateStream(AudioStreamHeader header, byte[] audio)
    {
        var headerBytes = new byte[AudioStreamHeader.Size];
        header.Write(headerBytes);

        var stream = new MemoryStream();
        stream.Write(headerBytes);
        stream.Write(audio);
        stream.Position = 0;
        return stream;
    }
}
