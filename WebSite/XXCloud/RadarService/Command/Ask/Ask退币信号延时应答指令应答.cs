using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Ask
{
    public class Ask退币信号延时应答指令应答
    {
        public byte 机头地址 { get; set; }
        public UInt16 流水号 { get; set; }
        public Ask退币信号延时应答指令应答(string hAddress, UInt16 sn)
        {
            机头地址 = Convert.ToByte(hAddress, 16);
            流水号 = sn;
        }
    }
}
