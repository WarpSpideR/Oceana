using NAudio.CoreAudioApi;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// An <see cref="IAudioDeviceResolver"/> backed by the Windows Core Audio (WASAPI) device enumerator.
/// </summary>
public sealed class WasapiAudioDeviceResolver : IAudioDeviceResolver
{
    /// <inheritdoc/>
    public MMDevice Resolve(string? deviceIdOrName)
    {
        using var enumerator = new MMDeviceEnumerator();

        if (string.IsNullOrWhiteSpace(deviceIdOrName))
        {
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        MMDevice? match = null;
        foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            var isMatch = match is null
                && (string.Equals(device.ID, deviceIdOrName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(device.FriendlyName, deviceIdOrName, StringComparison.OrdinalIgnoreCase));

            if (isMatch)
            {
                match = device;
            }
            else
            {
                device.Dispose();
            }
        }

        return match ?? throw new InvalidOperationException($"No active render device matched '{deviceIdOrName}'.");
    }

    /// <inheritdoc/>
    public IReadOnlyList<AudioDeviceInfo> ListRenderDevices()
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = new List<AudioDeviceInfo>();

        foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            devices.Add(new AudioDeviceInfo(device.ID, device.FriendlyName));
            device.Dispose();
        }

        return devices;
    }
}
