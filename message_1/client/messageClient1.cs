
using System;
using System.Net.Sockets;
using System.Text;
class messageClient1
{
    TcpClient client = null;
    public void run()
    {
            while(true){
            Console.WriteLine("=====클라이언트=====");
            Console.WriteLine("1. connect Server");
            Console.WriteLine("2. Send Message");
            Console.WriteLine("==================");

            string key = Console.ReadLine();
            int order = 0;

            if(int.TryParse(key, out order))
            {
                switch(order)
                {
                    case 1:
                    {
                        if(client != null)
                        {
                            Console.WriteLine("이미 연결되어있습니다.");
                            Console.ReadKey();
                        }
                        else{
                            Connect();
                        }
                        break;
                    }
                    case 2:
                    {
                        if (client == null)
                        {
                            Console.WriteLine("먼저 서버와 연결하세요");
                            Console.ReadKey();
                        }
                        else{
                            SendMessage();
                        }
                        break;
                    }
                }
            }

            else
            {
                Console.WriteLine("잘못 입력하셨습니다.");
                Console.ReadKey();
                
            
            }
            Console.Clear();
        }
    }

    private void SendMessage()
    {
        Console.WriteLine("보낼 메세지를 입력하세요");
        string message = Console.ReadLine();
        byte[] byteData = new byte[1024];
        byteData = Encoding.Default.GetBytes(message);

        client.GetStream().Write(byteData,0,byteData.Length);
        Console.WriteLine("전송성공");
        Console.ReadKey();
    }

    private void Connect()
    {
        client = new TcpClient();
        client.Connect("127.0.0.1",9999);
        Console.WriteLine("서버연결 성공 이제 Message를 입력하세요");
        Console.ReadKey();
    }
    static void Main(string[] args)
    {
        messageClient1 client = new messageClient1();
        client.run();
    }
}