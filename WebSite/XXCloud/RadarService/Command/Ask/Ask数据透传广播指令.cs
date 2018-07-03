using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public enum 透传指令类别
    {
        LOGO图片 = 0,
        彩票文字 = 1,
        打印顺序 = 2,
    }
    public class Ask数据透传广播指令
    {
        public Ask数据透传广播指令(透传指令类别 type, byte version, int index, int count, byte[] dataBUF)
        {
            try
            {
                List<byte> sendbuf = new List<byte>();

                sendbuf.Add((byte)(index));
                sendbuf.Add((byte)count);
                sendbuf.Add((byte)type);
                sendbuf.Add(version);
                sendbuf.AddRange(dataBUF);

                FrameData data = new FrameData();
                data.routeAddress = PubLib.路由器段号;
                data.commandType = COMObject.CommandType.数据透传广播指令;
                byte[] Send = PubLib.GetFrameDataBytes(data, sendbuf.ToArray(), COMObject.CommandType.数据透传广播指令);
                StringBuilder sb = new StringBuilder();
                sb.Append(PubLib.BytesToString(Send) + Environment.NewLine);
                TerminalDataProcess.BodySendEx(null, Send, data.commandType, COMObject.CommandType.数据透传广播指令应答, sb.ToString());
            }
            catch 
            {
                throw;
            }
        }
    }
}
