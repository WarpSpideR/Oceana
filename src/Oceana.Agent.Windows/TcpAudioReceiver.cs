using System.Net.Sockets;
using NAudio.Wave;

namespace Oceana.Agent.Windows;

/// <summary>
/// Implementation of the <see cref="IAudioReceiver"/> using tcp as a transport.
/// </summary>
internal class TcpAudioReceiver : IWaveProvider
{
    private readonly TcpClient client;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpAudioReceiver"/> class.
    /// </summary>
    /// <param name="client">Incoming client tcp connection.</param>
    public TcpAudioReceiver(TcpClient client)
    {
        this.client = client;
    }

    /// <inheritdoc/>
    public WaveFormat WaveFormat => new WaveFormat(44100, 2);

    /// <inheritdoc/>
    public int Read(byte[] buffer, int offset, int count)
    {
        return client.Client.Receive(buffer, offset, count, SocketFlags.None);
    }
}
