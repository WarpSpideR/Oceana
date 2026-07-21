using Microsoft.Extensions.Configuration;
using Oceana.Agent.Windows.Configuration;
using Oceana.Agent.Windows.Networking;
using Oceana.Agent.Windows.Playback;
using Serilog;

namespace Oceana.Agent.Windows;

/// <summary>
/// Main entry point to the agent.
/// </summary>
public static class Program
{
    private const int DefaultPort = 8090;

    /// <summary>
    /// Main entry point to the agent.
    /// </summary>
    /// <param name="args">Additional command line arguments passed to the agent.</param>
    /// <returns>A task representing the asynchronous execution of the agent.</returns>
    public static async Task Main(string[] args)
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            var deviceResolver = new WasapiAudioDeviceResolver();

            if (args.Any(argument => string.Equals(argument, "--list-devices", StringComparison.OrdinalIgnoreCase)))
            {
                ListDevices(deviceResolver, logger);
                return;
            }

            var serverOptions = LoadServerOptions();
            if (string.IsNullOrWhiteSpace(serverOptions.ServerUrl))
            {
                logger.Fatal("No 'ServerUrl' is configured in appsettings.json; the agent requires a server. Exiting.");
                return;
            }

            var port = ParsePort(args, logger);
            var agentId = LoadOrCreateAgentId();
            var routingStore = new RoutingStore();

            using var cancellation = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                logger.Information("Shutdown requested; stopping the agent.");
                cancellation.Cancel();
            };

            await using var control = new ServerControlConnection(serverOptions.ServerUrl, agentId, port, deviceResolver, routingStore, logger);
            control.Start(cancellation.Token);

            var playerFactory = new WasapiAudioPlayerFactory(deviceResolver);
            var listener = new AudioAgentListener(port, playerFactory, routingStore, logger);
            await listener.RunAsync(cancellation.Token);
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "The agent terminated unexpectedly.");
        }
        finally
        {
            logger.Dispose();
        }
    }

    private static ServerOptions LoadServerOptions()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var options = new ServerOptions();
        configuration.Bind(options);
        return options;
    }

    private static Guid LoadOrCreateAgentId()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "agent-id");
        if (File.Exists(path) && Guid.TryParse(File.ReadAllText(path).Trim(), out var existing))
        {
            return existing;
        }

        var id = Guid.NewGuid();
        File.WriteAllText(path, id.ToString());
        return id;
    }

    private static void ListDevices(IAudioDeviceResolver resolver, ILogger logger)
    {
        foreach (var device in resolver.ListRenderDevices())
        {
            logger.Information("Render device: {Name} [{Id}]", device.Name, device.Id);
        }
    }

    private static int ParsePort(string[] args, ILogger logger)
    {
        for (var i = 0; i < (args.Length - 1); i++)
        {
            if (!string.Equals(args[i], "--port", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (int.TryParse(args[i + 1], out var parsed) && parsed is > 0 and <= 65535)
            {
                return parsed;
            }

            logger.Warning("Ignoring invalid --port value '{Value}'; falling back to {DefaultPort}.", args[i + 1], DefaultPort);
        }

        return DefaultPort;
    }
}
