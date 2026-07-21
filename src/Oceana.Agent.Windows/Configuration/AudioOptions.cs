namespace Oceana.Agent.Windows.Configuration;

/// <summary>
/// Agent audio configuration, bound from the <c>Audio</c> configuration section.
/// </summary>
public sealed class AudioOptions
{
    /// <summary>
    /// The name of the configuration section these options are bound from.
    /// </summary>
    public const string SectionName = "Audio";

    /// <summary>
    /// Gets or sets the configured output devices and the source channels routed to each.
    /// When empty, the entire incoming stream is played to the system default output device.
    /// </summary>
    public IList<AudioOutputOptions> Outputs { get; set; } = new List<AudioOutputOptions>();
}
