using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Ask
{
    public class Ask设置LOGO图片
    {
        public Ask设置LOGO图片(byte version,byte[] image)
        {
            int packetCount = image.Length / PubLib.MaxPacketSize;
            int resumeLenght = image.Length % PubLib.MaxPacketSize;

            if (resumeLenght != 0)
                packetCount++;
            else
                resumeLenght = PubLib.MaxPacketSize;
            for (int i = 0; i < packetCount; i++)
            {
                byte[] routeBytes;
                if (i == packetCount - 1)
                {
                    //最后一包
                    routeBytes = new byte[resumeLenght];
                }
                else
                {
                    routeBytes = new byte[PubLib.MaxPacketSize];
                }
                Array.Copy(image, i * PubLib.MaxPacketSize, routeBytes, 0, routeBytes.Length);
                
                Ask数据透传广播指令 ask = new Ask数据透传广播指令(透传指令类别.LOGO图片, version, i + 1, packetCount, routeBytes);
            }
        }
    }
}
