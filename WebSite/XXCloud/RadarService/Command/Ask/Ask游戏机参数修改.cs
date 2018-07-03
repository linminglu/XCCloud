using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask游戏机参数修改
    {
        byte[] SendData;
        Ask.Ask游戏机参数申请 应答数据;

        public Ask游戏机参数修改(string hAddress)
        {
            try
            {
                DataTable dt = DataAccess.ExecuteQuery(string.Format("select * from t_head h,t_game g where h.GameID=g.GameID and h.HeadAddress='{0}' and h.segment='{1}'", hAddress, PubLib.路由器段号)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    FrameData data = new FrameData();
                    data.frameType = FrameType.命令帧;
                    data.routeAddress = row["Segment"].ToString();
                    data.commandType = COMObject.CommandType.游戏机参数修改;
                    应答数据 = new Command.Ask.Ask游戏机参数申请(PubLib.路由器段号, hAddress);
                    byte[] dataBuf = PubLib.GetBytesByObject(应答数据);
                    SendData = PubLib.GetFrameDataBytes(data, dataBuf, COMObject.CommandType.游戏机参数修改);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(PubLib.BytesToString(SendData) + Environment.NewLine);
                    sb.AppendFormat("机头地址：{0}\r\n", PubLib.Hex2String(应答数据.机头地址));
                    sb.AppendFormat("单次退币限额：{0}\r\n", 应答数据.单次退币限额);
                    sb.AppendFormat("退币时接收游戏机数币数：{0}\r\n", 应答数据.退币时给游戏机脉冲数比例因子);
                    sb.AppendFormat("退币时卡上增加币数：{0}\r\n", 应答数据.退币时卡上增加币数比例因子);
                    sb.AppendFormat("本店卡校验密码：{0}\r\n", 应答数据.本店卡校验密码);
                    sb.AppendFormat("开关1：{0}\r\n", PubLib.Hex2BitString(应答数据.开关1));
                    sb.AppendFormat("开关2：{0}\r\n", PubLib.Hex2BitString(应答数据.开关2));
                    sb.AppendFormat("首次投币启动间隔：{0}\r\n", 应答数据.首次投币启动间隔);
                    sb.AppendFormat("退币速度：{0}\r\n", 应答数据.退币速度);
                    sb.AppendFormat("退币脉宽：{0}\r\n", 应答数据.退币脉宽);
                    sb.AppendFormat("投币速度：{0}\r\n", 应答数据.投币速度);
                    sb.AppendFormat("投币脉宽：{0}\r\n", 应答数据.投币脉宽);
                    sb.AppendFormat("第二路上分线上分脉宽：{0}\r\n", 应答数据.第二路上分线上分脉宽);
                    sb.AppendFormat("第二路上分线上分启动间隔：{0}\r\n", 应答数据.第二路上分线首次上分启动间隔);
                    sb.AppendFormat("第二路上分线上分速度：{0}\r\n", 应答数据.第二路上分线上分速度);
                    TerminalDataProcess.BodySend(应答数据, SendData, data.commandType, COMObject.CommandType.游戏机参数修改应答, sb.ToString(), hAddress);
                }
            }
            catch 
            {
                throw;
            }
        }

        public string GetSendData(DateTime printDate)
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }
    }
}
