using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Recv
{
    public class Recv机头地址动态分配
    {
        public string MCUID = "";
        public FrameData RecvData;
        public string 机头地址;
        public byte[] SendData;
        public DateTime SendDataTime;

        public Recv机头地址动态分配(FrameData data)
        {
            try
            {
                if (data.commandData.Length == 7)
                {
                    byte[] temp = new byte[8];
                    Array.Copy(data.commandData, temp, 7);
                    temp[7] = 0x20;
                    data.commandData = temp;
                }
                RecvData = data;

                if (data.commandData.Length >= 8)
                {
                    List<byte> mid = new List<byte>(data.commandData);
                    mid.Add(0x00);
                    bool IsNew = (mid[0] == 1);
                    UInt64 ui64 = BitConverter.ToUInt64(mid.ToArray(), 1);
                    MCUID = Convert.ToString((long)ui64, 16).ToUpper();

                    //if (!SecrityHeadInfo.CheckHead(MCUID)) return;
                    Ask.Ask机头地址动态分配 ask = new Command.Ask.Ask机头地址动态分配(data.routeAddress, MCUID, IsNew);
                    if (ask.isSuccess)
                    {
                        机头地址 = Coding.ConvertData.Hex2String(ask.机头地址);
                        byte[] readyToSend = Coding.ConvertData.GetBytesByObject(ask);
                        SendData = Coding.ConvertData.GetFrameDataBytes(data, readyToSend, CommandType.机头地址动态分配应答);
                        LogHelper.LogHelper.WriteLogSpacail(string.Format("{0} 申请地址 {1}", MCUID, 机头地址));
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public string GetRecvData(DateTime printDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", printDate);
            sb.Append(Coding.ConvertData.BytesToString(RecvData.recvData) + Environment.NewLine);
            sb.AppendFormat("指令类别：{0}\r\n", RecvData.commandType);
            sb.AppendFormat("路由器地址：{0}\r\n", RecvData.routeAddress);
            sb.AppendFormat("长地址：{0}\r\n", MCUID);
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
            sb.Append(Coding.ConvertData.BytesToString(SendData) + Environment.NewLine);
            sb.AppendFormat("机头地址：{0}\r\n", 机头地址);
            sb.AppendFormat("长地址：{0}\r\n", MCUID);
            return sb.ToString();
        }
    }
}
