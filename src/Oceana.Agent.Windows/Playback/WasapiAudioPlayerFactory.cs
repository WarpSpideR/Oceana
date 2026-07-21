namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// An <see cref="IAudioPlayerFactory"/> that produces <see cref="WasapiAudioPlayer"/> instances,
/// resolving the target device via an <see cref="IAudioDeviceResolver"/>.
/// </summary>
public sealed class WasapiAudioPlayerFactory : IAudioPlayerFactory
{
    private readonly IAudioDeviceResolver deviceResolver;

    /// <summary>
    /// Initialises a new instance of the <see cref="WasapiAudioPlayerFactory"/> class.
    /// </summary>
    /// <param name="deviceResolver">The resolver used to look up render devices.</param>
    public WasapiAudioPlayerFactory(IAudioDeviceResolver deviceResolver)
    {
        this.deviceResolver = deviceResolver;
    }

    /// <inheritdoc/>
    public IAudioPlayer Create(string? deviceId)
    {
        var device = deviceResolver.Resolve(deviceId);
        return new WasapiAudioPlayer(device);
    }
}
