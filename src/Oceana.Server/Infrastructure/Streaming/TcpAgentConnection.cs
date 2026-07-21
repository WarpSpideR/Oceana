using System.Net.Sockets;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// An <see cref="IAgentConnection"/> backed by a <see cref="TcpClient"/>.
/// </summary>
public sealed class TcpAgentConnection : IAgentConnection
{
    private readonly TcpClient client;
    private readonly NetworkStream stream;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpAgentConnection"/> class.
    /// </summary>
    /// <param name="client">The connected TCP client.</param>
    public TcpAgentConnection(TcpClient client)
    {
        this.client = client;
        stream = client.GetStream();
    }

    /// <inheritdoc/>
    public Stream Stream => stream;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await stream.DisposeAsync();
        client.Dispose();
    }
}
