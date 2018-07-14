using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RadarService.Command.Ask
{
    public class Ask远程投币上分数据
    {
        public bool IsSuccess = false;
        public string Message = "";

        public Ask远程投币上分数据(Info.DeviceInfo.机头信息 head, int Coins, string pushType, int PushSN, bool 专卡专用, EndPoint p)
        {
            try
            {
                if (head != null)
                {
                    FrameData data = new FrameData();
                    data.commandType = CommandType.远程投币上分指令;
                    data.routeAddress = head.路由器段号;
                    List<byte> dataList = new List<byte>();
                    dataList.Add((byte)Convert.ToByte(head.机头短地址, 16));
                    dataList.AddRange(BitConverter.GetBytes((UInt16)Coins));
                    dataList.Add((byte)(专卡专用 ? 7 : 0));
                    dataList.AddRange(BitConverter.GetBytes((UInt16)PushSN));
                    byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, dataList.ToArray(), CommandType.远程投币上分指令);

                    StringBuilder sb = new StringBuilder();
                    sb.Append(Coding.ConvertData.BytesToString(Send) + Environment.NewLine);
                    sb.AppendFormat("指令类别：{0}\r\n", CommandType.远程投币上分指令);
                    sb.AppendFormat("机头地址：{0}\r\n", head.机头短地址);
                    sb.AppendFormat("投币数：{0}\r\n", Coins);
                    sb.AppendFormat("投币类别：{0}\r\n", pushType);
                    sb.AppendFormat("流水号：{0}\r\n", PushSN);
                    Console.WriteLine(sb.ToString());

                    HostServer.BodySend(null, Send, data.commandType, CommandType.远程投币上分指令应答, sb.ToString(), PushSN, head.机头短地址, p);

                    LogHelper.LogHelper.WritePush(Send, head.路由器段号, head.机头短地址, pushType, Coins.ToString(), PushSN.ToString());
                    IsSuccess = true;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
