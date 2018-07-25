using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RadarService.Command.Ask
{
    public class Ask机头锁定解锁指令
    {
        public bool IsSuccess = false;
        public Ask机头锁定解锁指令(Info.DeviceInfo.机头信息 head, bool isLock, EndPoint p)
        {
            try
            {
                FrameData data = new FrameData();
                data.commandType = CommandType.机头锁定解锁指令;
                data.routeAddress = head.路由器段号;
                List<byte> dataList = new List<byte>();
                dataList.Add((byte)Convert.ToByte(head.机头短地址, 16));
                dataList.Add((byte)(isLock ? 1 : 0));
                byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, dataList.ToArray(), CommandType.机头锁定解锁指令);

                StringBuilder sb = new StringBuilder();
                sb.Append(Coding.ConvertData.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", CommandType.机头锁定解锁指令);
                sb.AppendFormat("机头地址：{0}\r\n", head.机头短地址);
                sb.AppendFormat("操作：{0}\r\n", isLock);
                IsSuccess = true;
                HostServer.BodySend(null, Send, data.commandType, CommandType.机头锁定解锁指令应答, sb.ToString(), PublicHelper.SystemDefiner.雷达主发流水号, head.机头短地址, p, head.路由器段号);
            }
            catch(Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
                throw ex;
            }
        }
    }
}
