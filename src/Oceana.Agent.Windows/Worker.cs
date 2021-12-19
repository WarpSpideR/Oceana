using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Oceana.Agent.Windows
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting comms");
            
            var comms = new CommunicationChannel();
            await comms.ConnectAsync(IPAddress.Any);

            _logger.LogInformation("Comms connected");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);

                //if (DateTime.Now > changeTime)
                //{
                //    changeTime = DateTime.Now.AddSeconds(15);

                //    sourceId = ++sourceId % sources.Count();

                //    var oldInput = input;
                //    input = new NAudioSource(sourceId);

                //    infiniteSource.SetAudioSource(input);

                //    oldInput.Dispose();
                //}
            }
        }
    }
}
