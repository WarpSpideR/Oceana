using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.OpenApi;
using Oceana.Server.Features.Agents;
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
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "http://localhost:3000" };

        builder.Services.AddCors(options =>
            options.AddPolicy(
                corsPolicyName,
                policy => policy
                    .WithOrigins(allowedOrigins)
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

        builder.Services.AddSingleton<IAgentRegistry, AgentRegistry>();
        builder.Services.AddSingleton<IAgentConnectionFactory, TcpAgentConnectionFactory>();
        builder.Services.AddSingleton<IStatusNotifier, SignalRStatusNotifier>();
        builder.Services.AddSingleton<IAgentRoutingCommander, SignalRRoutingCommander>();
        builder.Services.AddSingleton<IAudioStreamManager, AudioStreamManager>();

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

        app.Run();
    }
}
