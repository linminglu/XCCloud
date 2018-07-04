using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.Socket.UDP;
using XXCloudService.Utility;

namespace XXCloudService.Api.XCGame
{
    /// <summary>
    /// Params 的摘要说明
    /// </summary>
    public class Params : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken)]
        public object getStoreParam(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            ParamQueryResultModel paramQueryResultModel = null;
            XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
            if (UDPApiService.GetParam(userTokenModel.StoreId, "0", ref paramQueryResultModel, out errMsg))
            {
                var obj = new {
                    txtCoinPrice = paramQueryResultModel.TxtCoinPrice,
                    txtTicketDate = paramQueryResultModel.TxtTicketDate
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else
            { 
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }  
        }
    }
}