using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.RadarService.Coding
{
    public static class ConvertData
    {
        public static string BytesToString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(Hex2String(b) + " ");
            }
            return sb.ToString();
        }

        public static byte[] StringToByte(string data)
        {
            List<byte> b = new List<byte>();
            if (data.Length % 2 != 0) return Encoding.ASCII.GetBytes("123456");

            for (int i = 0; i < data.Length; i += 2)
            {
                b.Add(Convert.ToByte(data.Substring(i, 2), 16));
            }
            return b.ToArray();
        }

        public static string UInt32ToHexString(UInt32 data)
        {
            string value = "";
            byte[] bytes = BitConverter.GetBytes(data);
            foreach (byte b in bytes)
            {
                value = Hex2String(b) + value;
            }
            return value;
        }
        public static string Hex2String(byte data)
        {
            string value = Convert.ToString(data, 16);
            if (value.Length == 1)
            {
                value = "0" + value;
            }
            return value;
        }
        public static string Hex2String(byte[] data)
        {
            string value = "";
            foreach (byte b in data)
            {
                value += Hex2String(b);
            }
            return value;
        }

        public static string Hex2BitString(byte data)
        {
            string value = Convert.ToString(data, 2);
            int len = value.Length;
            for (int i = 0; i < 8 - len; i++)
            {
                value = "0" + value;
            }
            return value;
        }

        public static string Hex2BitString(UInt16 data)
        {
            string value = Convert.ToString(data, 2);
            int len = value.Length;
            for (int i = 0; i < 16 - len; i++)
            {
                value = "0" + value;
            }
            return value;
        }

        public static byte[] GetFrameDataBytes(FrameData dataFrame, byte[] dataBytes, CommandType cmdType)
        {
            List<byte> lstData = new List<byte>();
            lstData.Add(UDPServer.FrameHead);
            lstData.Add(0x00);
            byte[] bs = BitConverter.GetBytes(Convert.ToInt16("0x" + dataFrame.routeAddress, 16));
            lstData.AddRange(bs);
            lstData.Add((byte)cmdType);
            if (dataBytes != null)
            {
                lstData.Add((byte)dataBytes.Length);
                lstData.AddRange(dataBytes);
            }
            else
            {
                lstData.Add(0);
            }
            bs = BitConverter.GetBytes((Int16)CRC.Crc16(lstData.ToArray(), (byte)lstData.Count));
            lstData.AddRange(bs);
            lstData.Add(UDPServer.FrameButtom);
            List<byte> value = new List<byte>();
            value.Add(UDPServer.FrameBlankWord);
            value.Add(UDPServer.FrameBlankWord);
            value.Add(UDPServer.FrameBlankWord);
            value.Add(UDPServer.FrameBlankWord);
            value.AddRange(lstData);
            value.Add(UDPServer.FrameBlankWord);
            value.Add(UDPServer.FrameBlankWord);

            return value.ToArray();
        }

        public static byte[] GetBytesByObject(object o)
        {
            List<byte> value = new List<byte>();
            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                switch (pi.PropertyType.Name.ToLower())
                {
                    case "byte[]":
                        value.AddRange((byte[])pi.GetValue(o, null));
                        break;
                    case "byte":
                        value.Add((byte)pi.GetValue(o, null));
                        break;
                    case "uint16":
                        byte[] tuv = BitConverter.GetBytes((UInt16)pi.GetValue(o, null));
                        value.AddRange(tuv);
                        break;
                    case "int16":
                        byte[] tv = BitConverter.GetBytes((Int16)pi.GetValue(o, null));
                        value.AddRange(tv);
                        break;
                    case "uint32":
                        byte[] tul = BitConverter.GetBytes((UInt32)pi.GetValue(o, null));
                        value.AddRange(tul);
                        break;
                    case "int32":
                        byte[] tl = BitConverter.GetBytes((Int32)pi.GetValue(o, null));
                        value.AddRange(tl);
                        break;
                    case "uint64":
                        byte[] tl64 = BitConverter.GetBytes((UInt64)pi.GetValue(o, null));
                        value.AddRange(tl64);
                        if (pi.Name == "MCUID")
                        {
                            //芯片长地址移除最高位，转换成7位
                            value.RemoveAt(value.Count - 1);
                        }
                        break;
                    case "string":
                        switch (pi.Name)
                        {
                            case "本店卡校验密码":
                                string pv = pi.GetValue(o, null).ToString();
                                byte[] pvbytes = StringToByte(pv);
                                value.AddRange(pvbytes);
                                break;
                            case "IC卡号码":
                                string ic = pi.GetValue(o, null).ToString();
                                byte[] bytes = Encoding.ASCII.GetBytes(ic);
                                value.AddRange(bytes);
                                break;
                            case "游戏机机头编号":
                                string hid = pi.GetValue(o, null).ToString();
                                byte[] hidbytes = Encoding.ASCII.GetBytes(hid);
                                value.AddRange(hidbytes);
                                break;
                        }
                        break;
                    case "datetime":
                        DateTime dtd = Convert.ToDateTime(pi.GetValue(o, null).ToString());
                        byte[] dtdbytes = DateTimeBCD(dtd);
                        value.AddRange(dtdbytes);
                        break;
                }
            }
            return value.ToArray();
        }

        public static byte[] DateTimeBCD()
        {
            DateTime d = DateTime.Now;
            List<byte> res = new List<byte>();
            res.Add(0x00);
            res.Add((byte)Convert.ToInt32((d.Year - 2000).ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Month.ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Day.ToString(), 16));
            res.Add((byte)Convert.ToInt32(((int)d.DayOfWeek).ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Hour.ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Minute.ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Second.ToString(), 16));

            return res.ToArray();
        }
        public static byte[] DateTimeBCD(DateTime d)
        {
            List<byte> res = new List<byte>();
            res.Add(0x00);
            res.Add((byte)Convert.ToInt32((d.Year - 2000).ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Month.ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Day.ToString(), 16));
            res.Add((byte)Convert.ToInt32(((int)d.DayOfWeek).ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Hour.ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Minute.ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Second.ToString(), 16));

            return res.ToArray();
        }

        public static byte[] DateBCD()
        {
            DateTime d = DateTime.Now;
            List<byte> res = new List<byte>();
            res.Add(0x20);
            res.Add((byte)Convert.ToInt32((d.Year - 2000).ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Month.ToString(), 16));
            res.Add((byte)Convert.ToInt32(d.Day.ToString(), 16));

            return res.ToArray();
        }

        public static DateTime GetTimeBCD(byte[] data)
        {
            string value = string.Format("20{0}-{1}-{2} {3}:{4}:{5}", data[1], data[2], data[3], data[5], data[6], data[7]);
            DateTime d;
            if (!DateTime.TryParse(value, out d))
            {
                d = DateTime.Now;
            }
            return d;
        }
    }
}
