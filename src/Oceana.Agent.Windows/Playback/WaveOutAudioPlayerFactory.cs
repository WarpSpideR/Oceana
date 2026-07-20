namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// An <see cref="IAudioPlayerFactory"/> that produces <see cref="WaveOutAudioPlayer"/> instances.
/// </summary>
public sealed class WaveOutAudioPlayerFactory : IAudioPlayerFactory
{
    /// <inheritdoc/>
    public IAudioPlayer Create()
    {
        return new WaveOutAudioPlayer();
    }
}
