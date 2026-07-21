using NAudio.CoreAudioApi;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// Resolves configured device identifiers to WASAPI render devices, and lists the available devices.
/// </summary>
public interface IAudioDeviceResolver
{
    /// <summary>
    /// Resolves a configured device identifier to a render device.
    /// </summary>
    /// <param name="deviceIdOrName">The device id or friendly name; null or empty selects the system default render device.</param>
    /// <returns>The resolved render device; the caller takes ownership and must dispose it.</returns>
    MMDevice Resolve(string? deviceIdOrName);

    /// <summary>
    /// Lists the currently active render devices.
    /// </summary>
    /// <returns>The active render devices.</returns>
    IReadOnlyList<AudioDeviceInfo> ListRenderDevices();
}
