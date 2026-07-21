using System.Buffers.Binary;
using NAudio.Wave;

namespace Oceana.Agent.Windows.Playback;

public class ChannelRouterTests
{
    [Fact]
    public void Route_ExtractsChannelSubset()
    {
        var (buffer, route) = CreateRoute([0, 1]);
        var router = new ChannelRouter(4, 2, [route]);

        router.Route(Frames([[1, 2, 3, 4], [5, 6, 7, 8]]));

        ReadSamples(buffer).Should().Equal(1, 2, 5, 6);
    }

    [Fact]
    public void Route_ReordersChannels()
    {
        var (buffer, route) = CreateRoute([1, 0]);
        var router = new ChannelRouter(4, 2, [route]);

        router.Route(Frames([[1, 2, 3, 4], [5, 6, 7, 8]]));

        ReadSamples(buffer).Should().Equal(2, 1, 6, 5);
    }

    [Fact]
    public void Route_ExtractsSingleChannel()
    {
        var (buffer, route) = CreateRoute([2]);
        var router = new ChannelRouter(4, 2, [route]);

        router.Route(Frames([[1, 2, 3, 4], [5, 6, 7, 8]]));

        ReadSamples(buffer).Should().Equal(3, 7);
    }

    [Fact]
    public void Route_FansOutToMultipleOutputs()
    {
        var (bufferA, routeA) = CreateRoute([0, 1]);
        var (bufferB, routeB) = CreateRoute([2, 3]);
        var router = new ChannelRouter(4, 2, [routeA, routeB]);

        router.Route(Frames([[1, 2, 3, 4], [5, 6, 7, 8]]));

        ReadSamples(bufferA).Should().Equal(1, 2, 5, 6);
        ReadSamples(bufferB).Should().Equal(3, 4, 7, 8);
    }

    [Fact]
    public void Route_HandlesReadsNotAlignedToFrames()
    {
        var (buffer, route) = CreateRoute([0, 1]);
        var router = new ChannelRouter(4, 2, [route]);
        var all = Frames([[1, 2, 3, 4], [5, 6, 7, 8]]); // 16 bytes, 8-byte frames

        router.Route(all.AsSpan(0, 5)); // partial first frame
        router.Route(all.AsSpan(5)); // completes frame 0 and delivers frame 1

        ReadSamples(buffer).Should().Equal(1, 2, 5, 6);
    }

    [Fact]
    public void Constructor_RejectsChannelOutOfRange()
    {
        var (_, route) = CreateRoute([0, 4]);

        var act = () => new ChannelRouter(4, 2, [route]);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static (BufferedWaveProvider Buffer, ChannelRoute Route) CreateRoute(int[] channels)
    {
        var buffer = new BufferedWaveProvider(new WaveFormat(48000, 16, Math.Max(channels.Length, 1)), TimeSpan.FromSeconds(1));
        return (buffer, new ChannelRoute(channels, buffer));
    }

    private static byte[] Frames(int[][] frames)
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
}
