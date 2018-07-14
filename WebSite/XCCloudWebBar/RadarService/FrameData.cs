using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService
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
                if (Coding.CRC.CRCCheck(recvData))
                {
                    isComplite = true;
                    routeAddress = Coding.ConvertData.Hex2String(recvData[3]) + Coding.ConvertData.Hex2String(recvData[2]);
                    commandType = (CommandType)recvData[4];
                    commandLength = (int)recvData[5];
                    commandData = new byte[commandLength];
                    Array.Copy(recvData, 6, commandData, 0, commandLength);
                }
                else
                {
                    LogHelper.LogHelper.WriteLog("数据接收错误", data);
                    HostServer.当前错误指令数++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog("数据接收错误", data);
                LogHelper.LogHelper.WriteLog(ex);
            }
        }
    }
}
