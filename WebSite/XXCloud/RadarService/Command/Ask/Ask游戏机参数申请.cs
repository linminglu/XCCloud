using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using RadarService.Info;


namespace RadarService.Command.Ask
{
    [Serializable]
    public class 游戏机维修开关信号1
    {
        public bool bit15保留 { get; set; }
        public bool bit14保留 { get; set; }
        public bool bit13保留 { get; set; }
        public bool bit12保留 { get; set; }
        public bool bit11保留 { get; set; }
        public bool bit10启用第二路上分信号 { get; set; }
        public bool bit9数币脉冲电平 { get; set; }
        public bool bit8SSR退币驱动脉冲电平 { get; set; }
        public bool bit7第二路上分线上分电平 { get; set; }
        public bool bit6投币脉冲电平 { get; set; }
        public bool bit5硬件投币控制 { get; set; }
        public bool bit4转发实物投币 { get; set; }
        public bool bit3允许实物退币 { get; set; }
        public bool bit2退分方式 { get; set; }
        public bool bit1允许电子投币 { get; set; }
        public bool bit0允许电子退币或允许打票 { get; set; }
    }
    [Serializable]
    public class 游戏机维修开关信号2
    {
        public bool bit15保留 { get; set; }
        public bool bit14小票是否打印二维码 { get; set; }
        public bool bit13启用彩票模式 { get; set; }
        public bool bit12只退实物彩票 { get; set; }
        public bool bit11启用防霸位功能 { get; set; }
        public bool bit10启用刷卡版彩票功能 { get; set; }
        public bool bit9启用刷卡即扣 { get; set; }
        public bool bit8启用增强防止转卡 { get; set; }
        public bool bit7启用回路报警检测 { get; set; }
        public bool bit6启用外部报警检测 { get; set; }
        public bool bit5启用即中即退模式 { get; set; }
        public bool bit4启用异常退币检测 { get; set; }
        public bool bit3退分锁定标志 { get; set; }
        public bool bit2BO按钮是否维持 { get; set; }
        public bool bit1启用专卡专用 { get; set; }
        public bool bit0退币超限标志 { get; set; }
    }
    [Serializable]
    public class Ask游戏机参数申请
    {
        public byte 机头地址 { get; set; }
        public UInt16 单次退币限额 { get; set; }
        public byte 按键一扣卡里币数 { get; set; }
        public byte 按键二扣卡里币数 { get; set; }
        public byte 退币时给游戏机脉冲数比例因子 { get; set; }
        public byte 退币时卡上增加比例因子 { get; set; }
        public byte 刷卡即扣延时秒数 { get; set; }
        public UInt16 退币按钮脉宽 { get; set; }
        public string 本店卡校验密码 { get; set; }
        public UInt16 开关1 { get; set; }
        public UInt16 开关2 { get; set; }
        public byte 首次投币启动间隔 { get; set; }
        public UInt16 投币速度 { get; set; }
        public byte 投币脉宽 { get; set; }
        public byte 第二路上分线首次上分启动间隔 { get; set; }
        public UInt16 第二路上分线上分速度 { get; set; }
        public byte 第二路上分线上分脉宽 { get; set; }
        public UInt16 退币速度 { get; set; }
        public byte 退币脉宽 { get; set; }
        public byte 异常检测时间 { get; set; }
        public byte 异常检测次数 { get; set; }
        public byte 测试间隔时间 { get; set; }
        public byte 有效期天数 { get; set; }

