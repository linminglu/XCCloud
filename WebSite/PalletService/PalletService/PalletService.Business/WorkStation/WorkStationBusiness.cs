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
        public static bool Register(string dogId,string workStation, ref object result_data,out string token)
        {
            token = string.Empty;
            string errMsg = string.Empty;
            WorkStationRegisterModel model = new WorkStationRegisterModel();
            model.DogId = Computer.DogId;
            model.WorkStation = Computer.WorkStation;
            string url = SysConfigBusiness.XCCloudHost + "/xccloud/Workstation?action=AddWorkStation";
            string param = Utils.DataContractJsonSerializer(model);
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
