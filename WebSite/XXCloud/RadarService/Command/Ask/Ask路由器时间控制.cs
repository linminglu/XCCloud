using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask路由器时间控制
    {
        public Ask路由器时间控制(string date, int IsSet)
        {
            try
            {
                FrameData data = new FrameData();
                data.commandType = COMObject.CommandType.路由器时间控制;
                data.routeAddress = "FFFF";
                List<byte> dataList = new List<byte>();
                dataList.Add((byte)IsSet);
                DateTime d;

                if (IsSet == 1)
                {
                    byte[] dateBytes;
                    d = Convert.ToDateTime(date);
                    dateBytes = PubLib.DateTimeBCD(d);
                    dataList.AddRange(dateBytes);
                }

                byte[] Send = PubLib.GetFrameDataBytes(data, dataList.ToArray(), COMObject.CommandType.路由器时间控制);
                StringBuilder sb = new StringBuilder();
                sb.Append(PubLib.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", COMObject.CommandType.路由器时间控制);
                sb.AppendFormat("控制类型：{0}\r\n", IsSet);
                sb.AppendFormat("时间：{0}\r\n", date);
                TerminalDataProcess.BodySend(null, Send, data.commandType, COMObject.CommandType.路由器时间控制应答, sb.ToString(), "FF");
            }
            catch
            {
                throw;
            }
        }
    }
}
