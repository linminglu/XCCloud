using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Recv
{
    public class Recv游戏机参数查询应答
    {
        public string 机头地址 = "";
        public Recv游戏机参数查询应答(FrameData data)
        {
            try
            {
                机头地址 = PubLib.Hex2String(data.commandData[0]);
                int 当前扣币基数 = 0;
                Info.HeadInfo.机头信息 机头 = Info.HeadInfo.GetHeadInfo(PubLib.路由器段号, 机头地址);
                机头.参数.单次退币限额 = BitConverter.ToUInt16(data.commandData, 1);
                当前扣币基数 = data.commandData[3];
                机头.参数.退币时给游戏机脉冲数比例因子 = data.commandData[4];
                机头.参数.退币时卡上增加币数比例因子 = data.commandData[5];
                机头.参数.退币按钮脉宽 = BitConverter.ToUInt16(data.commandData, 6);

                string value = PubLib.Hex2BitString(BitConverter.ToUInt16(data.commandData, 14));
                机头.开关.允许电子退币或允许打票 = (value.Substring(15, 1) == "1");
                机头.开关.允许电子投币 = (value.Substring(14, 1) == "1");
                机头.开关.允许十倍投币 = (value.Substring(13, 1) == "1");                
                机头.开关.允许实物退币 = (value.Substring(12, 1) == "1");
                机头.开关.转发实物投币 = (value.Substring(11, 1) == "1");
                机头.开关.硬件投币控制 = (value.Substring(10, 1) == "1");
                机头.开关.投币脉冲电平 = (value.Substring(9, 1) == "1");
                机头.开关.第二路上分线上分电平 = (value.Substring(8, 1) == "1");
                机头.开关.SSR退币驱动脉冲电平 = (value.Substring(7, 1) == "1");
                机头.开关.数币脉冲电平 = (value.Substring(6, 1) == "1");
                机头.开关.启用第二路上分信号 = (value.Substring(5, 1) == "1");

                value = PubLib.Hex2BitString(BitConverter.ToUInt16(data.commandData, 16));
                机头.开关.退币超限标志 = (value.Substring(15, 1) == "1");                
                机头.开关.启用专卡专用 = (value.Substring(14, 1) == "1");
                机头.开关.BO按钮是否维持 = (value.Substring(13, 1) == "1");
                机头.开关.退分锁定标志 = (value.Substring(12, 1) == "1");
                机头.开关.启用异常退币检测 = (value.Substring(11, 1) == "1");
                机头.开关.启用即中即退模式 = (value.Substring(10, 1) == "1");
                机头.开关.启用外部报警检测 = (value.Substring(9, 1) == "1");
                机头.开关.启用回路报警检测 = (value.Substring(8, 1) == "1");
                机头.开关.增强防止转卡 = (value.Substring(7, 1) == "1");
                机头.开关.启动刷卡即扣 = (value.Substring(6, 1) == "1");

                机头.参数.首次投币启动间隔 = data.commandData[18];
                机头.参数.投币速度 = BitConverter.ToUInt16(data.commandData, 19);
                机头.参数.投币脉宽 = data.commandData[21];
                机头.参数.第二路上分线首次上分启动间隔 = data.commandData[22];
                机头.参数.第二路上分线上分速度 = BitConverter.ToUInt16(data.commandData, 23);
                机头.参数.第二路上分线上分脉宽 = data.commandData[25];
                机头.参数.退币速度 = BitConverter.ToUInt16(data.commandData, 26);
                机头.参数.退币脉宽 = data.commandData[28];
                机头.参数.异常SSR退币检测速度 = data.commandData[34];
                机头.参数.异常SSR退币检测次数 = data.commandData[35];

                StringBuilder sb = new StringBuilder();
                sb = new StringBuilder();
                sb.Append("=============================================\r\n");
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  接收数据\r\n", DateTime.Now);
                sb.AppendFormat("{0}\r\n", PubLib.BytesToString(data.commandData));
                sb.AppendFormat("机头地址：{0}\r\n", 机头地址);
                sb.AppendFormat("单次退币限额：{0}\r\n", 机头.参数.单次退币限额);
                sb.AppendFormat("当前扣币基数：{0}\r\n", 当前扣币基数);
                sb.AppendFormat("退币时接收游戏机数币数：{0}\r\n", 机头.参数.退币时给游戏机脉冲数比例因子);
                sb.AppendFormat("退币时卡上增加币数：{0}\r\n", 机头.参数.退币时卡上增加币数比例因子);
                sb.AppendFormat("开关1：{0}\r\n", PubLib.Hex2BitString(BitConverter.ToUInt16(data.commandData, 14)));
                sb.AppendFormat("开关2：{0}\r\n", PubLib.Hex2BitString(BitConverter.ToUInt16(data.commandData, 16)));
                sb.AppendFormat("首次投币启动间隔：{0}\r\n", 机头.参数.首次投币启动间隔);
                sb.AppendFormat("投币速度：{0}\r\n", 机头.参数.投币速度);
                sb.AppendFormat("投币脉宽：{0}\r\n", 机头.参数.投币脉宽);
                sb.AppendFormat("第二路上分线上分启动间隔：{0}\r\n", 机头.参数.第二路上分线首次上分启动间隔);
                sb.AppendFormat("第二路上分线上分速度：{0}\r\n", 机头.参数.第二路上分线上分速度);
                sb.AppendFormat("第二路上分线上分脉宽：{0}\r\n", 机头.参数.第二路上分线上分脉宽);
                sb.AppendFormat("退币速度：{0}\r\n", 机头.参数.退币速度);
                sb.AppendFormat("退币脉宽：{0}\r\n", 机头.参数.退币脉宽);
                sb.AppendFormat("异常SSR退币检测速度：{0}\r\n", 机头.参数.异常SSR退币检测速度);
                sb.AppendFormat("异常SSR退币检测次数：{0}\r\n", 机头.参数.异常SSR退币检测次数);
                UIClass.接收内容 = sb.ToString();  
            }
            catch 
            {
                throw;
            }
        }
    }
}
