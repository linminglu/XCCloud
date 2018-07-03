using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask电子币模式投币数据
    {
        public byte 机头地址 { get; set; }
        public UInt16 币数 { get; set; }
        public UInt16 脉冲数 { get; set; }
        public byte 控制信号 { get; set; }
        public UInt16 流水号 { get; set; }

        public int 扣币数 = 0;
        public string 数字币编号 = "0";

        public Ask电子币模式投币数据(string rAddress, string hAddress, UInt32 UID, UInt16 SN)
        {
            try
            {
                控制信号结构 信号 = new 控制信号结构();
                机头地址 = Convert.ToByte(hAddress, 16);
                //IC卡号码 = IC;
                //动态密码 = checkCode;
                流水号 = SN;

                #region 内存查询方法
                DataMember.电子币投币应答结构 投币 = Info.CoinInfo.电子币投币(UID, PubLib.路由器段号, hAddress);
                if (投币 != null)
                {
                    信号.机头上下分 = 投币.机头能上分;
                    信号.锁定游戏机头 = 投币.锁机头;
                    信号.允许从游戏机退币到卡里 = 投币.机头能打票;
                    币数 = (UInt16)投币.币数;
                    扣币数 = 投币.扣币数;
                    脉冲数 = (UInt16)投币.发脉冲数;
                    数字币编号 = 投币.数字币编号;
                }
                #endregion

                控制信号 = PubLib.GetBitByObject(信号);
            }
            catch 
            {
                throw;
            }
        }
    }
}
