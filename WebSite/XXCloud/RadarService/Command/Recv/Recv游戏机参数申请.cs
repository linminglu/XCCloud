using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace RadarService.Command.Recv
{
    public class Recv游戏机参数申请
    {
        public string 机头地址 = "";
        public byte[] SendData = null;
        public FrameData RecvData;
        public object 应答数据;
        public bool IsDevice = false;
        public DateTime SendDataTime;

        public Recv游戏机参数申请(FrameData data)
        {
            try
            {
                机头地址 = Coding.ConvertData.Hex2String(data.commandData[0]).ToLower();
                string MCUID = XCCloudSerialNo.SerialNoHelper.StringGet("info_" + data.routeAddress + "|" + 机头地址);
                if (MCUID == null) return;
                Info.DeviceInfo.机头信息 head = XCCloudSerialNo.SerialNoHelper.StringGet<Info.DeviceInfo.机头信息>("headinfo_" + MCUID);
                if (head != null)
                {
                    switch (head.类型)
                    {
                        case Info.DeviceInfo.设备类型.存币机:
                        case Info.DeviceInfo.设备类型.提币机:
                        case Info.DeviceInfo.设备类型.碎票机:
                        case Info.DeviceInfo.设备类型.投币机:
                            IsDevice = true;
                            Ask.Ask自助设备参数申请 askData1 = new Ask.Ask自助设备参数申请(head);
                            应答数据 = askData1;
                            break;
                        default:
                            IsDevice = false;
                            Ask.Ask游戏机参数申请 askData2 = new Command.Ask.Ask游戏机参数申请(head);
                            应答数据 = askData2;
                            break;
                    }
                    byte[] dataBuf = Coding.ConvertData.GetBytesByObject(应答数据);
                    SendData = Coding.ConvertData.GetFrameDataBytes(data, dataBuf, CommandType.游戏机参数申请应答);
                    RecvData = data;
                }
            }
            catch
            {
                throw;
            }
        }

        public string GetRecvData(DateTime printDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", printDate);
            sb.Append(Coding.ConvertData.BytesToString(RecvData.recvData) + Environment.NewLine);
            sb.AppendFormat("指令类别：{0}\r\n", RecvData.commandType);
            sb.AppendFormat("路由器地址：{0}\r\n", RecvData.routeAddress);
            sb.AppendFormat("机头地址：{0}\r\n", 机头地址);
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();
            if (!IsDevice)
            {
                Ask.Ask游戏机参数申请 AskData = 应答数据 as Ask.Ask游戏机参数申请;
                sb.Append("=============================================\r\n");
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
                sb.Append(Coding.ConvertData.BytesToString(SendData) + Environment.NewLine);
                sb.Append("当前是机头在获取参数\r\n");
                sb.AppendFormat("机头地址：{0}\r\n", Coding.ConvertData.Hex2String(AskData.机头地址));
                sb.AppendFormat("单次退币限额：{0}\r\n", AskData.单次退币限额);
                sb.AppendFormat("退币时接收游戏机数币数：{0}\r\n", AskData.退币时给游戏机脉冲数比例因子);
                //sb.AppendFormat("退币时卡上增加币数：{0}\r\n", AskData.退币时卡上增加币数比例因子);
                sb.AppendFormat("本店卡校验密码：{0}\r\n", AskData.本店卡校验密码);
                sb.AppendFormat("开关1：{0}\r\n", Coding.ConvertData.Hex2BitString(AskData.开关1));
                sb.AppendFormat("开关2：{0}\r\n", Coding.ConvertData.Hex2BitString(AskData.开关2));
                sb.AppendFormat("首次投币启动间隔：{0}\r\n", AskData.首次投币启动间隔);
                sb.AppendFormat("退币速度：{0}\r\n", AskData.退币速度);
                sb.AppendFormat("退币脉宽：{0}\r\n", AskData.退币脉宽);
                sb.AppendFormat("投币速度：{0}\r\n", AskData.投币速度);
                sb.AppendFormat("投币脉宽：{0}\r\n", AskData.投币脉宽);
                sb.AppendFormat("第二路上分线上分脉宽：{0}\r\n", AskData.第二路上分线上分脉宽);
                sb.AppendFormat("第二路上分线上分启动间隔：{0}\r\n", AskData.第二路上分线首次上分启动间隔);
                sb.AppendFormat("第二路上分线上分速度：{0}\r\n", AskData.第二路上分线上分速度);
            }
            else
            {
                //Ask.Ask存币机参数申请 AskData = 应答数据 as Ask.Ask存币机参数申请;
                //sb.Append("=============================================\r\n");
                //sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
                //sb.Append(Coding.ConvertData.BytesToString(SendData) + Environment.NewLine);
                //sb.Append("当前是存币机在获取参数\r\n");
                //sb.AppendFormat("机头地址：{0}\r\n", Coding.ConvertData.Hex2String(AskData.机头地址));
                //sb.AppendFormat("马达配置：{0}\r\n", Coding.ConvertData.Hex2BitString(AskData.马达配置));
                //sb.AppendFormat("马达1比例：{0}\r\n", AskData.马达1比例);
                //sb.AppendFormat("马达2比例：{0}\r\n", AskData.马达2比例);
                //sb.AppendFormat("存币箱最大存币数：{0}\r\n", AskData.存币箱最大存币数);
            }
            return sb.ToString();
        }
    }
}
