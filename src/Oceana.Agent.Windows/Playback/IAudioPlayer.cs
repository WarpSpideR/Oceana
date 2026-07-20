using NAudio.Wave;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// Abstracts the audio output device so that playback sessions can be exercised in tests without a sound card.
/// </summary>
public interface IAudioPlayer : IDisposable
{
    /// <summary>
    /// Initialises the player with the wave provider it should render.
    /// </summary>
    /// <param name="waveProvider">The source of audio samples to play.</param>
    void Init(IWaveProvider waveProvider);

    /// <summary>
    /// Begins, or resumes, playback of the initialised source.
    /// </summary>
    void Play();

    /// <summary>
    /// Stops playback.
    /// </summary>
    void Stop();
}
