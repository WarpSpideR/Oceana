namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// A resolved output: the target device and the ordered source channels routed to it.
/// </summary>
public sealed class OutputPlan
{
    /// <summary>
    /// Initialises a new instance of the <see cref="OutputPlan"/> class.
    /// </summary>
    /// <param name="deviceId">The target device id or friendly name; null selects the system default render device.</param>
    /// <param name="sourceChannels">The ordered source channel indices routed to the device.</param>
    public OutputPlan(string? deviceId, IReadOnlyList<int> sourceChannels)
    {
        DeviceId = deviceId;
        SourceChannels = sourceChannels;
    }

    /// <summary>
    /// Gets the target device id or friendly name; null selects the system default render device.
    /// </summary>
    public string? DeviceId { get; }

    /// <summary>
    /// Gets the ordered source channel indices routed to the device.
    /// </summary>
    public IReadOnlyList<int> SourceChannels { get; }
}
