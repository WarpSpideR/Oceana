using NAudio.Wave;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// Binds an ordered set of source channels to the buffer that feeds a single output device.
/// </summary>
public sealed class ChannelRoute
{
    /// <summary>
    /// Initialises a new instance of the <see cref="ChannelRoute"/> class.
    /// </summary>
    /// <param name="sourceChannels">The ordered source channel indices routed to the destination.</param>
    /// <param name="destination">The buffer that feeds the output device for these channels.</param>
    public ChannelRoute(IReadOnlyList<int> sourceChannels, BufferedWaveProvider destination)
    {
        SourceChannels = sourceChannels;
        Destination = destination;
    }

    /// <summary>
    /// Gets the ordered source channel indices routed to the destination.
    /// </summary>
    public IReadOnlyList<int> SourceChannels { get; }

    /// <summary>
    /// Gets the buffer that feeds the output device for these channels.
    /// </summary>
    public BufferedWaveProvider Destination { get; }
}
