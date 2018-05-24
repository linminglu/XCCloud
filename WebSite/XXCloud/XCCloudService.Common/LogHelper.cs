﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using XCCloudService.Common.Enum;

namespace XCCloudService.Common
{
    public class LogHelper
    {
        public static void SaveLog(string strMsg)
        {
            string s = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
            SaveLogFile(s + strMsg);
        }

        protected static void SaveLogFile(string strErrMsg)
        {
            string logRootDirectory = ("c:/Logs/");
            if (!Directory.Exists(logRootDirectory))
            {
                Directory.CreateDirectory(logRootDirectory);
            }

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            FileInfo inf = new FileInfo(logRootDirectory + fileName);
            StreamWriter wri = new StreamWriter(logRootDirectory + fileName, true, Encoding.UTF8, 1024);
            wri.WriteLine(strErrMsg);
            wri.Close();
        }


        public static void SaveLog(TxtLogType txtLogType ,string logTxt)
        {
            StreamWriter wri = null;
            try
            {
                string logRootDirectory = CommonConfig.TxtLogPath + GetTextLogChildPath(txtLogType);
                if (!Directory.Exists(logRootDirectory))
                {
                    Directory.CreateDirectory(logRootDirectory);
                }

                string fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
                FileInfo inf = new FileInfo(logRootDirectory + fileName);
                wri = new StreamWriter(logRootDirectory + fileName, true, Encoding.UTF8, 1024);
                string tip = string.Format("{0}{1}{2}", "***************************\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "***************************");
                wri.WriteLine(tip);
                wri.WriteLine(logTxt);
                wri.WriteLine("");
            }
            catch
            {

            }
            finally
            {
                if (wri != null)
                {
                    wri.Close();
                }
            }
        }

        public static void SaveLog(TxtLogType txtLogType, TxtLogContentType txtLogContentType, TxtLogFileType txtLogFileType, string logTxt)
        {
            StreamWriter wri = null;
            try
            {
                string logRootDirectory = string.Format("{0}{1}{2}",CommonConfig.TxtLogPath,GetTextLogChildPath(txtLogType),GetTextLogContentChildPath(txtLogContentType));
                if (!Directory.Exists(logRootDirectory))
                {
                    Directory.CreateDirectory(logRootDirectory);
                }

                string fileName = GetFileName(txtLogFileType);
                FileInfo inf = new FileInfo(logRootDirectory + fileName);
                wri = new StreamWriter(logRootDirectory + fileName, true, Encoding.UTF8, 1024);
                string tip = string.Format("{0}{1}{2}", "***************************", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "***************************");
                wri.WriteLine(tip);
                wri.WriteLine(logTxt);
                wri.WriteLine("");
            }
            catch
            {

            }
            finally
            {
                if (wri != null)
                {
                    wri.Close();
                }
            }
        }

        protected static string GetTextLogChildPath(TxtLogType txtLogType)
        {
            switch (txtLogType)
            {
                case TxtLogType.SystemInit: return "Init/";
                case TxtLogType.UPDService: return "UPD/";
                case TxtLogType.TCPService: return "TCP/";
                case TxtLogType.WeiXin: return "WeiXin/";
                case TxtLogType.WeiXinPay: return "WeiXinPay/";
                case TxtLogType.Api: return "Api/";
                case TxtLogType.AliPay: return "AliPay/";
                case TxtLogType.LogDBExcepton: return "LogDBExcepton/";
                default: return string.Empty;
            }
        }

        protected static string GetTextLogContentChildPath(TxtLogContentType txtLogContentType)
        {
            switch (txtLogContentType)
            {
                case TxtLogContentType.Common: return "Common/";
                case TxtLogContentType.Exception: return "Exception/";
                case TxtLogContentType.Debug: return "Debug/";
                case TxtLogContentType.Record: return "Record/";
                default: return string.Empty;
            }
        }

        protected static string GetTextLogDayChildPath()
        {
            return System.DateTime.Now.ToString("yyyyMMdd") + "/";
        }

        protected static string GetTextLogTimeChildPath()
        {
            return System.DateTime.Now.ToString("HH") + "/";
        }

        protected static string GetTextStoreChildPath(string storeId)
        {
            return storeId + "/";
        }

        protected static string GetFileName(TxtLogFileType txtLogFileType)
        {
            switch (txtLogFileType)
            {
                case TxtLogFileType.Day: return DateTime.Now.ToString("yyyyMMdd") + ".txt";
                case TxtLogFileType.Time: return string.Format("{0}{1}{2}{3}",DateTime.Now.ToString("yyyyMMddHHmmss"),"_",Utils.Number(6),".txt");
                default: return DateTime.Now.ToString("yyyyMMdd") + ".txt";
            }
        }
    }
}