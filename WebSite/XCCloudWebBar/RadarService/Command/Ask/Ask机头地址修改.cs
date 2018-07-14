using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net;

namespace RadarService.Command.Ask
{
    public class Ask机头地址修改
    {
        public Ask机头地址修改(string mcuid,string headAddress,string segment, EndPoint p)
        {
            try
            {
                FrameData data = new FrameData();
                data.routeAddress = segment;
                Ask.Ask机头地址动态分配 ask = new Command.Ask.Ask机头地址动态分配(data.routeAddress, mcuid, false);
                ask.机头地址 = Convert.ToByte(headAddress, 16);
                byte[] dataBuf = Coding.ConvertData.GetBytesByObject(ask);
                byte[] readyToSend = Coding.ConvertData.GetFrameDataBytes(data, dataBuf, CommandType.机头地址修改);
                StringBuilder sb = new StringBuilder();
                sb.Append(Coding.ConvertData.BytesToString(readyToSend) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", CommandType.机头地址修改);
                sb.AppendFormat("机头地址：{0}\r\n", headAddress);
                Console.WriteLine(sb.ToString());
                HostServer.BodySend(ask, readyToSend, data.commandType, CommandType.机头地址修改应答, sb.ToString(), PublicHelper.SystemDefiner.雷达主发流水号, headAddress, p);
            }
            catch
            {
                throw;
            }
        }
    }
}
