using NAudio.Wave;

namespace Oceana.Agent.Windows;

/// <summary>
/// Represents an endpoint for auido information.
/// </summary>
public class AudioSink
{
    private readonly WaveOutEvent sink;

    /// <summary>
    /// Initialises a new instance of the <see cref="AudioSink"/> class.
    /// </summary>
    /// <param name="deviceNumber">Device to play on.</param>
    public AudioSink(int deviceNumber)
    {
        sink = new WaveOutEvent();
        sink.DeviceNumber = deviceNumber;
    }

    /// <summary>
    /// Initialises the sink with the given audio source.
    /// </summary>
    /// <param name="source">Audio source to play.</param>
    public void Initialise(ISampleProvider source)
    {
        sink.Init(source);
        sink.Play();
    }
}
