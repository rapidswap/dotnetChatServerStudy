using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Msg3Client
{
    class ConsoleClient
    {
        TcpClient client= null;
        Thread receiveMessageThread = null;
        ConcurrentBag<string> sendMessageListToView = null;
        ConcurrentBag<string> receiveMessageListToView = null;
        private string name = null;
        public void Run()
        {
            sendMessageListToView = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();
            
            receiveMessageThread = new Thread(receiveMessage);
            while(true)
            {
                Console.WriteLine("=======클라이언트 메뉴=======");
                Console.WriteLine("1. 채팅 참여");
                Console.WriteLine("2. 메세지 보내기");
                Console.WriteLine("3. 보낸 메세지 확인");
                Console.WriteLine("4. 받은 메세지 확인");
                Console.WriteLine("0. 종료");
                Console.WriteLine("=========================");
                string key = Console.ReadLine();
                int order = 0;
                if(int.TryParse(key, out order))
                {
                    switch(order)
                    {
                        case StaticDefine.CONNECT:
                        {
                            if(client != null)
                            {
                                Console.WriteLine("이미 채팅에 참여하셨습니다.");
                                Console.ReadKey();
                            }
                            else{
                                Connect();
                            }
                            break;
                        }
                        case StaticDefine.SEND_MESSAGE:
                        {
                            if(client==null)
                            {
                                Console.WriteLine("채팅에 참여하세요.");
                                Console.ReadKey();
                            }
                            else{
                                SendMessage();
                            }
                            break;
                        }
                        case StaticDefine.SEND_MSG_VIEW:
                        {
                            SendMessageView();
                            break;
                        }
                        case StaticDefine.RECEIVE_MSG_VIEW:
                        {
                            ReceiveMessageView();
                            break;
                        }
                        case StaticDefine.EXIT:
                        {
                            if(client != null)
                            {
                                client.Close();
                            }
                            receiveMessageThread.Abort();
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    Console.ReadKey();
                }
                Console.Clear();
                Thread.Sleep(50);
            }
        }
        private void ReceiveMessageView()
        {
            if(receiveMessageListToView.Count== 0)
            {
                Console.WriteLine("받은 메세지가 없습니다.");
                Console.ReadKey();
                return;
            }
            foreach(string item in receiveMessageListToView)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }
        private void SendMessageView()
        {
            if(sendMessageListToView.Count == 0)
            {
                Console.WriteLine("보낸 메세지가 없습니다.");
                Console.ReadKey();
                return;
            }
            foreach(string item in sendMessageListToView)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }
        private void receiveMessage()
        {
            while(true)
            {
                try
                {
                    byte[] receiveByte = new byte[1024];
                    int bytesRead = client.GetStream().Read(receiveByte, 0, receiveByte.Length);
                    if (bytesRead == 0) continue;

                    string receiveMessage = Encoding.Default.GetString(receiveByte, 0, bytesRead);

                    string[] receiveMessageArray = receiveMessage.Split(new[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> receiveMessageList = new List<string>();

                    foreach(string item in receiveMessageArray)
                    {
                        if(string.IsNullOrEmpty(item) || !item.Contains("<")) continue;
                        if(item.Contains("관리자<TEST")) continue;
                        receiveMessageList.Add(item + ">");
                    }

                    if (receiveMessageList.Count > 0)
                    {
                        ParsingReceiveMessage(receiveMessageList);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"메시지 수신 오류: {e.Message}");
                    break;
                }
                Thread.Sleep(100);
            }
        }
        private void ParsingReceiveMessage(List<string> messageList)
        {
            foreach(string item in messageList)
            {
                string[] splitMsg = item.Split(new[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitMsg.Length < 2) continue;

                string sender = splitMsg[0];
                string message = splitMsg[1];

                // %^& 접두사 제거
                if (sender.StartsWith("%^&"))
                {
                    sender = sender.Substring(3);
                }

                if(sender.Contains("관리자"))
                {
                    string userList = string.Join(" ", message.Split('$').Where(u => !string.IsNullOrEmpty(u)));
                    // 사용자 목록에서도 %^& 제거
                    userList = string.Join(" ", userList.Split(' ').Select(u => u.StartsWith("%^&") ? u.Substring(3) : u));
                    Console.WriteLine($"[현재 접속인원] {userList}");
                }
                else
                {
                    Console.WriteLine($"[메시지가 도착하였습니다] {sender} : {message}");
                    receiveMessageListToView.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Sender : {sender}, Message : {message}");
                }
            }
        }
        private void SendMessage()
        {
            string getUserList = string.Format("{0}<GiveMeUserList>", name);
            byte[] getUserListByte = Encoding.Default.GetBytes(getUserList);
            client.GetStream().Write(getUserListByte, 0, getUserListByte.Length);

            Console.WriteLine("수신자를 입력해주세요.");
            string receiver = Console.ReadLine();
            Console.WriteLine("메세지를 입력해주세요.");
            string message = Console.ReadLine();
            if(string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
            {
                Console.WriteLine("잘못된 입력입니다.");
                Console.ReadKey();
                return;
            }
            
            // 수신자에 %^& 접두사 추가
            receiver = "%^&" + receiver;
            
            string parsedMessage = string.Format("{0}<{1}>", receiver, message);
            byte[] byteData = Encoding.Default.GetBytes(parsedMessage);
            client.GetStream().Write(byteData, 0, byteData.Length);
            sendMessageListToView.Add(string.Format("[{0}] Receiver : {1}, Message : {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), receiver.Substring(3), message));
            Console.WriteLine("메세지가 전송되었습니다.");
            Console.ReadKey();
        }
        private void Connect()
        {
            Console.WriteLine("닉네임을 입력해주세요.");
            name = Console.ReadLine();
            
            string parsedName = "%^&" + name;
            if(parsedName == "%^&")
            {
                Console.WriteLine("잘못된 입력입니다.");
                Console.ReadKey();
                return;
            }
            client= new TcpClient();
            client.Connect("127.0.0.1", 9999);
            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedName);
            client.GetStream().Write(byteData, 0, byteData.Length);
            receiveMessageThread.Start();
            Console.WriteLine("서버 접속 성공");
            Console.ReadKey();
        }
        
    }
}
