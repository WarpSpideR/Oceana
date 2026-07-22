using Oceana.Server.Features.Agents;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// A reachable agent in a zone and the agent devices audio should play on.
/// </summary>
/// <param name="Agent">The target agent.</param>
/// <param name="DeviceIds">The agent's device ids (currently reported) that are in the zone.</param>
public sealed record ZoneTarget(AgentInfo Agent, IReadOnlyList<string> DeviceIds);
