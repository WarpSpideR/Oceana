namespace Oceana.Server.Features.Agents;

/// <summary>
/// The persisted snapshot of all known agents' configuration.
/// </summary>
/// <param name="Agents">The agent configurations to persist.</param>
public sealed record AgentsState(IReadOnlyList<AgentConfig> Agents);
