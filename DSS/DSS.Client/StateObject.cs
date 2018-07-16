using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace DSS.Client
{
    class StateObject
    {
        public const int BUF_SIZE = 1024 * 8;
        public Socket socket;
        public byte[] buffer = new byte[BUF_SIZE];
    }
}
