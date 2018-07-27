using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletService.Common
{
    public class CommonConfig
    {
        public static string TxtLogPath;

        public static string XCCloudUDPServiceHost = System.Configuration.ConfigurationSettings.AppSettings["XCCloudUDPServiceHost"] ?? "";

        public static string XCCloudUDPServicePort = System.Configuration.ConfigurationSettings.AppSettings["XCCloudUDPServicePort"] ?? "";

        public static string MerchId = System.Configuration.ConfigurationSettings.AppSettings["MerchId"] ?? "";

        public static string StoreId = System.Configuration.ConfigurationSettings.AppSettings["StoreId"] ?? "";

        public static string StorePassword = string.Empty;

        public static string CurDownload = string.Empty;

        public static string UpgradeServerIP = System.Configuration.ConfigurationSettings.AppSettings["UpgradeServerIP"] ?? "";

        public static string ProcessName = System.Configuration.ConfigurationSettings.AppSettings["ProcessName"] ?? "";
    }
}
