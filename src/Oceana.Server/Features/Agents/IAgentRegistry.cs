namespace Oceana.Server.Features.Agents;

/// <summary>
/// Stores the set of known agents and their current state.
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// Registers a new agent and assigns it an identifier.
    /// </summary>
    /// <param name="name">The human-readable name of the agent.</param>
    /// <param name="host">The host name or IP address on which the agent is listening.</param>
    /// <param name="port">The TCP port on which the agent is listening.</param>
    /// <returns>The newly registered agent.</returns>
    AgentInfo Register(string name, string host, int port);

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
    /// Updates the status of an agent.
    /// </summary>
    /// <param name="id">The identifier of the agent.</param>
    /// <param name="status">The new status.</param>
    /// <param name="lastError">The message describing the most recent fault, or null.</param>
    /// <returns>The updated agent, or null when no agent has the given identifier.</returns>
    AgentInfo? UpdateStatus(Guid id, AgentStatus status, string? lastError = null);
}
