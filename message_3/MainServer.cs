using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace message_3
{
    public class MainServer
    {
        ClinetManager _clientManager = null;
        ConcurrentBag<string> chattingLog=null;
        ConcurrentBag<string> AccessLog=null;
        Thread conntectCheckThread=null;

        public MainServer()
        {
            _clientManager = new ClinetManager();
            chattingLog = new ConcurrentBag<string>();
            AccessLog = new ConcurrentBag<string>();
            _clientManager.EventHandler += ClinetEvent;
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
            while(true)
            {
                foreach(var item in _clientManager.clientDic)
                {
                    try{
                        string sendStringData="관리자<TEST>";
                        byte[] sendByteData = new byte[sendStringData.Length];
                        sendByteData=Encoding.Default.GetBytes(sendStringData);

                        item.Value.TcpClient.GetStream().Write(sendByteData,0,sendByteData.Length);
                    }
                    catch(Exception e)
                    {
                        RemoveClient(item.Value);
                    }
                }
                conntectCheckThread.Sleep(1000);
            }
        }

        private void RemoveClient(ClientData targetClient)
        {
            ClientData result = null;
            _clientManager.clientDic.TryRemove(targetClient.clientNumber,out result);
            string leaveLog=string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),result.clientName);
            AccessLog.Add(leaveLog);
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
            string LogMessage="";
            string parsedMessage="";
            string receiver="";

            int senderNumber = -1;
            int receiverNumber = -1;

            foreach(var item in msgList)
            {
                string[] splitedMsg = item.Split('<');
                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>",sender,splitedMsg[1]);
                senderNumber = GetClientNumber(sender);
                receiverNumber = GetClientNumber(receiver);
                if (senderNumber == -1 || receiverNumber == -1)
                {
                    return;
                }
                
                if (parsedMessage.Contains("<GiveMeUserList>"))
                {
                    string userListStringData = "관리자<";
                    foreach (var el in ClientManager.clientDic)
                    {
                        userListStringData += string.Format("${0}",el.Value.clientName);
                    }
                    userListStringData+=">";
                    byte[] userListByteData = new byte[userListStringData.Length];
                    userListByteData = Encoding.Default.GetBytes(userListStringData);
                    ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(userListByteData,0,userListByteData.Length);
                    return;
                }
                LogMessage = string.Format(@"[{0}] [{1}] -> [{2}]  ,  {3}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, receiver, splitedMsg[1]);
                ClientEvent(LogMessage, StaticDefine.ADD_CHATTING_LOG);
                byte[] sendByteData = Encoding.Default.GetBytes(parsedMessage);
                ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendByteData,0,sendByteData.Length);
                
            }
        }

        private interface GetClientNumber(string targetClientName)
        {
            foreach(var item in ClientManager.clientDic)
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
                case StaticDefine.ADD_ACCESS_LOG;
                {
                    AccessLog.Add(message);
                    break;
                }
                case StaticDefine.ADD_CHATTING_LOG
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

        public void ConSoleVIew()
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
            if (ClientManager.clientDic.Count == 0)
            {
                Console.WriteLine("접속자가 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in ClientManager.clientDic)
            {
                Console.WriteLine(item.Value.clientName);
            }
            Console.ReadKey();
        } 
    }
}