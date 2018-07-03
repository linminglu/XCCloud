using System;
using System.Collections.Generic;
using System.Linq;
using RadarService.Info;
namespace RadarService.Command.Ask
{
    public class 控制信号结构
    {
        public bool 保留5 { get; set; }
        public bool 保留4 { get; set; }
        public bool 是否正在使用限时送分优惠券 { get; set; }
        public bool 是否将退币上回游戏机 { get; set; }
        public bool 每天净退币上限 { get; set; }
        public bool 锁定游戏机头 { get; set; }
        public bool 允许从游戏机退币到卡里 { get; set; }
        public bool 机头上下分 { get; set; }
    }
    public class IC卡进出币控制信号结构
    {
        public bool 保留7 { get; set; }
        public bool 保留6 { get; set; }
        public bool 保留5是否正在使用限时送分优惠券 { get; set; }
        public bool 保留4是否将退币上回游戏机 { get; set; }
        public bool 保留3超出当日机头最大净退币上线 { get; set; }
        public bool 保留2是否启用卡片专卡专用功能 { get; set; }
        public bool 保留1当前卡是否允许退分 { get; set; }
        public bool 保留0当前卡是否允许上分 { get; set; }
    }

    public class IC卡权限结构
    {
        public bool bit7超级卡 { get; set; }
        public bool bit6 { get; set; }
        public bool bit5 { get; set; }
        public bool bit4 { get; set; }
        public bool bit3 { get; set; }
        public bool bit2 { get; set; }
        public bool bit1退分锁定解锁 { get; set; }
        public bool bit0专卡专用解锁 { get; set; }
    }

    public class AskIC卡模式会员余额查询
    {
        public byte 机头地址 { get; set; }
        public byte 卡类别 { get; set; }
        public byte 卡权限 { get; set; }
        public byte 扣卡里币基数 { get; set; }
        public UInt32 当前卡余额 { get; set; }
        public byte 控制信号 { get; set; }
        public UInt16 流水号 { get; set; }

        public AskIC卡模式会员余额查询(Info.DeviceInfo.机头信息 head, string IC, byte checkCode, UInt16 SN)
        {
            try
            {
                IC卡进出币控制信号结构 信号 = new IC卡进出币控制信号结构();
                IC卡权限结构 权限 = new IC卡权限结构();
                机头地址 = Convert.ToByte(head.机头短地址, 16);
                bool isAllowOut, isAllowIn, isAllowZKZY;
                int pcoin;
                UInt32 balance, lottery, point;
                byte cardType;
                流水号 = SN;

                if (IC != "")
                {
                    CoinInfo.IC卡查询(IC, head, (int)checkCode, out balance, out pcoin, out isAllowIn, out isAllowOut, out isAllowZKZY, out cardType, out 权限);
                    卡类别 = cardType;
                    扣卡里币基数 = (byte)pcoin;
                    当前卡余额 = balance;
                    信号.保留0当前卡是否允许上分 = isAllowIn;
                    信号.保留1当前卡是否允许退分 = isAllowOut;
                    信号.保留2是否启用卡片专卡专用功能 = isAllowZKZY;
                    if (cardType == 2)
                    {
                        head.状态.锁定机头 = false;
                    }
                }
                卡权限 = Coding.ConvertData.GetBitByObject(权限);
                控制信号 = Coding.ConvertData.GetBitByObject(信号);
            }
            catch
            {
                throw;
            }
        }
    }
}
