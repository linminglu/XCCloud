using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.Command.Recv;
using System.Data;
using System.Net;
using DSS;

namespace RadarService.Command.Ask
{
    public class Ask液晶卡头扩展信息下发指令
    {
        const int PacketLEN = 40;

        public Ask液晶卡头扩展信息下发指令(Info.DeviceInfo.机头信息 head, 参数类别 参数, EndPoint remoteP)
        {
            if (head != null)
            {
                //if (head.状态.在线状态)
                {
                    switch (参数)
                    {
                        case 参数类别.机头二维码:
                            string code = GetQRCode(head);
                            if (code != "")
                            {
                                SendParamet(参数类别.机头二维码, code, head, remoteP);
                            }
                            break;
                        case 参数类别.出票器网关规则:
                            SendGateway(head, 参数类别.出票器网关规则, remoteP);
                            break;
                        case 参数类别.显示参数:
                            SendShowParamet(head, remoteP);
                            break;
                    }
                }
            }
        }

        public void SendShowParamet(Info.DeviceInfo.机头信息 head, EndPoint remoteP)
        {
            Info.GameInfo.游戏机信息 game = Info.GameInfo.GetGameInfo(head.游戏机索引号);
            if (game != null)
            {
                Info.GameInfo.显示参数 p = new Info.GameInfo.显示参数();
                p.GameName = game.通用参数.游戏机名 + "  " + head.位置名称;
                p.OutName = Info.CoinInfo.GetBalanceName(game.退币参数.退币余额类别);
                p.PushName1 = Info.CoinInfo.GetBalanceName(game.投币参数.按钮1余额类别);
                p.PushName2 = Info.CoinInfo.GetBalanceName(game.投币参数.按钮2余额类别);
                p.SiteName = head.位置名称;
                p.StoreName = PublicHelper.SystemDefiner.StoreName;

                byte[] vdata = Coding.ConvertData.GetHeadParamet(p);
                int pCount = 0, pRemain = 0;
                pCount = vdata.Length / PacketLEN;
                pRemain = vdata.Length % PacketLEN;

                if (pRemain > 0)
                    pCount++;

                for (int i = 0; i < pCount; i++)
                {
                    int SN = PublicHelper.SystemDefiner.雷达主发流水号;

                    FrameData data = new FrameData();
                    data.commandType = CommandType.液晶卡头扩展信息下发指令;
                    data.routeAddress = head.路由器段号;
                    List<byte> dataList = new List<byte>();
                    dataList.Add((byte)Convert.ToByte(head.机头短地址, 16));
                    dataList.Add(0x03);
                    dataList.Add((byte)i);
                    dataList.Add((byte)pCount);
                    dataList.AddRange(BitConverter.GetBytes((UInt16)SN));
                    int len = 0;
                    if (i < pCount - 1)
                        len = PacketLEN;
                    else
                    {
                        if (pRemain > 0)
                            len = pRemain;
                        else
                            len = PacketLEN;
                    }
                    dataList.Add((byte)len);
                    dataList.AddRange(vdata.Skip(i * PacketLEN).Take(len).ToArray());
                    byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, dataList.ToArray(), data.commandType);

                    StringBuilder sb = new StringBuilder();
                    sb.Append(Coding.ConvertData.BytesToString(Send) + Environment.NewLine);
                    sb.AppendFormat("指令类别：{0}\r\n", data.commandType);
                    sb.AppendFormat("机头地址：{0}\r\n", head.机头短地址);
                    sb.AppendFormat("参数类别：{0}\r\n", 0x03);
                    sb.AppendFormat("包数：{0}\r\n", pCount);
                    sb.AppendFormat("序号：{0}\r\n", i);
                    sb.AppendFormat("流水号：{0}\r\n", SN);

                    HostServer.BodySend(null, Send, data.commandType, CommandType.液晶卡头扩展信息下发应答指令, sb.ToString(), SN, head.机头短地址, remoteP);
                }
            }
        }

