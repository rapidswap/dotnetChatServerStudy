using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace message_3
{
    public class ClientData
    {
        public TcpClient tcpClient { get; private set; }
        public string clientName { get; set; }
        public int clientNumber { get; set; }
        public byte[] readBuffer { get; set; }

        public ClientData(TcpClient tcpClient, int clientNumber)
        {
            this.tcpClient = tcpClient;
            this.clientNumber = clientNumber;
            this.clientName = "Unknown";
            this.readBuffer = new byte[1024];

            NetworkStream stream = tcpClient.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            this.clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        }
    }
}
