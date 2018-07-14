using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Recv
{
    public class Recv液晶卡头读卡指令
    {
        public string 机头地址 = "";
        public string IC卡号码 = "";
        public byte 动态密码 = 0;
        public UInt16 流水号;
        public byte[] SendData = null;
        public FrameData RecvData;
        public Ask.Ask液晶卡头读卡指令 应答数据;
        public DateTime SendDataTime;


        public Recv液晶卡头读卡指令(FrameData data, DateTime RecvDateTime)
        {
            try
            {
                if (data.commandLength != 0x0c) return;
                机头地址 = Coding.ConvertData.Hex2String(data.commandData[0]);
                for (int i = 0; i < 8; i++)
                {
                    byte rData = data.commandData[i + 1];
                    if (rData >= 0x30 && rData <= 0x39)
                    {
                        IC卡号码 += Encoding.ASCII.GetString(new byte[] { rData });
                    }
                    else if (rData != 0x00)
                    {
                        IC卡号码 = "";
                        break;
                    }
                }

                动态密码 = data.commandData[9];
                流水号 = BitConverter.ToUInt16(data.commandData, 10);
                Info.DeviceInfo.机头信息 head = Info.DeviceInfo.GetBufMCUIDDeviceInfo(data.routeAddress, 机头地址);
                if (head == null) return;
                //防霸位功能检查
                if (!霸位检查(head, IC卡号码) && IC卡号码 != "")
                {
                    object obj = null;
                    int res = HostServer.CheckRepeat(head.路由器段号, 机头地址, IC卡号码, CommandType.液晶卡头读卡指令, ref obj, 流水号);
                    if (res == 0)
                    {
                        应答数据 = new Ask.Ask液晶卡头读卡指令(head, IC卡号码, 动态密码, 流水号);
                        HostServer.InsertRepeat(data.routeAddress, 机头地址, IC卡号码, CommandType.液晶卡头读卡指令, CommandType.液晶卡头读卡应答指令, 应答数据, 流水号, RecvDateTime);
                    }
                    else if (res == 1)
                    {
                        应答数据 = (Ask.Ask液晶卡头读卡指令)obj;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    应答数据 = new Ask.Ask液晶卡头读卡指令(head, IC卡号码, 动态密码, 流水号);
                    应答数据.按钮一扣值余额 = 0;
                    应答数据.按钮二扣值余额 = 0;
                    应答数据.机头地址 = Convert.ToByte(机头地址, 16);
                    应答数据.卡类别 = 0;
                    应答数据.控制信号 = 0;
                    应答数据.按钮一扣卡里币数基数 = 0;
                    应答数据.按钮二扣卡里币数基数 = 0;
                    应答数据.流水号 = 流水号;
                }
                byte[] dataBuf = Coding.ConvertData.GetBytesByObject(应答数据);
                SendData = Coding.ConvertData.GetFrameDataBytes(data, dataBuf, CommandType.液晶卡头读卡应答指令);
                RecvData = data;
            }
            catch
            {
                throw;
            }
        }

        bool 霸位检查(Info.DeviceInfo.机头信息 head, string ICCardID)
        {
            return false;
            //HashSet<Info.DeviceInfo.机头信息> list = XCCloudSerialNo.SerialNoHelper.StringGet<HashSet<Info.DeviceInfo.机头信息>>(ICCardID);
            //if (list == null)
            //{
            //    list = new HashSet<Info.DeviceInfo.机头信息>();
            //    list.Add(head);
            //}
            //else
            //{
            //    if (list.Contains(head))
            //    {

            //    }
            //}              
        }

        //public void AskData()
        //{
        //    if (SendData != null)
        //    {
        //        TerminalDataProcess.SendData(SendData);
        //        SendDataTime = DateTime.Now;
        //        TerminalDataProcess.UpdateRepeatTime(PubLib.路由器段号, 机头地址, 流水号);
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
            sb.AppendFormat("IC卡号码：{0}\r\n", IC卡号码);
            sb.AppendFormat("动态密码：{0}\r\n", Coding.ConvertData.Hex2String(动态密码));
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
                sb.AppendFormat("卡类别：{0}\r\n", 应答数据.卡类别);
                sb.AppendFormat("按钮一扣卡里币数基数：{0}\r\n", 应答数据.按钮一扣卡里币数基数);
                sb.AppendFormat("按钮一扣值余额：{0}\r\n", 应答数据.按钮一扣值余额);
                sb.AppendFormat("按钮二扣卡里币数基数：{0}\r\n", 应答数据.按钮二扣卡里币数基数);
                sb.AppendFormat("按钮二扣值余额：{0}\r\n", 应答数据.按钮二扣值余额);
                sb.AppendFormat("显示方案索引：{0}\r\n", 应答数据.显示方案索引);
                sb.AppendFormat("门票剩余次数：{0}\r\n", 应答数据.门票剩余次数);
                string ss = Encoding.GetEncoding("gb2312").GetString(应答数据.门票名).Replace("\0", "");
                sb.AppendFormat("门票名：{0}\r\n", Encoding.GetEncoding("gb2312").GetString(应答数据.门票名).Replace("\0", ""));
                sb.AppendFormat("有效期：{0}\r\n", Coding.ConvertData.GetTicketDate(应答数据.有效期));
                sb.AppendFormat("控制信号：{0}\r\n", 应答数据.控制信号);
            }
            return sb.ToString();
        }
    }
}
