using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oceana.Agent
{
    public class CommunicationChannel : IDisposable
    {
        private readonly IServiceProvider Services;

        private readonly int DiscoveryPort;

        private readonly int CommunicationPort;

        private readonly byte[] MessageCodeBuffer = new byte[1];

        private UdpClient DiscoverySocket;

        private Socket CommunicationSocket;

        private NetworkStream CommunicationStream;

        public CommunicationChannel(IServiceProvider services)
        {
            Services = services;
            DiscoveryPort = 7591;
            CommunicationPort = 6012;
        }

        public bool IsConnected { get; private set; } = false;

        public async Task ConnectAsync(IPAddress host)
        {
            try
            {
                var server = new TcpListener(IPAddress.Any, CommunicationPort);
                server.Start();

                var client = await server.AcceptTcpClientAsync()
                    .ConfigureAwait(false);

                CommunicationStream = client.GetStream();

                IsConnected = true;

                var commsTask = Task.Run(Communicate);
            }
            catch (Exception e)
            {
                IsConnected = false;
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task Communicate()
        {
            while (true)
            {
                var bytesRead = await CommunicationStream.ReadAsync(MessageCodeBuffer, 0, 1);

                if (bytesRead == 1)
                {
                    var messageReader = MessageReaderFactory.GetReader((MessageCode)MessageCodeBuffer[0]);

                    if (messageReader != null)
                    {
                        await messageReader.ReadAsync(CommunicationStream);
                    }
                }

            }
        }

        public void Dispose()
        {
            DiscoverySocket.Dispose();
            CommunicationSocket.Dispose();
        }
    }
}
