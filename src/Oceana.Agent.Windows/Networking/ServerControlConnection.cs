using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Oceana.Agent.Windows.Playback;
using Oceana.Contracts;
using Serilog;

namespace Oceana.Agent.Windows.Networking;

/// <summary>
/// Maintains the agent's persistent SignalR control connection to the server: self-registers,
/// reports devices, and applies routing the server pushes. Reconnects and re-registers automatically.
/// </summary>
public sealed class ServerControlConnection : IAsyncDisposable
{
    private readonly Guid agentId;
    private readonly int audioPort;
    private readonly IAudioDeviceResolver deviceResolver;
    private readonly RoutingStore routingStore;
    private readonly ILogger logger;
    private readonly HubConnection connection;

    private CancellationToken cancellationToken;

    /// <summary>
    /// Initialises a new instance of the <see cref="ServerControlConnection"/> class.
    /// </summary>
    /// <param name="serverUrl">The base URL of the server; the control hub path is appended.</param>
    /// <param name="agentId">The stable agent identifier reported on registration.</param>
    /// <param name="audioPort">The TCP port the agent listens on for audio.</param>
    /// <param name="deviceResolver">Used to enumerate the agent's render devices.</param>
    /// <param name="routingStore">The store updated when the server pushes routing.</param>
    /// <param name="logger">The logger used to report control activity.</param>
    public ServerControlConnection(
        string serverUrl,
        Guid agentId,
        int audioPort,
        IAudioDeviceResolver deviceResolver,
        RoutingStore routingStore,
        ILogger logger)
    {
        this.agentId = agentId;
        this.audioPort = audioPort;
        this.deviceResolver = deviceResolver;
        this.routingStore = routingStore;
        this.logger = logger;

        var hubUrl = $"{serverUrl.TrimEnd('/')}/hubs/agents-control";
        connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect(new JitterRetryPolicy())
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();

        connection.On<AgentRouting>("SetRouting", ApplyRouting);
        connection.Reconnecting += OnReconnectingAsync;
        connection.Reconnected += OnReconnectedAsync;
        connection.Closed += OnClosedAsync;
    }

    /// <summary>
    /// Starts connecting to the server in the background (with retry) and registers once connected.
    /// Does not block; the agent keeps running with default routing until the server responds.
    /// </summary>
    /// <param name="ct">A token used to stop reconnection attempts on shutdown.</param>
    public void Start(CancellationToken ct)
    {
        cancellationToken = ct;
        _ = Task.Run(ConnectAndRegisterAsync, ct);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
    }

    private void ApplyRouting(AgentRouting routing)
    {
        routingStore.Set(routing.Outputs);
        logger.Information("Routing updated by server: {OutputCount} output(s).", routing.Outputs.Count);
    }

    private async Task ConnectAndRegisterAsync()
    {
        if (await ConnectWithRetryAsync())
        {
            await RegisterAsync();
        }
    }

    private async Task<bool> ConnectWithRetryAsync()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await connection.StartAsync(cancellationToken);
                logger.Information("Connected to the server control hub.");
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                logger.Warning("Control connection failed ({Error}); retrying shortly.", ex.Message);
                try
                {
                    await Task.Delay(2000 + Random.Shared.Next(0, 3000), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
        }

        return false;
    }

    private async Task RegisterAsync()
    {
        try
        {
            var devices = deviceResolver.ListRenderDevices()
                .Select(device => new AudioDevice(device.Id, device.Name))
                .ToArray();
            var registration = new AgentRegistration(agentId, Environment.MachineName, audioPort, devices);
            var routing = await connection.InvokeAsync<AgentRouting>("Register", registration, cancellationToken);
            routingStore.Set(routing.Outputs);
            logger.Information("Registered with the server as {AgentId}; {OutputCount} output(s) configured.", agentId, routing.Outputs.Count);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to register with the server.");
        }
    }

    private Task OnReconnectingAsync(Exception? error)
    {
        logger.Warning("Control connection reconnecting: {Error}", error?.Message);
        return Task.CompletedTask;
    }

    private async Task OnReconnectedAsync(string? connectionId)
    {
        // A reconnect is a fresh server-side connection with no group membership; re-register to rejoin.
        await RegisterAsync();
    }

    private async Task OnClosedAsync(Exception? error)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        logger.Warning("Control connection closed ({Error}); attempting to reconnect.", error?.Message);
        await ConnectAndRegisterAsync();
    }

    private sealed class JitterRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            var seconds = Math.Min(30, Math.Pow(2, retryContext.PreviousRetryCount)) + (Random.Shared.NextDouble() * 3);
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
