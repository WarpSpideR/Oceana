namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// Creates <see cref="IAudioPlayer"/> instances bound to specific output devices.
/// </summary>
public interface IAudioPlayerFactory
{
    /// <summary>
    /// Creates a new, uninitialised audio player bound to the given device.
    /// </summary>
    /// <param name="deviceId">The device id or friendly name; null selects the system default render device.</param>
    /// <returns>A new <see cref="IAudioPlayer"/> instance.</returns>
    IAudioPlayer Create(string? deviceId);
}
