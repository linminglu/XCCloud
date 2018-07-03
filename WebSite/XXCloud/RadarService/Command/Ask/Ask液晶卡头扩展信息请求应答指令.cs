using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Ask
{
    public class Ask液晶卡头扩展信息请求应答指令
    {
        public byte 机头地址 { get; set; }
        public byte 处理结果 { get; set; }
        public UInt16 流水号 { get; set; }
    }
}
