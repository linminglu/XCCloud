using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Server
{
    enum TransmiteEnum
    {
        门店数据变更同步请求 = 0xe0,
        门店数据变更同步应答 = 0x20,

        云端数据变更同步请求 = 0xe1,
        云端数据变更同步应答 = 0x21,

        通知服务器上线 = 0xf0,
        通知服务器上线应答 = 0x10,

        心跳 = 0xf1,
    }
}
