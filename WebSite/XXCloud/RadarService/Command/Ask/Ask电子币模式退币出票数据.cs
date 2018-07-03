using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask电子币模式退币出票数据
    {
        public byte 机头地址 { get; set; }
        public UInt32 条码号 { get; set; }
        public byte 动态密码 { get; set; }
        public UInt16 退币数 { get; set; }
        public UInt16 脉冲数 { get; set; }
        public byte 控制信号 { get; set; }
        public DateTime 打印时间 { get; set; }
        public UInt16 流水号 { get; set; }

        public Ask电子币模式退币出票数据(string rAddress, string hAddress, int Coins, UInt16 SN, DateTime opTime)
        {
            控制信号结构 信号 = new 控制信号结构();
            机头地址 = Convert.ToByte(hAddress, 16);
            退币数 = (UInt16)Coins;
            流水号 = SN;
            打印时间 = opTime;

            DataMember.电子币出币应答结构 退币 = Info.CoinInfo.电子币出币(Coins, rAddress, hAddress, opTime);
            if (退币 != null)
            {
                信号.机头上下分 = 退币.机头能上分;
                信号.锁定游戏机头 = 退币.锁机头;
                信号.允许从游戏机退币到卡里 = 退币.机头能打票;
                动态密码 = 退币.动态密码;
                条码号 = 退币.条码;
                信号.每天净退币上限 = 退币.超额锁;
                打印时间 = opTime;
                脉冲数 = Convert.ToUInt16(退币.发脉冲数);
                信号.是否将退币上回游戏机 = 退币.是否将退币上回游戏机;
                信号.是否正在使用限时送分优惠券 = 退币.是否正在使用限时送分优惠券;
            }
            控制信号 = PubLib.GetBitByObject(信号);
        }
    }
}
