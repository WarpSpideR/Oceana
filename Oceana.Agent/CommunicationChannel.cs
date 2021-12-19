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
        private readonly int DiscoveryPort;

        private readonly int CommunicationPort;

        private UdpClient DiscoverySocket;

        private Socket CommunicationSocket;

        private NetworkStream CommunicationStream;

        public CommunicationChannel()
        {
            DiscoveryPort = 7591;
            CommunicationPort = 6012;
        }

        public bool IsConnected { get; private set; } = false;

        public async Task ConnectAsync(IPAddress host)
        {
            try
            {
                var CommunicationSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                await CommunicationSocket.ConnectAsync(host, CommunicationPort)
                    .ConfigureAwait(false);

                CommunicationStream = new NetworkStream(CommunicationSocket);

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
            byte[] data = Encoding.ASCII.GetBytes("Heartbeat");

            while (true)
            {
                await CommunicationStream.WriteAsync(data, 0, data.Length);

                await Task.Delay(5000);
            }
        }

        public void Dispose()
        {
            DiscoverySocket.Dispose();
            CommunicationSocket.Dispose();
        }
    }
}
