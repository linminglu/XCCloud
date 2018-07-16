using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Server
{
    class StateObject
    {
        public const int BUF_SIZE = 1024 * 128;
        public Socket socket;
        public byte[] buffer = new byte[BUF_SIZE];
    }
}
