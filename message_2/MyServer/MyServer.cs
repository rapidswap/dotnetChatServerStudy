using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace MyServer{
    class MyServer
    {
        public MyServer()
        {
            AsyncServerStart();
        }

        private void AsyncServerStart()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any,9999));
            listener.Start();
            Console.WriteLine("서버 시작, 클라이언트 접속 대기");

            TcpClient acceptClient = listener.AcceptTcpClient();
            Console.WriteLine("클라이언트 접속완료");

            ClientData clientData = new ClientData(acceptClient);

            clientData.client.GetStream().BeginRead(clientData.readByteData,0,clientData.readByteData.Length,new AsyncCallback(DataReceived), clientData);

            while(true)
            {
                Console.WriteLine("서버 구동중");
                Thread.Sleep(1000);
            }
        }

        private void DataReceived(IAsyncResult ar)
        {
            ClientData callbackClient = ar.AsyncState as ClientData;

            int bytesRead = callbackClient.client.GetStream().EndRead(ar);

            string readString = Encoding.Default.GetString(callbackClient.readByteData,0,bytesRead);
            Console.WriteLine(readString);

            callbackClient.client.GetStream().BeginRead(callbackClient.readByteData,0,callbackClient.readByteData.Length,new AsyncCallback(DataReceived),callbackClient);
        }
    }
}