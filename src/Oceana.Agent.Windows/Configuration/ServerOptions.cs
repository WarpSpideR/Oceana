namespace Oceana.Agent.Windows.Configuration;

/// <summary>
/// Agent configuration for reaching the Oceana server, bound from <c>appsettings.json</c>.
/// </summary>
public sealed class ServerOptions
{
    /// <summary>
    /// Gets or sets the base URL of the Oceana server. The agent appends the control hub path
    /// (<c>/hubs/agents-control</c>). A server is required; the agent will not run without this.
    /// </summary>
    public string? ServerUrl { get; set; }
}
