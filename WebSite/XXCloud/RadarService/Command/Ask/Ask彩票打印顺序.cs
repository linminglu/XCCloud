using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Ask
{
    public class Ask彩票打印顺序
    {
        public Ask彩票打印顺序(byte version, byte[] byteIndex)
        {
            Ask数据透传广播指令 ask = new Ask数据透传广播指令(透传指令类别.打印顺序, version, 1, 1, byteIndex);
        }
    }
}
