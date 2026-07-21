namespace Oceana.Contracts;

/// <summary>
/// Identifies an audio render device available on an agent.
/// </summary>
public sealed class AudioDevice
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AudioDevice"/> class.
    /// </summary>
    /// <param name="id">The stable device id.</param>
    /// <param name="name">The human-readable device name.</param>
    public AudioDevice(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the stable device id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the human-readable device name.
    /// </summary>
    public string Name { get; }
}
