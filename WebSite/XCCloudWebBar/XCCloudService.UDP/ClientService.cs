using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.SocketService.UDP.Common;

namespace XCCloudWebBar.SocketService.UDP
{
    public class ClientService
    {
        public void Connection()
        {
            Client.Init(UDPConfig.Host, UDPConfig.Port, System.Guid.NewGuid(), UDPSocketClientType.串口通讯服务);

        }

        public void Send(byte[] data)
        {
            Client.Send(data);
        }
    }
}
