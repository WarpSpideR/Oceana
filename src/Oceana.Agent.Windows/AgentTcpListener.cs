using System.Net;
using System.Net.Sockets;
using NAudio.Wave;

namespace Oceana.Agent.Windows;

/// <summary>
/// Used to listen for incomming tcp connnections.
/// </summary>
public class AgentTcpListener
{
    /// <summary>
    /// Listens for incoming tcp connections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Task representing the result of the asynchronous operation.</returns>
    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        using var tcpListener = new TcpListener(IPAddress.Loopback, 8090);
        tcpListener.Start();

        Console.WriteLine("Listenting for connections on {ip}:{port}");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await tcpListener.AcceptTcpClientAsync(cancellationToken);

            _ = Task.Factory.StartNew(() => ProcessClient(client), TaskCreationOptions.LongRunning);
        }
    }

    private static void ProcessClient(TcpClient client)
    {
        Console.WriteLine("Client connected");

        var audioReceiver = new TcpAudioReceiver(client);

        var output = new WaveOutEvent();
        output.DeviceNumber = 0;
        output.Init(audioReceiver);
        output.Play();
    }
}
