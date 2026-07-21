using System.Buffers.Binary;
using NAudio.Wave;
using Oceana.Agent.Windows.Configuration;
using Oceana.Agent.Windows.Playback;
using Oceana.Protocol;
using Serilog;

namespace Oceana.Agent.Windows.Networking;

public class AudioPlaybackSessionTests
{
    [Fact]
    public async Task RunAsync_WithoutConfig_PlaysAllChannelsToDefaultDevice()
    {
        var audio = CreateSampleBytes(4000);
        using var stream = CreateStream(new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16), audio);

        BufferedWaveProvider? captured = null;
        var player = Substitute.For<IAudioPlayer>();
        player.When(p => p.Init(Arg.Any<IWaveProvider>())).Do(call => captured = (BufferedWaveProvider)call.Arg<IWaveProvider>()!);
        var factory = Substitute.For<IAudioPlayerFactory>();
        factory.Create(Arg.Any<string?>()).Returns(player);

        var session = new AudioPlaybackSession(factory, Array.Empty<AudioOutputOptions>(), Substitute.For<ILogger>());
        await session.RunAsync(stream, CancellationToken.None);

        factory.Received(1).Create(null); // default device
        player.Received(1).Play();
        player.Received(1).Stop();
        captured.Should().NotBeNull();
        captured!.BufferedBytes.Should().Be(audio.Length); // 2-channel identity route preserves the bytes
    }

    [Fact]
    public async Task RunAsync_SplitsFourChannelsAcrossTwoDevices()
    {
        var header = new AudioStreamHeader(AudioEncoding.Pcm, 4, 48000, 16);
        var audio = BuildFrames([[1, 2, 3, 4], [5, 6, 7, 8]]);
        using var stream = CreateStream(header, audio);

        var buffers = new Dictionary<string, BufferedWaveProvider>();
        var factory = Substitute.For<IAudioPlayerFactory>();
        factory.Create(Arg.Any<string?>()).Returns(call =>
        {
            var deviceId = call.Arg<string?>() ?? "(default)";
            var player = Substitute.For<IAudioPlayer>();
            player.When(p => p.Init(Arg.Any<IWaveProvider>()))
                .Do(init => buffers[deviceId] = (BufferedWaveProvider)init.Arg<IWaveProvider>()!);
            return player;
        });

        var outputs = new[]
        {
            new AudioOutputOptions { Device = "A", Channels = [0, 1] },
            new AudioOutputOptions { Device = "B", Channels = [2, 3] },
        };
        var session = new AudioPlaybackSession(factory, outputs, Substitute.For<ILogger>());

        await session.RunAsync(stream, CancellationToken.None);

        ReadSamples(buffers["A"]).Should().Equal(1, 2, 5, 6); // channels 0 and 1
        ReadSamples(buffers["B"]).Should().Equal(3, 4, 7, 8); // channels 2 and 3
    }

    [Fact]
    public async Task RunAsync_WithChannelOutOfRange_Throws()
    {
        var audio = BuildFrames([[1, 2], [3, 4]]);
        using var stream = CreateStream(new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16), audio);
        var factory = Substitute.For<IAudioPlayerFactory>();
        var outputs = new[] { new AudioOutputOptions { Device = "A", Channels = [0, 2] } }; // channel 2 invalid for 2 channels
        var session = new AudioPlaybackSession(factory, outputs, Substitute.For<ILogger>());

        var act = () => session.RunAsync(stream, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RunAsync_WhenAlreadyCancelled_ThrowsAndNeverCreatesPlayer()
    {
        var audio = CreateSampleBytes(1000);
        using var stream = CreateStream(new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16), audio);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var factory = Substitute.For<IAudioPlayerFactory>();
        var session = new AudioPlaybackSession(factory, Array.Empty<AudioOutputOptions>(), Substitute.For<ILogger>());

        var act = () => session.RunAsync(stream, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        factory.DidNotReceive().Create(Arg.Any<string?>());
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

    private static byte[] BuildFrames(int[][] frames)
    {
        var channels = frames[0].Length;
        var data = new byte[frames.Length * channels * 2];
        var offset = 0;
        foreach (var frame in frames)
        {
            foreach (var sample in frame)
            {
                BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(offset), (short)sample);
                offset += 2;
            }
        }

        return data;
    }

    private static int[] ReadSamples(BufferedWaveProvider buffer)
    {
        var bytes = new byte[buffer.BufferedBytes];
        var read = buffer.Read(bytes);
        var samples = new int[read / 2];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(i * 2));
        }

        return samples;
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
