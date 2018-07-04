using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.Model.CustomModel.Common;
using XCCloudWebBar.Model.CustomModel.XCGame;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using XCCloudWebBar.Model.Socket.UDP;
using XCCloudWebBar.SocketService.UDP;
using XCCloudWebBar.SocketService.UDP.Factory;

namespace XXCloudService.Utility
{
    public class UDPApiService
    {
        public static bool GetParam(string storeId, string requestType, ref ParamQueryResultModel paramQueryResultModel, out string errMsg)
        {
            StoreBusiness storeBusiness = new StoreBusiness();
            StoreCacheModel storeCacheModel = null;
            if (!storeBusiness.IsEffectiveStore(storeId, ref storeCacheModel, out errMsg))
            {
                return false;
            }

            string sn = System.Guid.NewGuid().ToString().Replace("-", "");
            UDPSocketCommonQueryAnswerModel answerModel = null;
            string radarToken = string.Empty;
            if (DataFactory.SendDataParamQuery(sn,storeId, storeCacheModel.StorePassword, requestType,out radarToken,out errMsg))
            {

            }
            else
            {
                return false;
            }

            answerModel = null;
            while (answerModel == null)
            {
                System.Threading.Thread.Sleep(1000);
                answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
            }

            if (answerModel != null)
            {
                ParamQueryResultNotifyRequestModel model = (ParamQueryResultNotifyRequestModel)(answerModel.Result);
                //移除应答缓存数据
                UDPSocketCommonQueryAnswerBusiness.Remove(sn);

                if (model.Result_Code == "1")
                {
                    paramQueryResultModel = model.Result_Data;
                    return true;
                }
                else
                {
                    errMsg = model.Result_Msg;
                    return false;
                }
            }
            else
            {
                errMsg = "系统没有响应";
                return false;
            }
        }

        public static bool UserPhoneQuery(string storeId, string storePassword, string mobile, out string errMsg)
        {
            errMsg = string.Empty;
            string sn = System.Guid.NewGuid().ToString().Replace("-", "");
            UDPSocketCommonQueryAnswerModel answerModel = null;
            string radarToken = string.Empty;
            if (!DataFactory.SendDataUserPhoneQuery(sn, storeId, storePassword, mobile, out radarToken, out errMsg))
            {
                return false;
            }

            answerModel = null;
            int whileCount = 0;
            while (answerModel == null && whileCount <= 25)
            {
                //获取应答缓存数据
                whileCount++;
                System.Threading.Thread.Sleep(1000);
                answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
            }

            if (answerModel != null)
            {
                UserPhoneQueryResultNotifyRequestModel model = (UserPhoneQueryResultNotifyRequestModel)(answerModel.Result);
                //移除应答缓存数据
                UDPSocketCommonQueryAnswerBusiness.Remove(sn);
                if (model.Result_Code == "0")
                {
                    errMsg = "未查询到该用户";
                    return false;
                }
            }
            else
            {
                errMsg = "系统没有响应";
                return false;
            }

            return true;
        }
    }
}