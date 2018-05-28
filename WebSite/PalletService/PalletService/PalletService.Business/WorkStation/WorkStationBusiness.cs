using PalletService.Business.SysConfig;
using PalletService.Common;
using PalletService.Model.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalletService.Business.WorkStation
{
    public class WorkStationBusiness
    {
        public static bool Register(WorkStationRegisterModel model, ref object result_data,out string token)
        {
            token = string.Empty;
            string errMsg = string.Empty;
            string url = SysConfigBusiness.XCCloudHost + "/xccloud/XCCloudMD5Key?action=getMd5Token";
            string param = Utils.SerializeObject(model);
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
