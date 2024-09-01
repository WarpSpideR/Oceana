using System.Net;
using System.Net.Sockets;

namespace Oceana.Agent.Windows;

/// <summary>
/// Implementation of the <see cref="IAudioSender"/> interface.
/// </summary>
internal class TcpAudioSender : IAudioSender
{
    private readonly TcpClient tcpSender;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpAudioSender"/> class.
    /// </summary>
    /// <param name="endPoint">Remote endpoint to connect to.</param>
    public TcpAudioSender(IPEndPoint endPoint)
    {
        tcpSender = new TcpClient();
        tcpSender.Connect(endPoint);
    }

    /// <inheritdoc/>
    public async Task SendAsync(ArraySegment<byte> payload)
    {
        await tcpSender.Client.SendAsync(payload);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        tcpSender?.Close();
    }
}
