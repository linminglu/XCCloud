using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.PublicHelper
{
    /// <summary>
    /// 公共变量定义
    /// </summary>
    public class SystemDefiner
    {
        public static int 测试间隔时间 = 0;
        public static string AppSecret = "";
        public static bool AllowProjectAddup = false;
        public static string LogPath = "C:\\xclog";
        public static string MerchID = "";
        public static string StoreID = "";

        public static int ProxyType = 0;
        public static bool InitSuccess = false;
        public static string StorePassword = "";
        public static int ElecTicketValidDay = 0;
        public static bool PrintBarcode = false;
        public static string StoreName = "";

        static int coinSN = 0;
        public static int 雷达主发流水号
        {
            get
            {
                coinSN++;
                if (coinSN == 65535)
                    coinSN = 1;
                else if (coinSN == 0)
                    coinSN++;
                return coinSN;
            }
        }

    }
}
