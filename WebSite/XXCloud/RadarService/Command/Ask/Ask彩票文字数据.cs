using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Ask
{
    public class Ask彩票文字数据
    {
        public Ask彩票文字数据(byte version, int lineNo,int lineCount, string words)
        {
            byte[] wordBytes = Encoding.GetEncoding("gb2312").GetBytes(words);
            List<byte> sendbuf = new List<byte>();
            sendbuf.Add((byte)lineNo);
            sendbuf.Add((byte)lineCount);
            sendbuf.AddRange(wordBytes);

            Ask数据透传广播指令 ask = new Ask数据透传广播指令(透传指令类别.彩票文字, version, 1, 1, sendbuf.ToArray());
        }
    }
}
