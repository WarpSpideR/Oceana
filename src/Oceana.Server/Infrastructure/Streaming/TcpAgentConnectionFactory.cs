using System.Net.Sockets;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Creates <see cref="TcpAgentConnection"/> instances by dialling out to agents over TCP.
/// </summary>
public sealed class TcpAgentConnectionFactory : IAgentConnectionFactory
{
    /// <inheritdoc/>
    public async Task<IAgentConnection> ConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        var client = new TcpClient { NoDelay = true };
        await client.ConnectAsync(host, port, cancellationToken);
        return new TcpAgentConnection(client);
    }
}