        public Ask游戏机参数申请(DeviceInfo.机头信息 head)
        {
            try
            {
                DataAccess ac = new DataAccess();
                DataTable dt = ac.ExecuteQuery(string.Format("select * from Data_GameInfo where ID='{0}'", head.游戏机索引号)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    机头地址 = Convert.ToByte(head.机头短地址, 16);
                    有效期天数 = (byte)PublicHelper.SystemDefiner.ElecTicketValidDay;
                    单次退币限额 = Convert.ToUInt16(row["OnceOutLimit"]);
                    按键一扣卡里币数 = Convert.ToByte(row["PushCoin1"]);
                    按键二扣卡里币数 = Convert.ToByte(row["PushCoin2"]);
                    退币时给游戏机脉冲数比例因子 = Convert.ToByte(row["OutReduceFromGame"]);
                    退币时卡上增加比例因子 = Convert.ToByte(row["OutAddToCard"]);
                    刷卡即扣延时秒数 = Convert.ToByte(row["ReadDelay"]);
                    退币按钮脉宽 = Convert.ToUInt16(row["BOPulse"]);
                    本店卡校验密码 = PublicHelper.SystemDefiner.StorePassword;
                    游戏机维修开关信号1 开关信号1 = new 游戏机维修开关信号1();
                    开关信号1.bit0允许电子退币或允许打票 = Convert.ToBoolean(row["AllowElecOut"]);
                    开关信号1.bit1允许电子投币 = Convert.ToBoolean(row["AllowElecPush"]);
                    开关信号1.bit2退分方式 = Convert.ToBoolean(row["OutMode"]);
                    开关信号1.bit3允许实物退币 = Convert.ToBoolean(row["AllowRealOut"]);
                    开关信号1.bit4转发实物投币 = Convert.ToBoolean(row["AllowRealPush"]);
                    开关信号1.bit5硬件投币控制 = false;// Convert.ToBoolean(row["PushControl"]);//已取消
                    开关信号1.bit6投币脉冲电平 = Convert.ToBoolean(row["PushLevel"]);
                    开关信号1.bit7第二路上分线上分电平 = Convert.ToBoolean(row["SecondLevel"]);
                    开关信号1.bit8SSR退币驱动脉冲电平 = Convert.ToBoolean(row["OutLevel"]);
                    开关信号1.bit9数币脉冲电平 = Convert.ToBoolean(row["CountLevel"]);
                    开关信号1.bit10启用第二路上分信号 = Convert.ToBoolean(row["UseSecondPush"]);
                    开关1 = Coding.ConvertData.GetBit16ByObject(开关信号1);
                    游戏机维修开关信号2 开关信号2 = new 游戏机维修开关信号2();
                    开关信号2.bit0退币超限标志 = ((head.投币.盈利数 > head.投币.每天净退币上限) && !head.状态.是否忽略超分报警);
                    开关信号2.bit1启用专卡专用 = Convert.ToBoolean(row["GuardConvertCard"]);
                    开关信号2.bit2BO按钮是否维持 = Convert.ToBoolean(row["BOKeep"]);
                    开关信号2.bit3退分锁定标志 = Convert.ToBoolean(row["BOLock"]);
                    开关信号2.bit4启用异常退币检测 = Convert.ToBoolean(row["ExceptOutTest"]);
                    开关信号2.bit5启用即中即退模式 = Convert.ToBoolean(row["NowExit"]);
                    开关信号2.bit6启用外部报警检测 = Convert.ToBoolean(row["OutsideAlertCheck"]);
                    开关信号2.bit7启用回路报警检测 = Convert.ToBoolean(row["ReturnCheck"]);
                    开关信号2.bit8启用增强防止转卡 = Convert.ToBoolean(row["StrongGuardConvertCard"]);
                    开关信号2.bit9启用刷卡即扣 = Convert.ToBoolean(row["ReadCat"]);
                    开关信号2.bit10启用刷卡版彩票功能 = Convert.ToBoolean(row["ICTicketOperation"]);
                    开关信号2.bit11启用防霸位功能 = Convert.ToBoolean(row["BanOccupy"]);
                    开关信号2.bit12只退实物彩票 = Convert.ToBoolean(row["OnlyExitLottery"]);
                    开关信号2.bit13启用彩票模式 = head.彩票模式;
                    开关信号2.bit14小票是否打印二维码 = PublicHelper.SystemDefiner.PrintBarcode;
                    开关2 = Coding.ConvertData.GetBit16ByObject(开关信号2);
                    首次投币启动间隔 = Convert.ToByte(row["PushStartInterval"]);
                    投币速度 = Convert.ToUInt16(row["PushSpeed"]);
                    投币脉宽 = Convert.ToByte(row["PushPulse"]);
                    第二路上分线首次上分启动间隔 = Convert.ToByte(row["SecondStartInterval"]);
                    第二路上分线上分速度 = Convert.ToUInt16(row["SecondSpeed"]);
                    第二路上分线上分脉宽 = Convert.ToByte(row["SecondPulse"]);
                    退币速度 = Convert.ToUInt16(row["OutSpeed"]);
                    退币脉宽 = Convert.ToByte(row["OutPulse"]);
                    异常检测时间 = Convert.ToByte(row["ExceptOutSpeed"]);
                    异常检测次数 = Convert.ToByte(row["Frequency"]);
                    测试间隔时间 = (byte)PublicHelper.SystemDefiner.测试间隔时间;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
