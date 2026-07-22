namespace Oceana.Server.Features.Zones;

/// <summary>
/// A named group of audio devices (drawn from one or more agents) that audio can be streamed to together.
/// </summary>
public sealed record ZoneInfo
{
    /// <summary>
    /// Gets the stable identifier of the zone.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the human-readable name of the zone.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the devices assigned to the zone.
    /// </summary>
    public IReadOnlyList<ZoneDevice> Devices { get; init; } = Array.Empty<ZoneDevice>();

    /// <summary>
    /// Gets the playback volume applied to audio streamed to the zone, from 0.0 (silent) to
    /// 1.0 (full). Attenuation only.
    /// </summary>
    public double Volume { get; init; } = 1.0;
}
