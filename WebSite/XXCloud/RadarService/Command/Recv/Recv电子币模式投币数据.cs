using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Recv
{
    public class Recv电子币模式投币数据
    {
        public string 机头地址 = "";
        public UInt32 UID = 0;
        public UInt16 流水号;
        public byte[] SendData = null;
        public FrameData RecvData;
        public Ask.Ask电子币模式投币数据 应答数据;
        public DateTime SendDataTime;

        public Recv电子币模式投币数据(FrameData data, DateTime RecvDateTime)
        {
            try
            {
                RecvData = data;
                if (data.commandData.Length >= 12)
                {
                    机头地址 = Coding.ConvertData.Hex2String(data.commandData[0]);
                    UID = BitConverter.ToUInt32(data.commandData, 1);
                    流水号 = BitConverter.ToUInt16(data.commandData, 10);
                    object obj = null;
                    int res = HostServer.CheckRepeat(data.routeAddress, 机头地址, UID.ToString(), CommandType.电子币模式投币数据, ref obj, 流水号);
                    if (res == 0)
                    {
                        应答数据 = new Ask.Ask电子币模式投币数据(data.routeAddress, 机头地址, UID, 流水号);
                        HostServer.InsertRepeat(data.routeAddress, 机头地址, UID.ToString(), CommandType.电子币模式投币数据, CommandType.电子币模式投币数据应答, 应答数据, 流水号, RecvDateTime);
                    }
                    else if (res == 1)
                    {
                        应答数据 = (Ask.Ask电子币模式投币数据)obj;
                    }
                    else
                    {
                        //重复性检查出错，等待重试
                        return;
                    }
                    byte[] dataBuf = Coding.ConvertData.GetBytesByObject(应答数据);
                    SendData = Coding.ConvertData.GetFrameDataBytes(data, dataBuf, CommandType.电子币模式投币数据应答);
                }
            }
            catch
            {
                throw;
            }
        }
        //public void AskData()
        //{
        //    if (SendData != null)
        //    {
        //        TerminalDataProcess.SendData(SendData);
        //        SendDataTime = DateTime.Now;
        //    }
        //}

        public string GetRecvData(DateTime printDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", printDate);
            sb.Append(Coding.ConvertData.BytesToString(RecvData.recvData) + Environment.NewLine);
            sb.AppendFormat("指令类别：{0}\r\n", RecvData.commandType);
            sb.AppendFormat("路由器地址：{0}\r\n", RecvData.routeAddress);
            sb.AppendFormat("机头地址：{0}\r\n", 机头地址);
            sb.AppendFormat("IC卡号：{0}\r\n", 应答数据.数字币编号);
            sb.AppendFormat("流水号：{0}\r\n", 流水号);
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            if (SendData != null)
            {
                sb.Append("=============================================\r\n");
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
                sb.Append(Coding.ConvertData.BytesToString(SendData) + Environment.NewLine);
                sb.AppendFormat("机头地址：{0}\r\n", Coding.ConvertData.Hex2String(应答数据.机头地址));
                sb.AppendFormat("币数：{0}\r\n", 应答数据.币数);
                sb.AppendFormat("扣币数：{0}\r\n", 应答数据.扣币数);
                sb.AppendFormat("脉冲数：{0}\r\n", 应答数据.脉冲数);
                sb.AppendFormat("流水号：{0}\r\n", 应答数据.流水号);
                sb.AppendFormat("控制信号：{0}\r\n", Coding.ConvertData.Hex2BitString(应答数据.控制信号));
            }
            return sb.ToString();
        }
    }
}
