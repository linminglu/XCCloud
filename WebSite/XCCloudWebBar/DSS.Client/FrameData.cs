using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Client
{
    class FrameData
    {
        const byte BLANKWORD = 0xfe;
        const byte STARTWORD = 0x68;
        const byte ENDWORD = 0x16;
        public int DataLen { get; set; }
        public TransmiteEnum CommandType { get; set; }
        public byte[] RecvData { get; set; }
        public string FrameJsontxt { get; set; }
        public bool CheckSuccess { get; set; }
        public EndPoint RecvPoint { get; set; }
        public FrameData(byte[] data, EndPoint remotePoint)
        {
            if (data[0] == BLANKWORD && data[1] == BLANKWORD && data[2] == STARTWORD && data[data.Length - 1] == BLANKWORD && data[data.Length - 2] == BLANKWORD && data[data.Length - 3] == ENDWORD)
            {
                try
                {
                    //符合帧结构
                    CheckSuccess = true;
                    DataLen = BitConverter.ToUInt16(data, 3);
                    CommandType = (TransmiteEnum)data[7];
                    RecvPoint = remotePoint;
                    Array.Copy(data, 8, RecvData, 0, DataLen);
                    FrameJsontxt = Encoding.UTF8.GetString(RecvData);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex, data);
                }
            }
        }
    }
}
