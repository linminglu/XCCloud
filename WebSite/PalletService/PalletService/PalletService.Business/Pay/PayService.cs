using PalletService.Common;
using PalletService.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalletService.Business.Pay
{
    public class PayService
    {
        static XCHelper xcHelper;

        public static void PayServiceInit()
        {
            CommonConfig.StorePassword = "778852013145";
            xcHelper = new XCHelper(CommonConfig.XCCloudUDPServiceHost, int.Parse(CommonConfig.XCCloudUDPServicePort), CommonConfig.StoreId, CommonConfig.StorePassword);
            xcHelper.Init();
        }

        public static void ScanPayRequest(string orderId, string authCode)
        {
            xcHelper.ScanPayRequest(orderId, authCode);
        }        
    }
}
