using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace message_3
{
    class ClientData
    {
        public TcpClient tcpclient {get; set;}
        public byte[] readBuffer {get; set;}
        public StringBuilder currentMsg {get;set;}
        public string clientName {get; set;}
        public int clientNumber {get; set;}

        public ClientData(TcpClient tcpclient)
        {
            currentMsg = new StringBuilder();
            readBuffer = new byte[1024];
            this.tcpclient = tcpclient;
            char[] splitDivision = new char[2];
            splitDivision[0]='.';
            splitDivision[1]=':';

            string[] temp = null;

            temp = tcpclient.Client.LocalEndPoint.ToString().Split(splitDivision);
            this.clientName=int.Parse(temp[3]);

           
        }
    }
}