using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace message_3
{
    class ClinetManager
    {
        public static ConcurrentDictionary<int, ClientData> clientDic = new ConcurrentDictionary<int, ClientData>();
        public event Action<string, string> messageParsingAction = null;
        public event Action<string, int> EventHandler =null;

        public void AddClient(TcpClient newClient)
        {
            ClinetData currentClient = new ClinetData(newClient);

            try{
                currentClient.tcpClient.GetStream().BeginRead(currentClient.readBuffer,0,currentClient.readBuffer.Length, new AsyncCallback(DataReceived), currentClient);
                clientDic.TryAdd(currentClient.clientNumber,currentClient);
            }
            catch(Exceptin e)
            {

            }
        }

        private void DataReceived(IAsyncResult ar)
        {
            ClientData client = ar.AsyncState as ClientData;

            try
            {
                int byteLength = client.tcpClient.GetStream().EndRead(ar);
                string strData = Encoding.Default.GetString(client.readBuffer,0,byteLength);
                client.tcpClient.GetStream().BeginRead(client.readBuffer,0,client.readBuffer.Length,new AsyncCallback(DataReceived),client);
                if (string.IsNullOrEmpty(client.clientName))
                {
                    if(EventHandler !=null)
                    {
                        if(CheckID(strData))
                        {
                            string userName = strData.Substring(3);
                            client.clientName = userName;
                            string accessLog = string.Format("[{0}] {1} Access Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),client.clientName);
                            EventHandler.Invoke(accessLog, StaticDefine.ADD_ACCESS_LOG);
                            return;
                        }
                    }
                }
                if (messageParsingAction != null)
                {
                    messageParsingAction.BeginInvoke(client.clientName, strData,null,null);
                }
            }
            catch (Exceptin e)
            {

            }
        }

        private bool CheckID(string ID)
        {
            if(ID.Contains("%^&"))
                return true;
            
            return false;
        }
    }
    
}
