using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Msg3Client
{
    class StaticDefine
    {
        public const int CONNECT = 1;
        public const int SEND_MESSAGE = 2;
        public const int SEND_MSG_VIEW = 3;
        public const int RECEIVE_MSG_VIEW = 4;
        public const int EXIT = 0;
    }
}