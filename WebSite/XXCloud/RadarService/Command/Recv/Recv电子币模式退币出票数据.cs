using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Recv
{
    public class Recv电子币模式退币出票数据
    {
        public string 机头地址 = "";
        public int 退币数 = 0;
        public DateTime 退币打票时间;
        public UInt16 流水号;
        public byte[] SendData = null;
        public FrameData RecvData;
        //public Ask.Ask电子币模式退币出票数据 应答数据;
        public object 应答数据;
        public DateTime SendDate;
        public Recv电子币模式退币出票数据(FrameData data, DateTime RecvDateTime)
        {
            try
            {
                LogHelper.WriteTBLog(Coding.ConvertData.BytesToString(data.recvData));
                RecvData = data;
                if (data.commandData.Length >= 5)
                {
                    机头地址 = Coding.ConvertData.Hex2String(data.commandData[0]);
                    退币数 = (int)BitConverter.ToUInt16(data.commandData, 1);
                    Info.HeadInfo.机头信息 head = Info.HeadInfo.GetHeadInfo(data.routeAddress, 机头地址);
                    byte[] bs = new byte[8];
                    Array.Copy(data.commandData, 3, bs, 0, 8);
                    退币打票时间 = Coding.ConvertData.GetTimeBCD(bs);
                    流水号 = BitConverter.ToUInt16(data.commandData, 11);
                    byte[] dataBuf;
                    object obj = null;

                    int res = HostServer.CheckRepeat(data.routeAddress, 机头地址, "", CommandType.电子币模式退币出票数据, ref obj, 流水号);
                    if (res == 0)
                    {
                        if (head.类型 == RadarService.Info.HeadInfo.设备类型.碎票机)
                        {
                            应答数据 = new Ask.Ask碎票打印应答(data.routeAddress, 机头地址, 退币数, 流水号);
                            退币打票时间 = ((Ask.Ask碎票打印应答)应答数据).打印时间;
                        }
                        else
                        {
                            应答数据 = new Ask.Ask电子币模式退币出票数据(data.routeAddress, 机头地址, 退币数, 流水号, 退币打票时间);
                            HostServer.InsertRepeat(data.routeAddress, 机头地址, "", CommandType.电子币模式退币出票数据, CommandType.电子币模式退币出票数据应答, 应答数据, 流水号, RecvDateTime);
                        }
                    }
                    else if (res == 1)
                    {
                        if (head.类型 == RadarService.Info.HeadInfo.设备类型.碎票机)
                        {
                            应答数据 = (Ask.Ask碎票打印应答)obj;
                        }
                        else
                        {
                            应答数据 = (Ask.Ask电子币模式退币出票数据)obj;
                            ((Ask.Ask电子币模式退币出票数据)应答数据).打印时间 = 退币打票时间;
                            TableMemory.flw_ticket_exit.UpdatePrintDate(((Ask.Ask电子币模式退币出票数据)应答数据).条码号, 退币打票时间);
                        }
                    }
                    else
                    {
                        //重复性检查出错
                        return;
                    }
                    if (head.类型 == RadarService.Info.HeadInfo.设备类型.碎票机)
                        dataBuf = PubLib.GetBytesByObject((Ask.Ask碎票打印应答)应答数据);
                    else
                        dataBuf = PubLib.GetBytesByObject((Ask.Ask电子币模式退币出票数据)应答数据);
                    SendData = PubLib.GetFrameDataBytes(data, dataBuf, CommandType.电子币模式退币出票数据应答);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }
        public void AskData()
        {
            if (SendData != null)
            {
                TerminalDataProcess.SendData(SendData);
                SendDate = DateTime.Now;
            }
        }

        public string GetRecvData(DateTime printDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", printDate);
            sb.Append(PubLib.BytesToString(RecvData.recvData) + Environment.NewLine);
            sb.AppendFormat("指令类别：{0}\r\n", RecvData.commandType);
            sb.AppendFormat("路由器地址：{0}\r\n", RecvData.routeAddress);
            sb.AppendFormat("机头地址：{0}\r\n", 机头地址);
            sb.AppendFormat("退币数：{0}\r\n", 退币数);
            sb.AppendFormat("流水号：{0}\r\n", 流水号);
            sb.AppendFormat("打印时间：{0}\r\n", 退币打票时间.ToString("yyyy-MM-dd HH:mm:ss"));
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            if (SendData != null)
            {
                Info.HeadInfo.机头信息 head = Info.HeadInfo.GetHeadInfo(PubLib.路由器段号, 机头地址);
                if (head.类型 == RadarService.Info.HeadInfo.设备类型.碎票机)
                {
                    sb.Append("=============================================\r\n");
                    sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDate);
                    sb.Append(PubLib.BytesToString(SendData) + Environment.NewLine);
                    sb.AppendFormat("机头地址：{0}\r\n", PubLib.Hex2String(((Ask.Ask碎票打印应答)应答数据).机头地址));
                    sb.AppendFormat("条码号：{0}\r\n", (Encoding.GetEncoding("gb2312").GetString(((Ask.Ask碎票打印应答)应答数据).条码号)));
                    sb.AppendFormat("碎票数：{0}\r\n", ((Ask.Ask碎票打印应答)应答数据).碎票数);
                    sb.AppendFormat("票余额：{0}\r\n", ((Ask.Ask碎票打印应答)应答数据).票余额);
                    sb.AppendFormat("流水号：{0}\r\n", ((Ask.Ask碎票打印应答)应答数据).流水号);
                    sb.AppendFormat("用户编号：{0}\r\n", (Encoding.GetEncoding("gb2312").GetString(((Ask.Ask碎票打印应答)应答数据).用户编号)));
                }
                else
                {
                    sb.Append("=============================================\r\n");
                    sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDate);
                    sb.Append(PubLib.BytesToString(SendData) + Environment.NewLine);
                    sb.AppendFormat("机头地址：{0}\r\n", PubLib.Hex2String(((Ask.Ask电子币模式退币出票数据)应答数据).机头地址));
                    sb.AppendFormat("条码号：{0}\r\n", ((Ask.Ask电子币模式退币出票数据)应答数据).条码号);
                    sb.AppendFormat("动态密码：{0}\r\n", PubLib.Hex2String(((Ask.Ask电子币模式退币出票数据)应答数据).动态密码));
                    sb.AppendFormat("退币数：{0}\r\n", ((Ask.Ask电子币模式退币出票数据)应答数据).退币数);
                    sb.AppendFormat("脉冲数：{0}\r\n", ((Ask.Ask电子币模式退币出票数据)应答数据).脉冲数);
                    sb.AppendFormat("流水号：{0}\r\n", ((Ask.Ask电子币模式退币出票数据)应答数据).流水号);
                    sb.AppendFormat("控制信号：{0}\r\n", PubLib.Hex2BitString(((Ask.Ask电子币模式退币出票数据)应答数据).控制信号));
                }
            }
            return sb.ToString();
        }
    }
}
