using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Oceana.Core;

namespace Oceana.Service
{
    /// <summary>
    /// Worker class to initialise Oceana service.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger Logger;

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
            await RealAsync(stoppingToken)
                .ConfigureAwait(false);

            if (stoppingToken == new CancellationToken(true))
            {
                await TestAsync(stoppingToken)
                    .ConfigureAwait(false);
            }
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Debug message.")]
        private async Task TestAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Testing NAudio components.");

            var input = new WaveInEvent
            {
                DeviceNumber = 2,
            };

            var inputProvider = new WaveInProvider(input);

            var output = new WaveOutEvent
            {
                DeviceNumber = 0,
            };
            output.Init(inputProvider);

            input.StartRecording();
            output.Play();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken)
                    .ConfigureAwait(false);
            }

            input.Dispose();
            output.Dispose();
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Debug message.")]
        private async Task RealAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Testing Oceana components.");

            var input = new NAudioInput(2);

            var output = new NAudioOutput(input);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken)
                    .ConfigureAwait(false);
            }

            input.Dispose();
            output.Dispose();
        }
    }
}
