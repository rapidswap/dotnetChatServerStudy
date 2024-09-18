using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace MyServer{
    class ClientData
    {
        public TcpClient client { get; set;}
        public byte[] readByteData {get; set;}
        public ClientData(TcpClient client)
        {
            this.client = client;
            this.readByteData = new byte[1024];
        }
    }
}