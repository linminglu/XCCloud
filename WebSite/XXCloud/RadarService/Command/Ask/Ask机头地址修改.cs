using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask机头地址修改
    {
        public string 机头地址 = "";
        public Ask机头地址修改(string hAddress, string mcuData)
        {
            try
            {
                DataTable dt = DataAccess.ExecuteQuery(string.Format("select * from t_head where mcuid='{0}'", mcuData)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    机头地址 = hAddress;
                    DataRow row = dt.Rows[0];
                    FrameData data = new FrameData();
                    data.frameType = FrameType.命令帧;
                    data.routeAddress = row["Segment"].ToString();
                    Ask.Ask机头地址动态分配 ask = new Command.Ask.Ask机头地址动态分配(data.routeAddress, mcuData, false);
                    ask.机头地址 = Convert.ToByte(hAddress, 16);
                    byte[] dataBuf = PubLib.GetBytesByObject(ask);
                    byte[] readyToSend = PubLib.GetFrameDataBytes(data, dataBuf, COMObject.CommandType.机头地址修改);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(PubLib.BytesToString(readyToSend) + Environment.NewLine);
                    sb.AppendFormat("指令类别：{0}\r\n", COMObject.CommandType.机头地址修改);
                    sb.AppendFormat("机头地址：{0}\r\n", hAddress);
                    TerminalDataProcess.BodySend(ask, readyToSend, data.commandType, COMObject.CommandType.机头地址修改应答, sb.ToString(), hAddress);
                }
            }
            catch 
            {
                throw;
            }
        }
    }
}
