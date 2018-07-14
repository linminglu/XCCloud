using DSS;
using DSS.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCCloudSerialNo;

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
        public Ask液晶卡头进出币指令() { }

        public Ask液晶卡头进出币指令(Info.DeviceInfo.机头信息 head, Info.CoinInfo.会员卡信息 member, Info.GameInfo.游戏机信息 game, byte UseType, int Coins, CoinType cType, UInt16 SN, FrameData frame, bool isPush, string PushAddr, ref string msg)
        {
            扣币类型 = UseType;
            try
            {
                int FreeCoin = 0;
                int PushIndex = 0;
                IC卡进出币控制信号结构 信号 = new IC卡进出币控制信号结构();
                机头地址 = Convert.ToByte(head.机头短地址, 16);
                流水号 = SN;
                控制类型 = (byte)cType;

                #region 内存查询方法
                DataMember.液晶卡头进出币应答结构 投币 = Info.CoinInfo.液晶卡头进出币(head, member, game, Coins, UseType, SN, cType, ref msg, isPush, PushAddr, out FreeCoin, out PushIndex);
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
                    控制信号 = Coding.ConvertData.GetBitByObject(信号);

                    Flw_DeviceData data = new Flw_DeviceData();
                    data.ID = SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                    data.MerchID = PublicHelper.SystemDefiner.MerchID;
                    data.StoreID = PublicHelper.SystemDefiner.StoreID;
                    data.DeviceID = head.设备编号;
                    data.GameIndexID = head.游戏机索引号;
                    data.SiteName = head.位置名称;
                    data.SN = SN;
                    data.State = 1;
                    data.ACKControlValue = 控制信号;
                    data.ACKFreeCoin = 送币数;
                    data.ACKPulesCount = 脉冲数;
                    data.CreateStoreID = PublicHelper.SystemDefiner.StoreID;
                    data.BusinessType = (int)cType;
                    data.RealTime = DateTime.Now;
                    data.MemberID = member.会员编号;
                    data.MemberName = member.会员姓名;
                    data.ICCardID = member.会员卡号;
                    data.Coin = Coins;
                    data.OrderID = data.ID;
                    data.Note = "投币 " + game.通用参数.游戏机名 + " | " + head.位置名称;
                    data.CheckDate = Convert.ToDateTime(SerialNoHelper.StringGet("营业日期"));
                    if (cType == CoinType.电子退币)
                        data.BalanceIndex = game.退币参数.退币余额类别;
                    else if (cType == CoinType.电子投币)
                        data.BalanceIndex = PushIndex;
                    data.RemainBalance = 币余额;
                    DataModel model = new DataModel();
                    data.Verifiction = model.Verifiction(data,PublicHelper.SystemDefiner.AppSecret);
                    model.Add(data);
                }
                else
                {
                    LogHelper.LogHelper.WriteLog("液晶卡头进出币数据有误\r\n" + msg, frame.recvData);
                }
                #endregion


            }
            catch
            {
                //LogHelper.WriteLog("IC卡进出币数据有误\r\n" + msg, frame.recvData);
                throw;
            }
        }
    }
}
