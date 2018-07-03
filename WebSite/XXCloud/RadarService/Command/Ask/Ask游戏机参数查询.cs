using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask游戏机参数查询
    {
        public string 机头地址 = "";
        public Ask游戏机参数查询(string hAddress)
        {
            try
            {
                List<byte> sendbuf = new List<byte>();
                sendbuf.Add(Convert.ToByte(hAddress, 16));
                机头地址 = hAddress;
                FrameData data = new FrameData();
                data.routeAddress = PubLib.路由器段号;
                data.commandType = COMObject.CommandType.游戏机参数查询;
                byte[] Send = PubLib.GetFrameDataBytes(data, sendbuf.ToArray(), COMObject.CommandType.游戏机参数查询);
                StringBuilder sb = new StringBuilder();
                sb.Append(PubLib.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", COMObject.CommandType.游戏机参数查询);
                sb.AppendFormat("机头地址：{0}\r\n", hAddress);
                TerminalDataProcess.BodySend(机头地址, Send, data.commandType, COMObject.CommandType.游戏机参数查询应答, sb.ToString(), hAddress);
            }
            catch 
            {
                throw;
            }
        }
    }
}
