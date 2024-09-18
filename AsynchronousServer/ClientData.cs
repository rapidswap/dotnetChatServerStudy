using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace AsynchronousServer
{
    class ClientData{
        public TcpClient client {get; set;}
        public byte[] readByteData{get;set;}
        public int clientNumber;

        public ClientData(TcpClient client)
        {
            this.client = client;
            this.readByteData = new byte[1024];

            string clientEndPoint = client.Client.LocalEndPoint.ToString();
            char[] point={'.',':'};
            string[] splitedData = clientEndPoint.Split(point);
            this.clientNumber=int.Parse(splitedData[3]);
            Console.WriteLine("{0}번 사용자 접속성공",clientNumber);
        }
    }
}