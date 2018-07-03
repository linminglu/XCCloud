using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask投币机投币指令应答
    {
        public byte 机头地址 { get; set; }
        public UInt32 投币流水号 { get; set; }
        public DateTime 投币时间 { get; set; }
        public byte 是否打印 { get; set; }
        public UInt16 保留字节 { get; set; }
        public UInt16 流水号 { get; set; }

        public bool IsSuccess = false;

        public Ask投币机投币指令应答(string rAddress, string hAddress, string pAddress, int pushType, string ICCardID, int PWD, int Coins, UInt16 SN, DateTime opTime, Info.HeadInfo.机头信息 head)
        {
            int 币余额 = 0, 新币余额 = 0;

            流水号 = SN;            
            机头地址 = Convert.ToByte(hAddress, 16);
            投币时间 = opTime;

            if (pushType == 2)
            {
                //刷卡投币
                DataTable dt = DataAccess.ExecuteQueryReturnTable(string.Format("SELECT m.ICCardID, m.MemberLevelID, m.`Lock`, m.Balance, m.Lottery, m.RepeatCode, l.AllowExitCoinToCard, l.LockHead from t_member m, t_memberlevel l where m.MemberLevelID=l.MemberLevelID and m.ICCardID='{0}'", ICCardID, PWD));
                if (dt.Rows.Count > 0)
                {
                    int balance = Convert.ToInt32(dt.Rows[0]["Balance"].ToString());
                    if (balance < Coins) return;
                    币余额 = Convert.ToInt32(dt.Rows[0]["Balance"].ToString());
                    新币余额 = 币余额 - Coins;
                    if (新币余额 < 0)
                        新币余额 = 0;

                    DBUpdate.AddList(string.Format("update t_member set Balance={0} where ICCardID='{1}';", 新币余额, ICCardID));
                }
            }

            是否打印 = 0x01;
            
            TableMemory.flw_485_coin.Table t = new TableMemory.flw_485_coin.Table();
            if (pushType == 1)
            {
                t.ICCardID = (99990000 + Convert.ToInt32(hAddress, 16)).ToString();
                t.HeadAddress = pAddress;
                t.Balance = 0;
            }
            else
            {
                t.ICCardID = ICCardID;
                t.HeadAddress = pAddress;
                t.Balance = 新币余额;
            }
            t.Coins = Coins;
            t.CoinType = (pushType == 1 ? "0" : "1");//实物投币

            t.RealTime = opTime;
            t.Segment = rAddress;
            投币流水号 = TableMemory.flw_485_coin.Add(t);

            //创建投币机流水
            TableMemory.flw_push_coin.Table f = new TableMemory.flw_push_coin.Table();
            f.FlwID = 投币流水号;
            f.HeadAddress = hAddress;
            f.DeviceName = head.常规.设备名称;
            TableMemory.flw_push_coin.Add(f);

            IsSuccess = true;
        }
    }
}
