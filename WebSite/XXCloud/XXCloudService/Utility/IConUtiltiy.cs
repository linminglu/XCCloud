using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Business.Common;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.Socket.UDP;
using XCCloudService.SocketService.UDP.Factory;

namespace XXCloudService.Utility
{
    public class IConUtiltiy
    {
        /// <summary>
        /// 出币
        /// </summary>
        /// <returns></returns>
        public static bool DeviceOutputCoin(XCGameManaDeviceStoreType deviceStoreType, DevieControlTypeEnum deviceControlType,string storeId, string mobile, int icCardId, string orderId, string segment, string mcuId, string storePassword, int foodId, int coins, string ruleId,out string errMsg)
        {
            errMsg = string.Empty;
            if (deviceStoreType == XCGameManaDeviceStoreType.Store || deviceStoreType == XCGameManaDeviceStoreType.Merch)
            {
                string action = ((int)(deviceControlType)).ToString();
                string sn = UDPSocketAnswerBusiness.GetSN();
                if (string.IsNullOrEmpty(orderId))
                {
                    orderId = System.Guid.NewGuid().ToString("N");
                }
                DeviceControlRequestDataModel deviceControlModel = new DeviceControlRequestDataModel(storeId, mobile, icCardId.ToString(), segment, mcuId, action, coins, sn, orderId, storePassword, foodId, ruleId,"");
                MPOrderBusiness.AddTCPAnswerOrder(orderId, mobile, coins, action, "", storeId);
                IconOutLockBusiness.Add(mobile, coins);
                if (!DataFactory.SendDataToRadar(deviceControlModel, out errMsg))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                errMsg = "门店类型不正确";
                return false;
            }
        }

        /// <summary>
        /// 远程投币
        /// </summary>
        /// <returns></returns>
        public static bool RemoteDeviceCoinIn(XCGameManaDeviceStoreType deviceStoreType, DevieControlTypeEnum deviceControlType, string icCardId, string orderId, string mcuId, string storePassword, string count, string ruleId, string ruleType, out string errMsg)
        {
            errMsg = string.Empty;
            if (deviceStoreType == XCGameManaDeviceStoreType.Store || deviceStoreType == XCGameManaDeviceStoreType.Merch)
            {
                DeviceStateCacheModel deviceStateModel = RedisCacheHelper.HashGet<DeviceStateCacheModel>(CommonConfig.DeviceStateKey, mcuId);
                //验证雷达是否上线
                if (deviceStateModel == null)
                {
                    errMsg = "雷达未上线";
                    return false;
                }
                if (deviceStateModel.State == "0")
                {
                    errMsg = "设备不在线";
                    return false;
                }

                string action = ((int)(deviceControlType)).ToString();
                string sn = UDPSocketAnswerBusiness.GetSN();
                if (string.IsNullOrEmpty(orderId))
                {
                    orderId = System.Guid.NewGuid().ToString("N");
                }
                RemoteDeviceControlRequestDataModel deviceControlModel = new RemoteDeviceControlRequestDataModel(deviceStateModel.Token, mcuId, icCardId, action, ruleType, ruleId, count, orderId, sn);
                //MPOrderBusiness.AddTCPAnswerOrder(orderId, mobile, count, action, "", storeId);
                //IconOutLockBusiness.Add(mobile, count);                
                if (!DataFactory.SendCoinInDataToRadar(deviceControlModel, storePassword, out errMsg))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                errMsg = "门店类型不正确";
                return false;
            }
        }
    }
}