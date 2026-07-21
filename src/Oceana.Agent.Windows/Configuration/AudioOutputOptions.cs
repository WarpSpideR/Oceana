namespace Oceana.Agent.Windows.Configuration;

/// <summary>
/// Configuration for a single output device and the source channels routed to it.
/// </summary>
public sealed class AudioOutputOptions
{
    /// <summary>
    /// Gets or sets the output device to play to, matched by its WASAPI friendly name or device id.
    /// When null or empty, the system default render device is used.
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// Gets or sets the ordered source channel indices routed to this device.
    /// The order defines the output channel order, so channels may be reordered or duplicated.
    /// </summary>
    public int[] Channels { get; set; } = Array.Empty<int>();
}
