using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Recv
{
    class Recv游戏机参数修改
    {
        public string 机头地址 = "";
        public Recv游戏机参数修改(FrameData data)
        {
            try
            {
                机头地址 = PubLib.Hex2String(data.commandData[0]);
                string bitValue = PubLib.Hex2BitString(data.commandData[1]);
                PubLib.机头地址修改应答 应答 = new PubLib.机头地址修改应答();
                应答.机头在线状态 = (bitValue.Substring(0) == "1");
            }
            catch 
            {
                throw;
            }
        }
    }
}
