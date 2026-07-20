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
            var port = ParsePort(args, logger);
            using var cancellation = new CancellationTokenSource();

            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                logger.Information("Shutdown requested; stopping the agent.");
                cancellation.Cancel();
            };

            var listener = new AudioAgentListener(port, new WaveOutAudioPlayerFactory(), logger);
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
