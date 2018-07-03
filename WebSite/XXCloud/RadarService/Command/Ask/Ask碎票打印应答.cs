using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask碎票打印应答
    {
        public byte 机头地址 { get; set; }
        public byte[] 条码号 { get; set; }
        public UInt16 碎票数 { get; set; }
        public UInt16 票余额 { get; set; }
        public byte[] 用户编号 { get; set; }
        public DateTime 打印时间 { get; set; }
        public UInt16 流水号 { get; set; }

        public Ask碎票打印应答(string rAddress, string hAddress, int Lottery, UInt16 SN)
        {
            控制信号结构 信号 = new 控制信号结构();
            机头地址 = Convert.ToByte(hAddress, 16);
            碎票数 = (UInt16)Lottery;
            流水号 = SN;
            打印时间 = DateTime.Now;

            Info.HeadInfo.机头信息 head = Info.HeadInfo.GetHeadInfo(rAddress, hAddress);
            if (head != null)
            {
                Random r = new Random();
                string barcode = r.Next(1111, 9999).ToString();

                string sql = string.Format("insert into flw_lottery (WorkType,GameID,HeadID,Barcode,LotteryCount,State,PrintTime,RealTime,ICCardID,GoodsFlwID,ScheduleID,UserID,WorkStation) values ('0','{0}','{1}','{4}','{2}',1,null,'{3}','',0,'0','0','{1}'); \r\n", head.常规.游戏机编号, head.常规.机头编号, Lottery, 打印时间.ToString("yyyy-MM-dd HH:mm:ss.fff"), barcode);
                sql += "select u.UserID from t_workstation w,u_users u where w.GameAccount=u.RealName and game_online=1; \r\n";

                DataTable dt = DataAccess.ExecuteQueryReturnTable(sql);
                if (dt.Rows.Count > 0)
                {
                    string userID = dt.Rows[0]["UserID"].ToString();
                    if (userID.Length == 1)
                        userID = "0" + userID;      //不足2位补0
                    userID = userID.Substring(0, 2);//只取2位长度

                    用户编号 = Encoding.GetEncoding("gb2312").GetBytes(userID);
                    条码号 = Encoding.GetEncoding("gb2312").GetBytes(barcode);
                }
            }
        }
    }
}
