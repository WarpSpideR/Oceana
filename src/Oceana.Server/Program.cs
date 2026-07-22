using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.OpenApi;
using Oceana.Server.Features.Agents;
using Oceana.Server.Features.Zones;
using Oceana.Server.Infrastructure.Persistence;
using Oceana.Server.Infrastructure.Realtime;
using Oceana.Server.Infrastructure.Streaming;
using Serilog;

namespace Oceana.Server;

/// <summary>
/// Hosts the Oceana server Web API.
/// </summary>
public static class Program
{
    /// <summary>
    /// Configures the services and middleware, then runs the web application.
    /// </summary>
    /// <param name="args">Command line arguments passed to the host.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSerilog(configuration => configuration.WriteTo.Console());

        const string corsPolicyName = "frontend";

        // There is no authentication yet, so the front end is allowed from any
        // origin (whatever dev port the SPA runs on). AllowCredentials — needed
        // for SignalR — cannot be combined with AllowAnyOrigin, so the request
        // origin is reflected via SetIsOriginAllowed instead. Tighten this to an
        // explicit allow-list when authentication is added (see roadmap.md).
        builder.Services.AddCors(options =>
            options.AddPolicy(
                corsPolicyName,
                policy => policy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

        builder.Services.AddFastEndpoints();
        builder.Services.OpenApiDocument(options =>
        {
            options.DocumentName = "v1";
            options.Title = "Oceana Server API";
            options.Version = "v1";
        });
        builder.Services
            .AddSignalR()
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Persist configuration (zones + agent config) as JSON under a data directory, resolved
        // from Storage:Path (absolute, or relative to the content root) and defaulting to ./data.
        var configuredPath = builder.Configuration["Storage:Path"];
        var dataDirectory = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(builder.Environment.ContentRootPath, "data")
            : Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(builder.Environment.ContentRootPath, configuredPath);

        builder.Services.AddSingleton<IStateStore<ZonesState>>(sp =>
            new JsonStateStore<ZonesState>(
                Path.Combine(dataDirectory, "zones.json"),
                sp.GetRequiredService<ILogger<JsonStateStore<ZonesState>>>()));
        builder.Services.AddSingleton<IStateStore<AgentsState>>(sp =>
            new JsonStateStore<AgentsState>(
                Path.Combine(dataDirectory, "agents.json"),
                sp.GetRequiredService<ILogger<JsonStateStore<AgentsState>>>()));

        builder.Services.AddSingleton<IAgentRegistry, AgentRegistry>();
        builder.Services.AddSingleton<IAgentConnectionFactory, TcpAgentConnectionFactory>();
        builder.Services.AddSingleton<IStatusNotifier, SignalRStatusNotifier>();
        builder.Services.AddSingleton<IAgentRoutingCommander, SignalRRoutingCommander>();
        builder.Services.AddSingleton<IAudioStreamManager, AudioStreamManager>();
        builder.Services.AddSingleton<IZoneBroadcaster, ZoneBroadcaster>();
        builder.Services.AddSingleton<IZonePlaybackService, ZonePlaybackService>();
        builder.Services.AddSingleton<IZoneRegistry, ZoneRegistry>();
        builder.Services.AddSingleton<IZoneStatusNotifier, SignalRZoneStatusNotifier>();

        var app = builder.Build();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseCors(corsPolicyName);
        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";
            config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
        });
        app.MapHub<AgentStatusHub>("/hubs/agents");
        app.MapHub<AgentControlHub>("/hubs/agents-control");
        app.MapHub<ZoneStatusHub>("/hubs/zones");

        app.Run();
    }
}
