﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSS;

namespace RadarService.LogHelper
{
    public class LogHelper
    {
        public static string LogPath = "";
        static string BytesToString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(Hex2String(b) + " ");
            }
            return sb.ToString();
        }
        static string Hex2String(byte data)
        {
            string value = Convert.ToString(data, 16);
            if (value.Length == 1)
            {
                value = "0" + value;
            }
            return value;
        }
        public static void WriteLog(Exception ex)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("错误时间：" + d.ToString());
                sw.WriteLine("错误内容：" + ex.Message);
                sw.WriteLine("堆栈信息：" + ex.StackTrace);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        public static void WriteLog(Exception ex, byte[] data)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("错误时间：" + d.ToString());
                sw.WriteLine("错误内容：" + ex.Message);
                if (data != null)
                    sw.WriteLine("数据包内容：" + BytesToString(data));
                sw.WriteLine("堆栈信息：" + ex.StackTrace);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        public static void WriteLog(string errorMsg, byte[] data)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + "coin.txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("错误时间：" + d.ToString());
                sw.WriteLine("错误内容：" + errorMsg);
                if (data != null)
                    sw.WriteLine("数据包内容：" + BytesToString(data));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        public static void WriteLogSpacail(string msg)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + "OFF.txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("事件时间：" + d.ToString());
                sw.WriteLine("事件内容：" + msg);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        public static void WriteLog(string errorMsg)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("事件时间：" + d.ToString());
                sw.WriteLine("事件内容：" + errorMsg);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        public static void WriteDBLog(string sql)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + "db.txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("执行时间：" + d.ToString());
                sw.WriteLine("执行语句：" + sql);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        public static void WriteTBLog(string txt)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd HH") + " HourTB.txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("接收时间：" + d.ToString());
                sw.WriteLine("原始数据：" + txt);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }
        public static void WritePush(byte[] data, string segment, string headAddress, string pushType, string coin, string sn)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\SN\\{0}-{1}\\{2}\\{3}", segment, headAddress, d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd HH") + " PUSH.txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("接收时间：" + d.ToString());
                if (data != null)
                    sw.WriteLine("原始数据：" + BytesToString(data));
                sw.WriteLine("投币类别：" + pushType);
                sw.WriteLine("投币数：" + coin);
                sw.WriteLine("投币流水：" + sn);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        public static void WriteSNLog(string type, string headAddress, string segment, string sn, byte[] data)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(LogPath + "\\SN\\{0}-{1}\\{2}\\{3}", segment, headAddress, d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd HH") + " .txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("接收时间：" + d.ToString());
                sw.WriteLine("类别：" + type);
                sw.WriteLine("流水号：" + sn);
                if (data != null)
                    sw.WriteLine("原始数据：" + BytesToString(data));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }
        public static void RadarDataLog(string segment, int directType, byte[] data)
        {
            string 分库日期 = DateTime.Now.ToString("yyyyMM");
            DataAccess ac = new DataAccess();
            //检查当前雷达日志分库是否存在
            string sql = "select count(name) from sysobjects where xtype='u' and name='Data_RadarLog_" + 分库日期 + "'";
            DataTable dt = ac.ExecuteQueryReturnTable(sql);
            if (dt.Rows.Count == 0)
            {
                //找不到当月的分库时自动创建
                string script = string.Format(@"
create table Data_RadarLog_{0} (
   ID                   int                  identity,
   MerchID              varchar(15)          null,
   StoreID              varchar(15)          null,
   Segment              varchar(10)          null,
   CreateTime           datetime             null,
   DataDirect           int                  null,
   DataContext          binary(500)          null,
   constraint PK_DATA_RADARLOG primary key (ID)
)", 分库日期);
                ac.Execute(script);
            }
            sql = string.Format("insert into Data_RadarLog_" + 分库日期 + " values ('{0}','{1}','{2}',getdate(),,'{3}',{4})"
                , PublicHelper.SystemDefiner.MerchID, PublicHelper.SystemDefiner.StoreID, segment, directType, data
                );
            ac.Execute(sql);
        }
    }
}
