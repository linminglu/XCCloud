﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common.Enum;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class PaymentModel
    {
    }

    public class PayQRcodeModel
    {
        public string OrderId { get; set; }

        public string QRcode { get; set; }
    }

    public class BarcodePayModel
    {
        public string OrderId { get; set; }

        public string PayAmount { get; set; }

        public OrderState OrderStatus { get; set; }
    }

    public class AlipayMiniAppSignModel
    {
        public string OrderId { get; set; }

        public string PaySign { get; set; }
    }

    public class PposPubSigPay
    {
        public string appId { get; set; }
        public string timeStamp { get; set; }
        public string nonceStr { get; set; }
        public string package { get; set; }
        public string paySign { get; set; }
    }

    public class AliCreateOrderResModel
    {
        public string tradeNO { get; set; }
        public string orderId { get; set; }
    }
}
