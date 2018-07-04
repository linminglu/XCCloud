using System;
using System.Collections.Generic;
using System.Web;

namespace XCCloudWebBar.Pay.WeiXinPay.Lib
{
    public class WxPayException : Exception 
    {
        public WxPayException(string msg) : base(msg) 
        {

        }
     }
}