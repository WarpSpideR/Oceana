namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// An open connection to an agent that accepts the handshake header followed by a stream of audio bytes.
/// </summary>
public interface IAgentConnection : IAsyncDisposable
{
    /// <summary>
    /// Gets the writable stream carrying data to the agent.
    /// </summary>
    Stream Stream { get; }
}
