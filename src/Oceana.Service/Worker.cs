using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NAudio.Utils;
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

        private static async Task Test(CancellationToken stoppingToken)
        {
            var input = new WaveInEvent();
            input.DeviceNumber = 2;

            var inputProvider = new WaveInProvider(input);

            var output = new WaveOutEvent();
            output.DeviceNumber = 0;
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

        private static async Task Real(CancellationToken stoppingToken)
        {
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

        /// <summary>
        /// Begins Oceana server.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token to initiate termination.</param>
        /// <returns>Task details.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Real(stoppingToken)
                .ConfigureAwait(false);

            if (stoppingToken == new CancellationToken(true))
            {
                await Test(stoppingToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
