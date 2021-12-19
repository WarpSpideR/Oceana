using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace Oceana.Service
{
    /// <summary>
    /// Worker class to initialise Oceana service.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger Logger;

        private Stream? ConnectionStream;

        /// <summary>
        /// Initialises a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public Worker(ILogger<Worker> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Begins Oceana server.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token to initiate termination.</param>
        /// <returns>Task details.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await TestCommsAsync(stoppingToken)
                .ConfigureAwait(false);

            if (stoppingToken == new CancellationToken(true))
            {
                await TestCommsAsync(stoppingToken)
                    .ConfigureAwait(false);
            }
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Reasons..")]
        private async Task TestCommsAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Attempting to connect");

            var communicationSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await communicationSocket.ConnectAsync(IPAddress.Parse("192.168.32.57"), 6012)
                .ConfigureAwait(false);

            Logger.LogInformation("Connected");

            try
            {
                ConnectionStream = new NetworkStream(communicationSocket);

                using var input = new NAudioSourceEvent(2);

                input.SamplesAvailable += Input_SamplesAvailable;
                Logger.LogInformation("Starting");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Bad things happened");
                throw;
            }

            communicationSocket.Close();
        }

        private void Input_SamplesAvailable(object? sender, SamplesAvailableEventArgs e)
        {
            if (ConnectionStream == null)
            {
                return;
            }

            var bytes = new byte[e.Samples.Length * sizeof(float)];
            Buffer.BlockCopy(e.Samples, 0, bytes, 0, bytes.Length);

            Logger.LogInformation($"Writing {e.Samples.Length} samples");
            var message = new AudioDataMessageWriter(bytes);

            message.SendAsync(ConnectionStream).Wait();
        }
    }
}
