using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask卡头升级通知
    {
        public Ask卡头升级通知(int 大帧数, string 文件名)
        {
            try
            {
                FrameData data = new FrameData();
                data.commandType = CommandType.卡头程序更新通知;
                data.routeAddress = PubLib.路由器段号;
                List<byte> dataList = new List<byte>();
                dataList.Add((byte)大帧数);
                dataList.AddRange(Encoding.ASCII.GetBytes(文件名));

                byte[] Send = PubLib.GetFrameDataBytes(data, dataList.ToArray(), CommandType.卡头程序更新通知);

                StringBuilder sb = new StringBuilder();
                sb.Append(PubLib.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", CommandType.卡头程序更新通知);
                sb.AppendFormat("大帧数：{0}\r\n", 大帧数);
                sb.AppendFormat("文件名：{0}\r\n", 文件名);

                TerminalDataProcess.BodySend(null, Send, data.commandType, CommandType.卡头程序更新通知应答, sb.ToString(), "FF");
            }
            catch
            {
                throw;
            }
        }
    }
}
