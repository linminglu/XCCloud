﻿using PalletService.Business.SysConfig;
using PalletService.Common;
using PalletService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalletService.Business.Dog
{
    public class DogBusiness
    {
        public static bool GetMD5Token(string merchId,string storeId,out string token,out string errMsg)
        {
            token = string.Empty;
            DogMD5RequestModel requestModel = new DogMD5RequestModel();
            requestModel.MerchID = merchId;
            requestModel.StoreID = storeId;

            object result_data = new object();
            string url = SysConfigBusiness.XCCloudHost + "/xccloud/XCCloudMD5Key?action=getMd5Token";
            string param = Utils.SerializeObject(requestModel);
            string resultJson = Utils.HttpPost(url, param);

            if (Utils.CheckApiReturnJson(resultJson, ref result_data, out errMsg))
            {
                object repeatCodeObj = Utils.GetJsonObjectValue(result_data, "token");
                token = repeatCodeObj.ToString();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
