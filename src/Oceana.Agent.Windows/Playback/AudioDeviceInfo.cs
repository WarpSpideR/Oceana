namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// Identifies a render device by its stable id and human-readable name.
/// </summary>
public sealed class AudioDeviceInfo
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AudioDeviceInfo"/> class.
    /// </summary>
    /// <param name="id">The stable device id.</param>
    /// <param name="name">The device friendly name.</param>
    public AudioDeviceInfo(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the stable device id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the device friendly name.
    /// </summary>
    public string Name { get; }
}
