using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Pay;

namespace XXCloudService.Api.XCGameMana
{
    /// <summary>
    /// Order 的摘要说明
    /// </summary>
    public class Order : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MobileToken)]
        public object createOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                int coins = 0;
                int orderType = 0;
                decimal payPrice = 0;
                string orderNo = string.Empty;
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string productName = dicParas.ContainsKey("productName") ? dicParas["productName"].ToString() : string.Empty;
                string payPriceStr = dicParas.ContainsKey("payPrice") ? dicParas["payPrice"].ToString() : string.Empty;
                string buyType = dicParas.ContainsKey("buyType") ? dicParas["buyType"].ToString() : string.Empty;
                string coinsStr = dicParas.ContainsKey("coins") ? dicParas["coins"].ToString() : string.Empty;
                string orderTypeStr = dicParas.ContainsKey("orderType") ? dicParas["orderType"].ToString() : string.Empty;

                MobileTokenModel mobileTokenModel = (MobileTokenModel)(dicParas[Constant.MobileTokenModel]);

                if (!decimal.TryParse(payPriceStr, out payPrice))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付金额不正确");
                }

                if (!int.TryParse(coinsStr, out coins))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "购买币数不正确");
                }

                if (!int.TryParse(orderTypeStr, out orderType))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付类型不正确");
                }

                //生成服务器订单号
                orderNo = PayOrderHelper.CreateXCGameOrderNo(storeId, payPrice, 0, orderType, productName, mobileTokenModel.Mobile, buyType, coins);

                var dataObj = new
                {
                    orderNo = orderNo
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, dataObj);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}