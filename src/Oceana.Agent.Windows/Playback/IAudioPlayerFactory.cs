namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// Creates <see cref="IAudioPlayer"/> instances, one for each playback session.
/// </summary>
public interface IAudioPlayerFactory
{
    /// <summary>
    /// Creates a new, uninitialised audio player.
    /// </summary>
    /// <returns>A new <see cref="IAudioPlayer"/> instance.</returns>
    IAudioPlayer Create();
}
