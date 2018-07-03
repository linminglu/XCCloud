using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask存币机参数修改
    {
        byte[] SendData;
        Ask.Ask存币机参数申请 应答数据;

        public Ask存币机参数修改(string hAddress)
        {
            try
            {
                DataTable dt = DataAccess.ExecuteQuery(string.Format("select * from t_device where address='{0}' and segment='{1}'", hAddress, PubLib.路由器段号)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    FrameData data = new FrameData();
                    data.frameType = FrameType.命令帧;
                    data.routeAddress = row["Segment"].ToString();
                    data.commandType = COMObject.CommandType.游戏机参数修改;
                    应答数据 = new Ask存币机参数申请(hAddress);
                    byte[] dataBuf = PubLib.GetBytesByObject(应答数据);
                    SendData = PubLib.GetFrameDataBytes(data, dataBuf, COMObject.CommandType.游戏机参数修改);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(PubLib.BytesToString(SendData) + Environment.NewLine);
                    sb.AppendFormat("机头地址：{0}\r\n", PubLib.Hex2String(应答数据.机头地址));
                    sb.AppendFormat("马达配置：{0}\r\n", PubLib.Hex2BitString(应答数据.马达配置));
                    sb.AppendFormat("马达1比例：{0}\r\n", 应答数据.马达1比例);
                    sb.AppendFormat("马达2比例：{0}\r\n", 应答数据.马达2比例);
                    sb.AppendFormat("存币箱最大存币数：{0}\r\n", 应答数据.存币箱最大存币数);
                    sb.AppendFormat("本店卡校验密码：{0}\r\n", 应答数据.本店卡校验密码);

                    TerminalDataProcess.BodySend(应答数据, SendData, data.commandType, COMObject.CommandType.游戏机参数修改应答, sb.ToString(), hAddress);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
