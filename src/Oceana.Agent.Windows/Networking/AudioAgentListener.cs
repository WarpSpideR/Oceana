using System.Net;
using System.Net.Sockets;
using Oceana.Agent.Windows.Playback;
using Serilog;

namespace Oceana.Agent.Windows.Networking;

/// <summary>
/// Listens for incoming server connections on all network interfaces and plays each connection's
/// audio stream to the configured output device(s), handling one connection at a time.
/// </summary>
public sealed class AudioAgentListener
{
    /// <summary>
    /// The size, in bytes, of the OS-level receive buffer requested for each connection, giving the kernel room to smooth bursty delivery.
    /// </summary>
    private const int ReceiveBufferBytes = 1 << 18;

    private readonly int port;
    private readonly IAudioPlayerFactory playerFactory;
    private readonly RoutingStore routingStore;
    private readonly ILogger logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="AudioAgentListener"/> class.
    /// </summary>
    /// <param name="port">The TCP port to listen on.</param>
    /// <param name="playerFactory">The factory used to create output players for each session.</param>
    /// <param name="routingStore">The store the current routing is read from at the start of each connection.</param>
    /// <param name="logger">The logger used to report listener activity.</param>
    public AudioAgentListener(int port, IAudioPlayerFactory playerFactory, RoutingStore routingStore, ILogger logger)
    {
        this.port = port;
        this.playerFactory = playerFactory;
        this.routingStore = routingStore;
        this.logger = logger;
    }

    /// <summary>
    /// Starts listening and processes incoming connections until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">A token used to stop the listener.</param>
    /// <returns>A task that completes once the listener has stopped.</returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        logger.Information("Agent listening for server connections on {Endpoint}.", listener.LocalEndpoint);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var client = await listener.AcceptTcpClientAsync(cancellationToken);
                await HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected while shutting down; fall through to stop the listener.
        }
        finally
        {
            listener.Stop();
            logger.Information("Agent listener stopped.");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        client.NoDelay = true;
        client.ReceiveBufferSize = ReceiveBufferBytes;

        var remote = client.Client.RemoteEndPoint;
        logger.Information("Server connected from {Remote}.", remote);

        try
        {
            var session = new AudioPlaybackSession(playerFactory, routingStore.Current, logger);
            await using var stream = client.GetStream();
            await session.RunAsync(stream, cancellationToken);
            logger.Information("Server {Remote} disconnected.", remote);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "The playback session for {Remote} ended with an error.", remote);
        }
    }
}
