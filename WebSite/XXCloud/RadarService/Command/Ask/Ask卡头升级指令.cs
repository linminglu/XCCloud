using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Ask
{
    public class Ask卡头升级指令
    {
        public byte[] ReadyToSend;
        public bool IsSuccess = false;

        public Ask卡头升级指令(int 大帧序号, int 小帧序号, byte[] 数据)
        {
            try
            {
                FrameData data = new FrameData();
                data.commandType = CommandType.卡头升级指令;
                data.routeAddress = PubLib.路由器段号;
                List<byte> dataList = new List<byte>();
                dataList.Add((byte)大帧序号);
                dataList.Add((byte)小帧序号);
                dataList.AddRange(数据);

                ReadyToSend = PubLib.GetFrameDataBytes(data, dataList.ToArray(), CommandType.卡头升级指令);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", DateTime.Now);
                sb.Append(PubLib.BytesToString(ReadyToSend) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", CommandType.卡头升级指令);
                sb.AppendFormat("大帧序号：{0}\r\n", 大帧序号);
                sb.AppendFormat("小帧序号：{0}\r\n", 小帧序号);

                UIClass.发送内容 = sb.ToString();

                IsSuccess = true;
            }
            catch
            {
                throw;
            }
        }
    }
}
