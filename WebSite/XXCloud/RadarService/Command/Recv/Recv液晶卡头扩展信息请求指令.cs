using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Recv
{
    public enum 参数类别
    {
        机头二维码 = 0x01,
        出票器网关规则 = 0x02,
        显示参数 = 0x03,
    }
    public class Recv液晶卡头扩展信息请求指令
    {
        public string 机头地址 = "";
        public 参数类别 参数;
        public UInt16 流水号;
        public byte[] SendData = null;
        public FrameData RecvData;
        public Ask.Ask液晶卡头扩展信息请求应答指令 应答数据;
        public UInt16 测试数据;
        public DateTime SendDataTime;
        public DateTime RecvDataTime;
        public bool isRepeat = false;
        public Info.DeviceInfo.机头信息 head = null;

        public Recv液晶卡头扩展信息请求指令(FrameData f, DateTime rTime)
        {
            机头地址 = Coding.ConvertData.Hex2String(f.commandData[0]);
            参数 = (参数类别)f.commandData[1];
            流水号 = BitConverter.ToUInt16(f.commandData, 2);
            string MCUID = XCCouldSerialNo.SerialNoHelper.StringGet("info_" + f.routeAddress + "|" + 机头地址);
            if (MCUID == null) return;
            head = XCCouldSerialNo.SerialNoHelper.StringGet<Info.DeviceInfo.机头信息>("headinfo_" + MCUID);

            RecvData = f;
            object obj = null;
            int res = HostServer.CheckRepeat(f.routeAddress, 机头地址, "", CommandType.液晶卡头扩展信息请求指令, ref obj, 流水号);
            isRepeat = (res == 1);
            if (!isRepeat)
                HostServer.InsertRepeat(f.routeAddress, 机头地址, "", CommandType.液晶卡头扩展信息请求指令, CommandType.液晶卡头扩展信息请求应答指令, 应答数据, 流水号, RecvDataTime);
            应答数据 = new Ask.Ask液晶卡头扩展信息请求应答指令();
            应答数据.处理结果 = 0x01;
            应答数据.机头地址 = Convert.ToByte(机头地址, 16);
            应答数据.流水号 = 流水号;

            byte[] dataBuf = Coding.ConvertData.GetBytesByObject(应答数据);
            SendData = Coding.ConvertData.GetFrameDataBytes(f, dataBuf, CommandType.液晶卡头扩展信息请求应答指令);
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
            sb.AppendFormat("参数：{0}\r\n", 参数);
            sb.AppendFormat("流水号：{0}\r\n", 流水号);
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
            sb.Append(Coding.ConvertData.BytesToString(SendData) + Environment.NewLine);
            sb.AppendFormat("机头地址：{0}\r\n", Coding.ConvertData.Hex2String(应答数据.机头地址));
            sb.AppendFormat("处理结果：{0}\r\n", 应答数据.处理结果);
            sb.AppendFormat("流水号：{0}\r\n", 应答数据.流水号);
            return sb.ToString();
        }
    }
}
