namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// De-interleaves an incoming interleaved PCM stream and appends each output's channel subset to
/// that output's buffer. Reads need not fall on frame boundaries — a partial trailing frame is
/// carried over to the next call.
/// </summary>
public sealed class ChannelRouter
{
    private readonly int bytesPerSample;
    private readonly int blockAlign;
    private readonly IReadOnlyList<ChannelRoute> routes;
    private readonly byte[][] staging;
    private readonly byte[] remainder;
    private int remainderLength;

    /// <summary>
    /// Initialises a new instance of the <see cref="ChannelRouter"/> class.
    /// </summary>
    /// <param name="sourceChannels">The number of channels in the incoming interleaved stream.</param>
    /// <param name="bytesPerSample">The number of bytes in a single sample (for example, 2 for 16-bit PCM).</param>
    /// <param name="routes">The outputs and the ordered source channels routed to each.</param>
    /// <exception cref="ArgumentOutOfRangeException">A route references a channel outside the source range.</exception>
    public ChannelRouter(int sourceChannels, int bytesPerSample, IReadOnlyList<ChannelRoute> routes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sourceChannels, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(bytesPerSample, 1);

        this.bytesPerSample = bytesPerSample;
        this.routes = routes;
        blockAlign = sourceChannels * bytesPerSample;
        remainder = new byte[blockAlign];
        staging = new byte[routes.Count][];

        for (var i = 0; i < routes.Count; i++)
        {
            staging[i] = Array.Empty<byte>();
            foreach (var channel in routes[i].SourceChannels)
            {
                if (channel < 0 || channel >= sourceChannels)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(routes),
                        $"Source channel {channel} is out of range for a {sourceChannels}-channel stream.");
                }
            }
        }
    }

    /// <summary>
    /// De-interleaves the supplied bytes and appends each output's channels to its buffer.
    /// </summary>
    /// <param name="data">A span of interleaved source bytes; it need not align to frame boundaries.</param>
    public void Route(ReadOnlySpan<byte> data)
    {
        if (remainderLength > 0)
        {
            var take = Math.Min(blockAlign - remainderLength, data.Length);
            data[..take].CopyTo(remainder.AsSpan(remainderLength));
            remainderLength += take;
            data = data[take..];

            if (remainderLength < blockAlign)
            {
                return;
            }

            WriteFrames(remainder, 1);
            remainderLength = 0;
        }

        var frameCount = data.Length / blockAlign;
        if (frameCount > 0)
        {
            WriteFrames(data[..(frameCount * blockAlign)], frameCount);
        }

        var tail = data.Length - (frameCount * blockAlign);
        if (tail > 0)
        {
            data[^tail..].CopyTo(remainder);
            remainderLength = tail;
        }
    }

    private void WriteFrames(ReadOnlySpan<byte> frames, int frameCount)
    {
        for (var r = 0; r < routes.Count; r++)
        {
            var route = routes[r];
            var channels = route.SourceChannels;
            var required = frameCount * channels.Count * bytesPerSample;
            if (staging[r].Length < required)
            {
                staging[r] = new byte[required];
            }

            var stage = staging[r];
            var outPos = 0;
            for (var f = 0; f < frameCount; f++)
            {
                var frameBase = f * blockAlign;
                foreach (var channel in channels)
                {
                    frames.Slice(frameBase + (channel * bytesPerSample), bytesPerSample).CopyTo(stage.AsSpan(outPos));
                    outPos += bytesPerSample;
                }
            }

            route.Destination.AddSamples(stage, 0, required);
        }
    }
}
