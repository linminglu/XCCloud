using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Business.XCGameMana;

namespace XCCloudWebBar.Pay
{
    public class PayOrderHelper
    {
        public static string CreateXCGameOrderNo(string storeId, decimal price, decimal fee, int orderType, string productName, string mobile, string buyType, int coins)
        {
            string orderNo = MPOrderBusiness.GetOrderNo(storeId, price, fee, orderType, productName, mobile, buyType, coins);
            return orderNo;
        }
    }
}
