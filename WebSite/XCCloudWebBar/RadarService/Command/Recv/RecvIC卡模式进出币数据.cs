using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;

namespace RadarService.Command.Recv
{
    public class RecvIC卡模式进出币数据
    {
        public string 机头地址 = "";
        public string IC卡号码 = "";
        public byte 动态密码 = 0;
        public CoinType 控制类型;
        public int 币数;
        public UInt16 流水号;
        public byte[] SendData = null;
        public FrameData RecvData;
        public Ask.AskIC卡模式进出币数据 应答数据;
        public UInt16 测试数据;
        public DateTime SendDataTime;
        public bool 高速投币标志 = false;
        public string 投币目标地址 = "";

        public RecvIC卡模式进出币数据(FrameData f, DateTime RecvDateTime, EndPoint p)
        {
            try
            {
                RecvData = f;
                if (f.commandData.Length >= 15)
                {
                    机头地址 = Coding.ConvertData.Hex2String(f.commandData[0]);
                    for (int i = 0; i < 8; i++)
                    {
                        if (f.commandData[i + 1] < 0x30 || f.commandData[i + 1] > 0x39)
                        {
                            f.commandData[i + 1] = 32;
                        }
                    }
                    IC卡号码 = Encoding.ASCII.GetString(f.commandData, 1, 8);

                    IC卡号码 = IC卡号码.Trim();

                    动态密码 = f.commandData[9];
                    控制类型 = (CoinType)f.commandData[10];
                    币数 = (int)BitConverter.ToUInt16(f.commandData, 11);
                    测试数据 = BitConverter.ToUInt16(f.commandData, 13);
                    流水号 = BitConverter.ToUInt16(f.commandData, 15);

                    高速投币标志 = (测试数据 / 256 == 1);
                    投币目标地址 = Coding.ConvertData.Hex2String((byte)(测试数据 % 256));



                    //Console.WriteLine("高速投币标志：" + 高速投币标志 + "  投币目标地址：" + 投币目标地址);
                    Info.DeviceInfo.机头信息 head = Info.DeviceInfo.GetBufMCUIDDeviceInfo(f.routeAddress, 机头地址);
                    if (head == null) return;
                    if (IC卡号码 == "") IC卡号码 = head.当前卡片号;
                    if (IC卡号码 == "") IC卡号码 = "0";
                    if (控制类型 == CoinType.实物投币) IC卡号码 = "0"; //实物投币过滤卡号，有可能是卡头没有清缓存导致
                    head.临时错误计数 = 测试数据;

                    head.状态.出币机或存币机正在数币 = false;
                    //FrmMain.GetInterface.ChangeDeviceStatus(head.常规.机头长地址, XCSocketService.DeviceStatusEnum.在线);

                    //if (head.是否从雷达获取到状态)
                    {
                        object obj = null;

                        int res = HostServer.CheckRepeat(f.routeAddress, 机头地址, IC卡号码, CommandType.IC卡模式投币数据, ref obj, 流水号);
                        if (res == 0)
                        {
                            if (高速投币标志)
                            {
                                //Console.WriteLine("远程投币");
                                Ask.Ask远程投币上分数据 ask = new Ask.Ask远程投币上分数据(head, 币数, "刷卡", 流水号, false, p);
                            }
                            string msg = "";
                            应答数据 = new Ask.AskIC卡模式进出币数据(head, IC卡号码, 币数, 控制类型, 动态密码, (UInt16)流水号, f, 高速投币标志, 投币目标地址, p, ref msg);
                            if (f.commandType == CommandType.IC卡模式投币数据)
                            {
                                HostServer.InsertRepeat(f.routeAddress, 机头地址, IC卡号码, CommandType.IC卡模式投币数据, CommandType.IC卡模式投币数据应答, 应答数据, 流水号, RecvDateTime);
                                if (应答数据.脉冲数 == 0)
                                {
                                    LogHelper.LogHelper.WriteLog("IC卡进出币数据有误\r\n" + msg, f.commandData);
                                }
                            }
                            else
                                HostServer.InsertRepeat(f.routeAddress, 机头地址, IC卡号码, CommandType.IC卡模式退币数据, CommandType.IC卡模式退币数据应答, 应答数据, 流水号, RecvDateTime);
                        }
                        else if (res == 1)
                        {
                            应答数据 = (Ask.AskIC卡模式进出币数据)obj;
                            HostServer.当前IC卡进出币指令重复数++;
                        }
                        else
                        {
                            //重复性检查错误
                            return;
                        }
                        byte[] dataBuf = Coding.ConvertData.GetBytesByObject(应答数据);
                        if (f.commandType == CommandType.IC卡模式投币数据)
                            SendData = Coding.ConvertData.GetFrameDataBytes(f, dataBuf, CommandType.IC卡模式投币数据应答);
                        else
                        {
                            SendData = Coding.ConvertData.GetFrameDataBytes(f, dataBuf, CommandType.IC卡模式退币数据应答);
                            LogHelper.LogHelper.WriteTBLog(Coding.ConvertData.BytesToString(f.recvData));
                        }
                    }
                }
            }
            catch
            {
                //LogHelper.WriteLog("IC卡进出币数据有误", f.commandData);
                throw;
            }
        }

        //public void AskData()
        //{
        //    SendDataTime = DateTime.Now;
        //    if (SendData != null)
        //    {
        //        TerminalDataProcess.SendData(SendData);
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
            sb.AppendFormat("控制类型：{0}\r\n", 控制类型);
            sb.AppendFormat("IC卡号：{0}\r\n", IC卡号码);
            sb.AppendFormat("币数：{0}\r\n", 币数);
            sb.AppendFormat("动态密码：{0}\r\n", Coding.ConvertData.Hex2String(动态密码));
            sb.AppendFormat("流水号：{0}\r\n", 流水号);
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
            sb.Append(Coding.ConvertData.BytesToString(SendData) + Environment.NewLine);
            sb.AppendFormat("机头地址：{0}\r\n", Coding.ConvertData.Hex2String(应答数据.机头地址));
            sb.AppendFormat("控制类型：{0}\r\n", 应答数据.控制类型);
            sb.AppendFormat("脉冲数：{0}\r\n", 应答数据.脉冲数);
            sb.AppendFormat("币余额：{0}\r\n", 应答数据.币余额);
            sb.AppendFormat("控制信号：{0}\r\n", Coding.ConvertData.Hex2BitString(应答数据.控制信号));
            sb.AppendFormat("流水号：{0}\r\n", 应答数据.流水号);
            return sb.ToString();
        }
    }
}
