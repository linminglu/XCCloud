using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Recv
{
    public class Recv远程被动退分解锁应答
    {
        public Recv远程被动退分解锁应答(FrameData data)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("=============================================\r\n");
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", DateTime.Now);
                sb.Append(PubLib.BytesToString(data.recvData) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", data.commandType);
                sb.AppendFormat("路由器地址：{0}\r\n", data.routeAddress);

                UIClass.接收内容 = sb.ToString();           
            }
            catch 
            {
                throw;
            }
        }
    }
}
