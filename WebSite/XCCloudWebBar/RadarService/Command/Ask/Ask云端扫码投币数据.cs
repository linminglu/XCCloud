using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.Command.Ask
{
    public class Ask云端扫码投币数据
    {
        public bool IsSuccess = false;
        public string Message = "";

        /// <summary>
        /// 云端扫码投币，包含散客现金投币，会员扫码投币规则，需要写本地数据库
        /// 散客现金投币
        /// </summary>
        /// <param name="head"></param>
        /// <param name="RuleType"></param>
        /// <param name="RuleID"></param>
        /// <param name="RuleCount"></param>
        /// <param name="OrderID">订单号</param>
        public Ask云端扫码投币数据(Info.DeviceInfo.机头信息 head, int RuleType, int RuleID, int RuleCount, string OrderID, bool 专卡专用, int PushSN, EndPoint p)
        {
            try
            {
                if (head != null)
                {
                    //游戏启动次数
                    int gameTimes = 0;
                    //单次游戏启动脉冲数
                    int pulsCount = 0;
                    //投币类别
                    string pushType = "";
                    Info.GameInfo.游戏机信息 game = Info.GameInfo.GetGameInfo(head.游戏机索引号);
                    //获取当前游戏机单局脉冲数
                    pulsCount = (game.投币参数.启用上分通道 ? game.投币参数.按钮2上分脉冲数 : game.投币参数.按钮1上分脉冲数);
                    DSS.DataModel model = new DSS.DataModel();
                    //获取投币规则启动游戏次数
                    if (RuleType == 1)
                    {
                        //散客投币
                        pushType = "散客现金投币";
                        object o = new DSS.Table.Data_GameAPP_Rule();
                        if (model.CovertToDataModel("select * from Data_GameAPP_Rule where id='" + RuleID + "'", ref o))
                        {
                            DSS.Table.Data_GameAPP_Rule r = (DSS.Table.Data_GameAPP_Rule)o;
                            gameTimes = (int)r.PlayCount * RuleCount;
                        }
                    }
                    else if (RuleType == 2)
                    {
                        //会员投币
                        pushType = "会员云投币";
                        object o = new DSS.Table.Data_GameAPP_MemberRule();
                        if (model.CovertToDataModel("select * from Data_GameAPP_MemberRule where id='" + RuleID + "'", ref o))
                        {
                            DSS.Table.Data_GameAPP_Rule r = (DSS.Table.Data_GameAPP_Rule)o;
                            gameTimes = (int)r.PlayCount * RuleCount;
                        }
                    }
                    pulsCount = pulsCount * gameTimes;

                    FrameData data = new FrameData();
                    data.commandType = CommandType.远程投币上分指令;
                    data.routeAddress = head.路由器段号;
                    List<byte> dataList = new List<byte>();
                    dataList.Add((byte)Convert.ToByte(head.机头短地址, 16));
                    dataList.AddRange(BitConverter.GetBytes((UInt16)pulsCount));
                    dataList.Add((byte)(专卡专用 ? 7 : 0));
                    dataList.AddRange(BitConverter.GetBytes((UInt16)PushSN));
                    byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, dataList.ToArray(), CommandType.远程投币上分指令);

                    StringBuilder sb = new StringBuilder();
                    sb.Append(Coding.ConvertData.BytesToString(Send) + Environment.NewLine);
                    sb.AppendFormat("指令类别：{0}\r\n", CommandType.远程投币上分指令);
                    sb.AppendFormat("机头地址：{0}\r\n", head.机头短地址);
                    sb.AppendFormat("脉冲数：{0}\r\n", pulsCount);
                    sb.AppendFormat("投币类别：{0}\r\n", pushType);
                    sb.AppendFormat("流水号：{0}\r\n", PushSN);
                    sb.AppendFormat("订单号：{0}\r\n", OrderID);
                    Console.WriteLine(sb.ToString());

                    HostServer.BodySend(null, Send, data.commandType, CommandType.远程投币上分指令应答, sb.ToString(), PushSN, head.机头短地址, p, head.路由器段号);

                    LogHelper.LogHelper.WritePush(Send, head.路由器段号, head.机头短地址, pushType, pulsCount.ToString(), PushSN.ToString());
                    IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                LogHelper.LogHelper.WriteLog(e);
            }
        }
    }
}
