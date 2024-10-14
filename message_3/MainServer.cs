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
    public class MainServer
    {
        private ClientManager _clientManager;
        ConcurrentBag<string> chattingLog=null;
        ConcurrentBag<string> AccessLog=null;
        Thread conntectCheckThread=null;
        private bool isRunning = true;

        public MainServer()
        {
            _clientManager = new ClientManager();
            chattingLog = new ConcurrentBag<string>();
            AccessLog = new ConcurrentBag<string>();
            _clientManager.EventHandler += ClientEvent;
            _clientManager.messageParsingAction += MessageParsing;
            Task serverStart = Task.Run(() =>
            {
                ServerRun();
            } );

            conntectCheckThread = new Thread(ConnectCheckLoop);
            conntectCheckThread.Start();
        }

        private void ConnectCheckLoop()
        {
            while(isRunning)
            {
                foreach(var item in _clientManager.clientDic.ToList())
                {
                    try
                    {
                        string sendStringData = "관리자<TEST>";
                        byte[] sendByteData = Encoding.Default.GetBytes(sendStringData);
                        item.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                    }
                    catch(Exception e)
                    {
                        RemoveClient(item.Value);
                        Console.WriteLine($"클라이언트 연결 오류: {e.Message}");
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void RemoveClient(ClientData targetClient)
        {
            ClientData result;
            if (_clientManager.clientDic.TryRemove(targetClient.clientNumber, out result))
            {
                string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.clientName);
                AccessLog.Add(leaveLog);
            }
        }

        private void MessageParsing(string sender, string message)
        {
            List<string> msgList = new List<string>();

            string[] msgArray = message.Split('>');
            foreach(var item in msgArray)
            {
                if(string.IsNullOrEmpty(item))
                {
                    continue;
                }
                msgList.Add(item);
            }
            SendMsgToClient(msgList,sender);
        }

        private void SendMsgToClient(List<string> msgList, string sender)
        {
            foreach(var item in msgList)
            {
                string[] splitedMsg = item.Split('<');
                if (splitedMsg.Length < 2) continue;

                string receiver = splitedMsg[0];
                string message = splitedMsg[1];
                string parsedMessage = string.Format("{0}<{1}>", sender, message);
                int receiverNumber = GetClientNumber(receiver);
                if (receiverNumber == -1)
                {
                    continue;
                }
                
                if (message.Contains("GiveMeUserList"))
                {
                    SendUserList(receiverNumber);
                }
                else
                {
                    string LogMessage = string.Format(@"[{0}] [{1}] -> [{2}]  ,  {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, receiver, message);
                    ClientEvent(LogMessage, StaticDefine.ADD_CHATTING_LOG);
                    SendMessage(receiverNumber, parsedMessage);
                }
            }
        }

        private void SendUserList(int receiverNumber)
        {
            string userListStringData = "관리자<";
            foreach (var el in _clientManager.clientDic)
            {
                // %^& 접두사를 제거하고 사용자 이름 추가
                string userName = el.Value.clientName.StartsWith("%^&") ? el.Value.clientName.Substring(3) : el.Value.clientName;
                userListStringData += string.Format("${0}", userName);
            }
            userListStringData += ">";
            SendMessage(receiverNumber, userListStringData);
        }

        private void SendMessage(int receiverNumber, string message)
        {
            try
            {
                if (_clientManager.clientDic.TryGetValue(receiverNumber, out ClientData clientData))
                {
                    byte[] sendByteData = Encoding.Default.GetBytes(message);
                    NetworkStream stream = clientData.tcpClient.GetStream();
                    stream.Write(sendByteData, 0, sendByteData.Length);
                    stream.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"메시지 전송 오류: {e.Message}");
                RemoveClient(_clientManager.clientDic[receiverNumber]);
            }
        }

        private int GetClientNumber(string targetClientName)
        {
            foreach(var item in _clientManager.clientDic)
            {
                if(item.Value.clientName == targetClientName)
                {
                    return item.Value.clientNumber;

                }
            }
            return -1;
        }
        private void ClientEvent(string message, int key)
        {
            switch(key)
            {
                case StaticDefine.ADD_ACCESS_LOG:
                    {
                        AccessLog.Add(message);
                        break;
                    }
                case StaticDefine.ADD_CHATTING_LOG:
                    {
                        chattingLog.Add(message);
                        break;
                    }
            }
        }

        private void ServerRun()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any,9999));

            listener.Start();

            while(true)
            {
                Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
                acceptTask.Wait();
                TcpClient newClient = acceptTask.Result;
                _clientManager.AddClient(newClient);
            }
        }

        public void ConsoleView() // ConSoleView에서 ConsoleView로 수정
        {
            while (true)
            {
                Console.WriteLine("=============서버=============");
                Console.WriteLine("1.현재접속인원확인");
                Console.WriteLine("2.접속기록확인");
                Console.WriteLine("3.채팅로그확인");
                Console.WriteLine("0.종료");
                Console.WriteLine("==============================");

                string key = Console.ReadLine();
                int order = 0;


                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case StaticDefine.SHOW_CURRENT_CLIENT:
                            {
                                ShowCurrentClient();
                                break;
                            }
                        case StaticDefine.SHOW_ACCESS_LOG:
                            {
                                ShowAccessLog();
                                break;
                            }
                        case StaticDefine.SHOW_CHATTING_LOG:
                            {
                                ShowCattingLog();
                                break;
                            }

                        case StaticDefine.EXIT:
                            {
                                conntectCheckThread.Abort();
                                return;
                            }
                        default:
                            {
                                Console.WriteLine("잘못 입력하셨습니다.");
                                Console.ReadKey();
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
                Thread.Sleep(50);
            }
        }

            // 채팅로그확인
        private void ShowCattingLog()
        {
            if (chattingLog.Count == 0)
            {
                Console.WriteLine("채팅기록이 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in chattingLog)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        // 접근로그확인
        private void ShowAccessLog()
        {
            if (AccessLog.Count == 0)
            {
                Console.WriteLine("접속기록이 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in AccessLog)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        // 현재접속유저확인
        private void ShowCurrentClient()
        {
            if (_clientManager.clientDic.Count == 0)
            {
                Console.WriteLine("접속자가 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in _clientManager.clientDic)
            {
                Console.WriteLine(item.Value.clientName);
            }
            Console.ReadKey();
        } 
        public void StopServer()
        {
            isRunning = false;
        }
    }
}
