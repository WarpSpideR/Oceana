namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Opens outbound connections to agents.
/// </summary>
public interface IAgentConnectionFactory
{
    /// <summary>
    /// Opens a connection to the given agent endpoint.
    /// </summary>
    /// <param name="host">The agent host name or IP address.</param>
    /// <param name="port">The agent TCP port.</param>
    /// <param name="cancellationToken">A token used to cancel the connection attempt.</param>
    /// <returns>The open connection.</returns>
    Task<IAgentConnection> ConnectAsync(string host, int port, CancellationToken cancellationToken);
}
