namespace Oceana.Contracts;

/// <summary>
/// Routes an ordered set of source channels to one output device.
/// </summary>
public sealed class AudioOutput
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AudioOutput"/> class.
    /// </summary>
    /// <param name="device">The target device id or friendly name; null selects the system default render device.</param>
    /// <param name="channels">The ordered source channel indices routed to the device.</param>
    public AudioOutput(string? device, int[] channels)
    {
        Device = device;
        Channels = channels;
    }

    /// <summary>
    /// Gets the target device id or friendly name; null selects the system default render device.
    /// </summary>
    public string? Device { get; }

    /// <summary>
    /// Gets the ordered source channel indices routed to the device.
    /// </summary>
    public int[] Channels { get; }
}