        void SendGateway(Info.DeviceInfo.机头信息 head, 参数类别 p, EndPoint remoteP)
        {
            int pCount = 0, pRemain = 0;
            string url = "";
            List<byte> sendList = new List<byte>();

            url = GetQRCode(head);
            sendList.Add(0x00); //固定值
            sendList.Add((byte)url.Length);
            sendList.AddRange(Encoding.ASCII.GetBytes(url));
            sendList.Add(0x03); //票号
            sendList.Add(0x00); //变量无长度

            byte[] vdata = sendList.ToArray();

            pCount = vdata.Length / PacketLEN;
            pRemain = vdata.Length % PacketLEN;

            if (pRemain > 0)
                pCount++;

            for (int i = 0; i < pCount; i++)
            {
                int SN = PublicHelper.SystemDefiner.雷达主发流水号;

                FrameData data = new FrameData();
                data.commandType = CommandType.液晶卡头扩展信息下发指令;
                data.routeAddress = head.路由器段号;
                List<byte> dataList = new List<byte>();
                dataList.Add((byte)Convert.ToByte(head.机头短地址, 16));
                dataList.Add((byte)p);
                dataList.Add((byte)i);
                dataList.Add((byte)pCount);
                dataList.AddRange(BitConverter.GetBytes((UInt16)SN));
                int len = 0;
                if (i < pCount - 1)
                    len = PacketLEN;
                else
                {
                    if (pRemain > 0)
                        len = pRemain;
                    else
                        len = PacketLEN;
                }
                dataList.Add((byte)len);
                dataList.AddRange(vdata.Skip(i * PacketLEN).Take(len).ToArray());
                byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, dataList.ToArray(), data.commandType);

                StringBuilder sb = new StringBuilder();
                sb.Append(Coding.ConvertData.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", data.commandType);
                sb.AppendFormat("机头地址：{0}\r\n", head.机头短地址);
                sb.AppendFormat("参数类别：{0}\r\n", p);
                sb.AppendFormat("包数：{0}\r\n", pCount);
                sb.AppendFormat("序号：{0}\r\n", i);
                sb.AppendFormat("流水号：{0}\r\n", SN);

                HostServer.BodySend(null, Send, data.commandType, CommandType.液晶卡头扩展信息下发应答指令, sb.ToString(), SN, head.机头短地址, remoteP);
            }
        }

        void SendParamet(参数类别 p, string v, Info.DeviceInfo.机头信息 head, EndPoint remoteP)
        {
            int pCount = 0, pRemain = 0;
            byte[] vdata = Encoding.ASCII.GetBytes(v);

            pCount = vdata.Length / PacketLEN;
            pRemain = vdata.Length % PacketLEN;

            if (pRemain > 0)
                pCount++;



            for (int i = 0; i < pCount; i++)
            {
                int SN = PublicHelper.SystemDefiner.雷达主发流水号;

                FrameData data = new FrameData();
                data.commandType = CommandType.液晶卡头扩展信息下发指令;
                data.routeAddress = head.路由器段号;
                List<byte> dataList = new List<byte>();
                dataList.Add((byte)Convert.ToByte(head.机头短地址, 16));
                dataList.Add((byte)p);
                dataList.Add((byte)i);
                dataList.Add((byte)pCount);
                dataList.AddRange(BitConverter.GetBytes((UInt16)SN));
                int len = 0;
                if (i < pCount - 1)
                    len = PacketLEN;
                else
                {
                    if (pRemain > 0)
                        len = pRemain;
                    else
                        len = PacketLEN;
                }
                dataList.Add((byte)len);
                dataList.AddRange(vdata.Skip(i * PacketLEN).Take(len).ToArray());
                byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, dataList.ToArray(), data.commandType);

                StringBuilder sb = new StringBuilder();
                sb.Append(Coding.ConvertData.BytesToString(Send) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", data.commandType);
                sb.AppendFormat("机头地址：{0}\r\n", head.机头短地址);
                sb.AppendFormat("参数类别：{0}\r\n", p);
                sb.AppendFormat("包数：{0}\r\n", pCount);
                sb.AppendFormat("序号：{0}\r\n", i);
                sb.AppendFormat("流水号：{0}\r\n", SN);

                HostServer.BodySend(null, Send, data.commandType, CommandType.液晶卡头扩展信息下发应答指令, sb.ToString(), SN, head.机头短地址, remoteP);
            }
        }

        string GetQRCode(Info.DeviceInfo.机头信息 head)
        {
            try
            {
                string sql = "select BarCode from Base_DeviceInfo where MCUID='" + head.设备序列号 + "'";
                DataAccess ac = new DataAccess();
                DataTable dt = ac.ExecuteQueryReturnTable(sql);
                if (dt.Rows.Count > 0)
                    return dt.Rows[0]["BarCode"].ToString();
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
            return "";
        }
    }
}
