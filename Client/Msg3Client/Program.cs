using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Msg3Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleClient a = new ConsoleClient();
            a.Run();
        }
    }
}