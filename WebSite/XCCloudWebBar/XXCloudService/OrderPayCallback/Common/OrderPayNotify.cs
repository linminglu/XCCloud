﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.CacheService.XCCloud;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.SocketService.UDP;
using XCCloudWebBar.SocketService.UDP.Factory;

namespace XCCloudWebBar.PayChannel.Common
{
    public class OrderPayNotify
    {
        public static void AddOrderPayCache(string orderId, decimal amount, string payTime, OrderState payState)
        {
            OrderPayCacheModel orderPay = new OrderPayCacheModel();
            orderPay.OrderId = orderId;
            orderPay.PayAmount = amount;
            orderPay.PayTime = payTime;
            orderPay.PayState = payState;
            OrderPayCache.Add(orderId, orderPay, CacheExpires.OrderPayCacheExpiresTime);
        }

        /// <summary>
        /// 发送支付通知
        /// </summary>
        public static void SendNotify(OrderPayCacheModel model)
        {
            ClientService service = new ClientService();
            service.Connection();
            //DeviceControlResponseModel dataModel = new DeviceControlResponseModel(result_code, result_msg, sn, signkey);
            byte[] data = DataFactory.CreateRequesProtocolData(TransmiteEnum.远程设备控制指令响应, model);
            service.Send(data);
        }
    }
}