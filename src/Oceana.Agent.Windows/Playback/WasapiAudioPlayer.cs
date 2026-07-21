using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// An <see cref="IAudioPlayer"/> backed by NAudio's <see cref="WasapiOut"/>, rendering to a specific
/// WASAPI render device in shared mode.
/// </summary>
public sealed class WasapiAudioPlayer : IAudioPlayer
{
    /// <summary>
    /// The requested shared-mode output latency, in milliseconds.
    /// </summary>
    private const int LatencyMilliseconds = 150;

    private readonly MMDevice device;
    private readonly WasapiPlayer output;

    /// <summary>
    /// Initialises a new instance of the <see cref="WasapiAudioPlayer"/> class.
    /// </summary>
    /// <param name="device">The render device to play to; the player takes ownership and disposes it.</param>
    public WasapiAudioPlayer(MMDevice device)
    {
        this.device = device;
        output = new WasapiPlayerBuilder()
            .WithDevice(device)
            .WithSharedMode()
            .WithEventSync()
            .WithLatency(LatencyMilliseconds)
            .Build();
    }

    /// <inheritdoc/>
    public void Init(IWaveProvider waveProvider)
    {
        output.Init(waveProvider);
    }

    /// <inheritdoc/>
    public void Play()
    {
        output.Play();
    }

    /// <inheritdoc/>
    public void Stop()
    {
        output.Stop();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        output.Dispose();
        device.Dispose();
    }
}
