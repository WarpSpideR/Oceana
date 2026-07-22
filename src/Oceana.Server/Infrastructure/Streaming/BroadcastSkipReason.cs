namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Why an agent in a zone was not included in a broadcast.
/// </summary>
public enum BroadcastSkipReason
{
    /// <summary>
    /// The agent's control connection is not established.
    /// </summary>
    Offline = 0,

    /// <summary>
    /// The agent is already handling another stream.
    /// </summary>
    Busy = 1,

    /// <summary>
    /// None of the zone's devices for the agent are currently reported by it.
    /// </summary>
    NoActiveDevices = 2,
}
