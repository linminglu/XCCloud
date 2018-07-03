using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Recv
{
    public class Recv投币机投币指令
    {
        public string 投币机地址 = "";
        public string 目标机头地址 = "";
        public int 投币类别;
        public UInt16 投币量;
        public string IC卡号码 = "";
        public byte 动态密码;
        public DateTime 投币时间;
        public UInt16 流水号;
        public byte[] SendData = null;
        public FrameData RecvData;
        public Ask.Ask投币机投币指令应答 应答数据;
        public DateTime SendDataTime;

        public Recv投币机投币指令(FrameData f, DateTime RecvDateTime)
        {
            RecvData = f;
            try
            {
                if (f.commandData.Length >= 25 && f.commandLength == 0x1a)
                {
                    投币机地址 = PubLib.Hex2String(f.commandData[0]);
                    目标机头地址 = PubLib.Hex2String(f.commandData[1]);
                    投币类别 = f.commandData[2];
                    投币量 = BitConverter.ToUInt16(f.commandData, 3);
                    IC卡号码 = Encoding.ASCII.GetString(f.commandData, 5, 8).Replace("\0", "");
                    动态密码 = f.commandData[13];
                    投币时间 = PubLib.GetTimeBCD(f.commandData.Skip(14).Take(8).ToArray());
                    流水号 = BitConverter.ToUInt16(f.commandData, 24);
                    if (投币量 == 0 && 流水号 == 0) return;
                    Info.HeadInfo.机头信息 head = Info.HeadInfo.GetHeadInfo(PubLib.路由器段号, 投币机地址);
                    if (head != null)
                    {
                        object obj = null;
                        int res = TerminalDataProcess.CheckRepeat(PubLib.路由器段号, 投币机地址, IC卡号码, CommandType.投币机投币指令, ref obj, 流水号);
                        Console.WriteLine("重复性检查结果：" + res);
                        if (res == 0)
                        {

                            应答数据 = new Ask.Ask投币机投币指令应答(PubLib.路由器段号, 投币机地址, 目标机头地址, 投币类别, IC卡号码, 动态密码, 投币量, (UInt16)流水号, 投币时间, head);
                            if (应答数据.IsSuccess)
                            {
                                Ask.Ask远程投币上分数据 ask = new Ask.Ask远程投币上分数据(目标机头地址, 投币量, IC卡号码 != "" ? "刷卡" : "硬币", 流水号);
                            }
                            TerminalDataProcess.InsertRepeat(f.routeAddress, 投币机地址, IC卡号码, CommandType.投币机投币指令, CommandType.投币机投币指令应答, 应答数据, 流水号, RecvDateTime);
                        }
                        else if (res == 1)
                        {
                            应答数据 = (Ask.Ask投币机投币指令应答)obj;
                            PubLib.当前IC卡进出币指令重复数++;
                        }
                        else
                        {
                            //重复性检查错误
                            return;
                        }

                        byte[] dataBuf = PubLib.GetBytesByObject(应答数据);
                        SendData = PubLib.GetFrameDataBytes(f, dataBuf, CommandType.投币机投币指令应答);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }

        public void AskData()
        {
            SendDataTime = DateTime.Now;
            if (SendData != null)
            {
                TerminalDataProcess.SendData(SendData);
                TerminalDataProcess.UpdateRepeatTime(PubLib.路由器段号, 投币机地址, 流水号);
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
            sb.AppendFormat("机头地址：{0}\r\n", 投币机地址);
            sb.AppendFormat("目标机头地址：{0}\r\n", 目标机头地址);
            sb.AppendFormat("投币时间：{0}\r\n", 投币时间.ToString());
            sb.AppendFormat("IC卡号：{0}\r\n", IC卡号码);
            sb.AppendFormat("币数：{0}\r\n", 投币量);
            sb.AppendFormat("动态密码：{0}\r\n", PubLib.Hex2String(动态密码));
            sb.AppendFormat("流水号：{0}\r\n", 流水号);
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
            sb.Append(PubLib.BytesToString(SendData) + Environment.NewLine);
            sb.AppendFormat("指令类别：{0}\r\n", RecvData.commandType);
            sb.AppendFormat("投币流水号：{0}\r\n", 应答数据.投币流水号);
            sb.AppendFormat("机头地址：{0}\r\n", PubLib.Hex2String(应答数据.机头地址));
            sb.AppendFormat("目标机头地址：{0}\r\n", 目标机头地址);
            sb.AppendFormat("投币时间：{0}\r\n", 投币时间.ToString());
            sb.AppendFormat("流水号：{0}\r\n", 应答数据.流水号);
            return sb.ToString();
        }
    }
}
