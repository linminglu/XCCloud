using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RadarService.Command.Ask
{
    public class Ask设置路由器地址
    {
        public Ask设置路由器地址(string routeAddress, EndPoint p)
        {
            try
            {
                byte[] routeBytes = BitConverter.GetBytes(Convert.ToUInt16(routeAddress, 16));
                FrameData data = new FrameData();
                data.commandType = CommandType.设置路由器地址;
                data.routeAddress = "FFFF";
                byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, routeBytes, CommandType.设置路由器地址);

                StringBuilder sb = new StringBuilder();
                sb.Append(Coding.ConvertData.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", CommandType.设置路由器地址);
                sb.AppendFormat("路由器地址：{0}\r\n", routeAddress);

                HostServer.BodySend(null, Send, data.commandType, CommandType.设置路由器地址应答, sb.ToString(), 0, "FF", p);
            }
            catch
            {
                throw;
            }
        }
    }
}
