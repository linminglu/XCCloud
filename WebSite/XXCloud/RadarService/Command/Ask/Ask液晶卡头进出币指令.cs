using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Ask
{
    public class Ask液晶卡头进出币指令
    {
        public byte 机头地址 { get; set; }
        public byte 控制类型 { get; set; }
        public byte 扣币类型 { get; set; }
        public UInt16 脉冲数 { get; set; }
        public UInt16 送币数 { get; set; }
        public UInt32 币余额 { get; set; }
        public byte 控制信号 { get; set; }
        public UInt16 流水号 { get; set; }

        public Ask液晶卡头进出币指令(Info.DeviceInfo.机头信息 head, byte UseType, string IC, int Coins, CoinType cType, byte checkCode, UInt16 SN, FrameData frame, bool isPush, string PushAddr, ref string msg)
        {
            扣币类型 = UseType;
            try
            {
                int FreeCoin = 0;
                IC卡进出币控制信号结构 信号 = new IC卡进出币控制信号结构();
                机头地址 = Convert.ToByte(head.机头短地址, 16);
                流水号 = SN;
                控制类型 = (byte)cType;

                #region 内存查询方法
                DataMember.液晶卡头进出币应答结构 投币 = Info.CoinInfo.液晶卡头进出币(head,IC, Coins,UseType, SN, cType, checkCode, ref msg, isPush, PushAddr, out FreeCoin);
                if (投币 != null)
                {
                    信号.保留0当前卡是否允许上分 = 投币.机头能上分;
                    信号.保留1当前卡是否允许退分 = 投币.机头能打票;
                    信号.保留2是否启用卡片专卡专用功能 = 投币.是否启用卡片专卡专用;
                    信号.保留3超出当日机头最大净退币上线 = 投币.超出当日机头最大净退币上线;
                    信号.保留4是否将退币上回游戏机 = 投币.是否将退币上回游戏机;
                    信号.保留5是否正在使用限时送分优惠券 = 投币.是否正在使用限时送分优惠券;
                    币余额 = (UInt32)投币.币余额;
                    脉冲数 = (UInt16)投币.发脉冲数;
                    送币数 = (UInt16)FreeCoin;
                }
                else
                {
                    LogHelper.LogHelper.WriteLog("液晶卡头进出币数据有误\r\n" + msg, frame.recvData);
                }
                #endregion

                控制信号 = Coding.ConvertData.GetBitByObject(信号);
            }
            catch
            {
                //LogHelper.WriteLog("IC卡进出币数据有误\r\n" + msg, frame.recvData);
                throw;
            }
        }
    }
}
