using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask游戏机退币不扣分检测
    {
        public Ask游戏机退币不扣分检测(string HeadAddress)
        {
            try
            {
                FrameData data = new FrameData();
                data.commandType = CommandType.退币信号延时检测指令;
                data.routeAddress = PubLib.路由器段号;
                List<byte> dataList = new List<byte>();
                dataList.Add((byte)Convert.ToByte(HeadAddress, 16));
                byte[] Send = PubLib.GetFrameDataBytes(data, dataList.ToArray(), CommandType.退币信号延时检测指令);

                StringBuilder sb = new StringBuilder();
                sb.Append(PubLib.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", CommandType.远程被动退分解锁指令);
                sb.AppendFormat("机头地址：{0}\r\n", HeadAddress);

                TerminalDataProcess.BodySend(null, Send, data.commandType, CommandType.退币信号延时检测指令应答, sb.ToString(), HeadAddress);
            }
            catch
            {
                throw;
            }
        }
    }
}
