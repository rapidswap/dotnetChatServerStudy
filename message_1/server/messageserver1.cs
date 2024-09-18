using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
class messageserver1
{
    static void Main(string[] args)
    {
        Console.WriteLine("서버 콘솔창 \n");
        TcpListener server = new TcpListener(IPAddress.Any, 9999);

        server.Start();

        TcpClient client = server.AcceptTcpClient();

        Console.WriteLine("클라이언트가 접속하였습니다.");

        while(true)
        {
            byte[] byteData = new byte[1024];

            client.GetStream().Read(byteData,0,byteData.Length);

            string strData = Encoding.Default.GetString(byteData);

            int endPoint = strData.IndexOf('\0');
            string parsedMessage = strData.Substring(0,endPoint+1);
            Console.WriteLine(parsedMessage);
        }
    }

}
