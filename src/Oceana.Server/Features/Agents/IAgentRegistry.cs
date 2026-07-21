using Oceana.Contracts;

namespace Oceana.Server.Features.Agents;

/// <summary>
/// Stores the set of known agents and their current state.
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// Records an agent's self-registration, creating or updating its entry and marking it connected.
    /// Existing routing and streaming status are preserved across re-registration.
    /// </summary>
    /// <param name="registration">The details the agent reported.</param>
    /// <param name="host">The host name or IP address derived from the control connection.</param>
    /// <param name="connectionId">The control connection identifier.</param>
    /// <returns>The stored agent.</returns>
    AgentInfo RegisterOrUpdate(AgentRegistration registration, string host, string connectionId);

    /// <summary>
    /// Marks the agent for a dropped control connection offline, unless it has already reconnected.
    /// </summary>
    /// <param name="connectionId">The control connection identifier that dropped.</param>
    /// <returns>The updated agent when it was marked offline; otherwise null.</returns>
    AgentInfo? MarkOffline(string connectionId);

    /// <summary>
    /// Returns every registered agent.
    /// </summary>
    /// <returns>A snapshot of all registered agents.</returns>
    IReadOnlyCollection<AgentInfo> GetAll();

    /// <summary>
    /// Returns the agent with the given identifier.
    /// </summary>
    /// <param name="id">The identifier of the agent.</param>
    /// <returns>The agent, or null when no agent has the given identifier.</returns>
    AgentInfo? Get(Guid id);

    /// <summary>
    /// Removes the agent with the given identifier.
    /// </summary>
    /// <param name="id">The identifier of the agent to remove.</param>
    /// <returns>True when an agent was removed; otherwise false.</returns>
    bool Remove(Guid id);

    /// <summary>
    /// Updates the streaming status of an agent.
    /// </summary>
    /// <param name="id">The identifier of the agent.</param>
    /// <param name="status">The new status.</param>
    /// <param name="lastError">The message describing the most recent fault, or null.</param>
    /// <returns>The updated agent, or null when no agent has the given identifier.</returns>
    AgentInfo? UpdateStatus(Guid id, AgentStatus status, string? lastError = null);

    /// <summary>
    /// Sets the desired routing for an agent.
    /// </summary>
    /// <param name="id">The identifier of the agent.</param>
    /// <param name="routing">The routing to store.</param>
    /// <returns>The updated agent, or null when no agent has the given identifier.</returns>
    AgentInfo? SetDesiredRouting(Guid id, AgentRouting routing);
}
