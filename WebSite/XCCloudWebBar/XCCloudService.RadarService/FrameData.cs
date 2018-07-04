using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.RadarService
{
    public class FrameData
    {
        /// <summary>
        /// 是否完整
        /// </summary>
        public bool isComplite = false;
        public string routeAddress = "";
        public CommandType commandType;
        public byte[] commandData;
        public int commandLength;
        public byte[] recvData;

        public FrameData()
        { }

        public FrameData(byte[] data)
        {
            int i = 0;
            int start = 0, len = 0;
            for (i = 0; i < data.Length; i++)
            {
                if (data[i] == UDPServer.FrameHead)
                {
                    start = i;
                    break;
                }
            }

            for (i = data.Length - 1; i >= 0; i--)
            {
                if (data[i] == UDPServer.FrameButtom)
                {
                    len = i - start;
                    break;
                }
            }

            recvData = data.Skip(start).Take(len + 1).ToArray();
            try
            {
                if (Coding.CRC.CRCCheck(data))
                {
                    isComplite = true;
                    routeAddress = Coding.ConvertData.Hex2String(data[3]) + Coding.ConvertData.Hex2String(data[2]);
                    commandType = (CommandType)data[4];
                    commandLength = (int)data[5];
                    commandData = new byte[commandLength];
                    Array.Copy(data, 6, commandData, 0, commandLength);
                }
                else
                {
                    LogHelper.WriteLog("数据接收错误", data);
                    HostServer.当前错误指令数++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("数据接收错误", data);
                LogHelper.WriteLog(ex);
            }
        }
    }
}
