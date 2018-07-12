﻿using PalletService.Business.SysConfig;
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
        public static bool Register(string dogId, string workStation, ref object result_data, out string token, ref DogTokenModel dogTokenModel)
        {
            token = string.Empty;
            string errMsg = string.Empty;
            WorkStationRegisterModel model = new WorkStationRegisterModel();
            model.DogId = Computer.DogId;
            model.WorkStation = Computer.WorkStation;
            string url = SysConfigBusiness.XCCloudHost + "/xccloud/Workstation?action=registerWorkStation";
            string param = Utils.DataContractJsonSerializer(model);
            string resultJson = Utils.HttpPost(url, param);
            if (Utils.CheckApiReturnJson(resultJson, ref result_data, out errMsg))
            {
                dogId = Utils.GetJsonObjectValue(result_data, "dogId").ToString();
                workStation = Utils.GetJsonObjectValue(result_data, "workStation").ToString();
                string merchId = Utils.GetJsonObjectValue(result_data, "merchId").ToString();
                string storeId = Utils.GetJsonObjectValue(result_data, "storeId").ToString();
                token = Utils.GetJsonObjectValue(result_data, "token").ToString();
                dogTokenModel = new DogTokenModel(dogId, workStation, merchId, storeId, token);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
