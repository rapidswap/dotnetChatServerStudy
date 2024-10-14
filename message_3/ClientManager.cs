using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace message_3
{
    public class ClientManager
    {
        internal ConcurrentDictionary<int, ClientData> clientDic { get; } = new ConcurrentDictionary<int, ClientData>();
        
        public delegate void LogHandler(string message, int key);
        public LogHandler EventHandler;
        
        public delegate void MessageParsingHandler(string sender, string message);
        public MessageParsingHandler messageParsingAction;

        public ClientManager()
        {
            clientDic = new ConcurrentDictionary<int, ClientData>();
            EventHandler = null;
            messageParsingAction = null;
        }

        public void AddClient(TcpClient tcpClient)
        {
            try
            {
                int clientNumber = clientDic.Count + 1;
                ClientData clientData = new ClientData(tcpClient, clientNumber);
                clientDic.TryAdd(clientNumber, clientData);

                string accessLog = string.Format("[{0}] {1} 서버 접속", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), clientData.clientName);
                EventHandler?.Invoke(accessLog, StaticDefine.ADD_ACCESS_LOG);

                Thread receiveThread = new Thread(() => ReceiveMessage(clientData));
                receiveThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"클라이언트 추가 오류: {e.Message}");
            }
        }

        private void ReceiveMessage(ClientData clientData)
        {
            NetworkStream stream = clientData.tcpClient.GetStream();
            byte[] buffer = new byte[1024];
            
            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // 클라이언트 연결 종료
                        break;
                    }

                    string strData = Encoding.Default.GetString(buffer, 0, bytesRead);
                    ProcessReceivedData(clientData, strData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"클라이언트 수신 오류: {e.Message}");
            }
            finally
            {
                // 클라이언트 연결 종료 처리
                RemoveClient(clientData);
            }
        }

        private void ProcessReceivedData(ClientData client, string strData)
        {
            if (string.IsNullOrEmpty(client.clientName) || client.clientName == "Unknown")
            {
                if (CheckID(strData))
                {
                    string userName = strData.Substring(3);
                    client.clientName = userName;
                    string accessLog = string.Format("[{0}] {1} Access Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), client.clientName);
                    EventHandler?.Invoke(accessLog, StaticDefine.ADD_ACCESS_LOG);
                    return;
                }
            }
            
            messageParsingAction?.Invoke(client.clientName, strData);
        }

        private void RemoveClient(ClientData clientData)
        {
            ClientData removedClient;
            if (clientDic.TryRemove(clientData.clientNumber, out removedClient))
            {
                string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), removedClient.clientName);
                EventHandler?.Invoke(leaveLog, StaticDefine.ADD_ACCESS_LOG);
            }
        }

        private bool CheckID(string ID)
        {
            return ID.Contains("%^&");
        }
    }
}
