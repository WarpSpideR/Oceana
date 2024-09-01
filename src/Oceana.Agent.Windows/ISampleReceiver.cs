namespace Oceana.Agent.Windows;

/// <summary>
/// Indicates that the instance can receive samples.
/// </summary>
public interface ISampleReceiver
{
    /// <summary>
    /// Writes samples to the receiver.
    /// </summary>
    /// <param name="samples">Samples to be written.</param>
    /// <returns>The number of samples that were written.</returns>
    public int Write(ArraySegment<float> samples);
}
