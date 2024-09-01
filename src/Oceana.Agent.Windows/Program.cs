using System.Net;
using NAudio.Wave;

namespace Oceana.Agent.Windows;

/// <summary>
/// Main entry point to the agent.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point to the agent.
    /// </summary>
    /// <param name="args">Additional command line arguments passed to the agent.</param>
    /// <returns>Task.</returns>
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Select mode to run in:");
        Console.WriteLine("1: Sender");
        Console.WriteLine("2: Receiver");

        var mode = int.Parse(Console.ReadLine() ?? "1");

        switch (mode)
        {
            case 1:
                Console.WriteLine("Sending audio");

                var sender = new TcpAudioSender(new IPEndPoint(IPAddress.Loopback, 8090));

                var reader = new Mp3FileReader("Sound.mp3");
                var buffer = new byte[1024 * 1024 * 5];
                var bytes = 0;
                do
                {
                    bytes = await reader.ReadAsync(buffer);

                    await sender.SendAsync(buffer[..bytes]);
                }
                while (bytes > 0);

                Console.WriteLine("Bytes sent");

                break;
            case 2:
                var listener = new AgentTcpListener();
                await listener.ListenAsync(default);

                break;
        }

        Console.WriteLine("Press any key to close...");

        _ = Console.ReadLine();
    }
}
