using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace PalletService.Notify
{
    public class StateObject
    {
        public const int BUF_SIZE = 1024 * 8;
        public Socket socket;
        public byte[] buffer = new byte[BUF_SIZE];
    }

    public class FrameData
    {
        public int DataLen { get; set; }
        public int FrameIndex { get; set; }
        public int FrameCount { get; set; }
        public byte FrameType { get; set; }
        public string FrameJsontxt { get; set; }

        public bool IsSuccess = false;
        /// <summary>
        /// 检查接收数据
        /// </summary>
        /// <param name="recBytes"></param>
        /// <returns></returns>
        byte[] AnalyzeClientData(byte[] recBytes)
        {
            List<byte> buf = new List<byte>();
            int flag = 0;
            for (int i = 0; i < recBytes.Length; i++)
            {
                switch (flag)
                {
                    case 0:     //检测到引导区
                        if (recBytes[i] == 0xfe)
                            flag++;
                        break;
                    case 1:
                        if (recBytes[i] == 0x68)
                        {
                            buf.Add(recBytes[i]);
                            flag++;     //收到帧头
                        }
                        else if (recBytes[i] != 0xfe)
                            flag = 0;   //收到的非帧头和引导字节则为非法数据
                        break;
                    case 2:
                        buf.Add(recBytes[i]);
                        if (buf.Count > 6)
                        {
                            if (buf[buf.Count - 3] == 0x16 && buf[buf.Count - 2] == 0xfe && buf[buf.Count - 1] == 0xfe)
                            {
                                //收到有效帧尾
                                int dataLen = BitConverter.ToUInt16(buf.ToArray(), 1);
                                int frameIndex = buf[3];
                                int frameCount = buf[4];
                                if (frameCount == 1 && frameIndex == 1)
                                {
                                    //单包数据直接处理
                                    return buf.ToArray();
                                }
                                else
                                {
                                    //需要拼包缓存处理
                                }
                            }
                        }
                        break;
                }
            }
            return null;
        }

        public FrameData(byte[] data)
        {
            byte[] recv = AnalyzeClientData(data);
            if (recv == null)
            {
                IsSuccess = false;
                return;
            }
            this.DataLen = BitConverter.ToInt16(recv, 1);
            this.FrameIndex = recv[3];
            this.FrameCount = recv[4];
            this.FrameType = recv[5];
            this.FrameJsontxt = Encoding.UTF8.GetString(recv, 6, DataLen);
            IsSuccess = true;
        }
    }
}
