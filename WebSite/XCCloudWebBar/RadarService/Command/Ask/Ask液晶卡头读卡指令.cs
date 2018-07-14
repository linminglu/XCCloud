using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.Info;

namespace RadarService.Command.Ask
{
    public class Ask液晶卡头读卡指令
    {
        public byte 机头地址 { get; set; }
        public byte 卡类别 { get; set; }
        public byte 卡权限 { get; set; }
        public byte 按钮一扣卡里币数基数 { get; set; }
        public UInt32 按钮一扣值余额 { get; set; }
        public byte 按钮二扣卡里币数基数 { get; set; }
        public UInt32 按钮二扣值余额 { get; set; }
        public byte 显示方案索引 { get; set; }
        public byte 门票剩余次数 { get; set; }
        public byte[] 门票名 { get; set; }
        public byte[] 有效期 { get; set; }
        public byte 控制信号 { get; set; }
        public UInt16 流水号 { get; set; }

        public Ask液晶卡头读卡指令(Info.DeviceInfo.机头信息 head, string IC, byte checkCode, UInt16 SN)
        {
            try
            {
                IC卡进出币控制信号结构 信号 = new IC卡进出币控制信号结构();
                IC卡权限结构 权限 = new IC卡权限结构();
                机头地址 = Convert.ToByte(head.机头短地址, 16);
                bool isAllowOut, isAllowIn, isAllowZKZY;
                int Coin1, Balance1, Coin2, Balance2;
                byte cardType;
                流水号 = SN;
                门票名 = new byte[20];
                有效期 = new byte[3];

                if (IC != "")
                {
                    string tn = "";
                    int c = 0;
                    DateTime d;
                    CoinInfo.液晶卡头查询(IC, head, (int)checkCode, out Coin1, out Balance1, out Coin2, out Balance2, out isAllowIn, out isAllowOut, out isAllowZKZY, out cardType, out tn, out c, out d);
                    byte[] b = Encoding.GetEncoding("gb2312").GetBytes(tn);
                    int len = 0;
                    if (b.Length > 20)
                        len = 20;
                    else
                        len = b.Length;
                    Array.Copy(b, 门票名, len);
                    if (tn != "") 显示方案索引 = 0x01;
                    if (c == 0) 显示方案索引 = 0x00;
                    有效期[0] = (byte)(d.Year - 2000);
                    有效期[1] = (byte)(d.Month);
                    有效期[2] = (byte)(d.Day);
                    卡类别 = cardType;
                    按钮一扣卡里币数基数 = (byte)Coin1;
                    按钮一扣值余额 = (UInt32)Balance1;
                    按钮二扣卡里币数基数 = (byte)Coin2;
                    按钮二扣值余额 = (UInt32)Balance2;
                    信号.保留0当前卡是否允许上分 = isAllowIn;
                    信号.保留1当前卡是否允许退分 = isAllowOut;
                    信号.保留2是否启用卡片专卡专用功能 = isAllowZKZY;
                    门票剩余次数 = (byte)c;
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
