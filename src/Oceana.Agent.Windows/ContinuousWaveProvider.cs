using NAudio.Wave;

namespace Oceana.Agent.Windows;

/// <summary>
/// Ensures a continuous stream of bytes is provided.
/// </summary>
internal class ContinuousWaveProvider : IWaveProvider
{
    private IWaveProvider? source;

    /// <inheritdoc/>
    public WaveFormat WaveFormat => throw new NotImplementedException();

    /// <summary>
    /// Sets the source.
    /// </summary>
    /// <param name="source">New wave provider source.</param>
    public void SetSource(IWaveProvider source)
    {
        this.source = source;
    }

    /// <inheritdoc/>
    public int Read(byte[] buffer, int offset, int count)
    {
        if (source is null)
        {
            Array.Fill<byte>(buffer, 0, offset, count);
            return count;
        }

        var bytes = source.Read(buffer, offset, count);
        if (bytes == 0)
        {
            Array.Fill<byte>(buffer, 0, offset, count);
            return count;
        }

        return bytes;
    }
}
