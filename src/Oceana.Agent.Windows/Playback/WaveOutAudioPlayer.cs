using NAudio.Wave;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// An <see cref="IAudioPlayer"/> backed by NAudio's <see cref="WaveOut"/>, rendering to the system default output device.
/// </summary>
public sealed class WaveOutAudioPlayer : IAudioPlayer
{
    /// <summary>
    /// The device number that selects the WAVE_MAPPER, i.e. the current system default output device.
    /// </summary>
    private const int DefaultOutputDevice = -1;

    /// <summary>
    /// The size, in milliseconds, of each buffer the device rotates through.
    /// </summary>
    private const int BufferMilliseconds = 100;

    /// <summary>
    /// The number of buffers the device rotates through; more buffers give the render callback additional slack against scheduling and garbage-collection pauses.
    /// </summary>
    private const int OutputBufferCount = 3;

    private readonly WaveOut output = new WaveOut
    {
        DeviceNumber = DefaultOutputDevice,
        BufferMilliseconds = BufferMilliseconds,
        NumberOfBuffers = OutputBufferCount,
    };

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
    }
}
