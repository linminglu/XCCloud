﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{    

    public class XCCloudUserTokenModel
    {
        public XCCloudUserTokenModel(string logId, long endTime, int logType, TokenDataModel dataModel = null)
        {
            this.LogId = logId;
            this.EndTime = endTime;
            this.LogType = logType;
            this.DataModel = dataModel;
        }
        
        public string LogId { set; get; }
        
        public int LogType { set; get; }

        public long EndTime { set; get; }

        public TokenDataModel DataModel { set; get; }
    }

    public class TokenDataModel
    {        
 
    }

    public class MerchDataModel : TokenDataModel
    {
        public string MerchID { get; set; }

        public string StoreID { get; set; }

        public Nullable<int> WorkStationID { get; set; }

        public Nullable<int> MerchType { get; set; }

        public Nullable<int> CreateType { get; set; }

        public string CreateUserID { get; set; }
    }

    public class StoreIDDataModel:TokenDataModel
    {
        public string MerchId { set; get; }

        public string StoreId { set; get; }

        public string StorePassword { set; get; }

        public string WorkStation { set; get; }

        public StoreIDDataModel(string merchId,string storeId, string password, string workStation)
        {
            this.MerchId = merchId;
            this.StoreId = storeId;
            this.StorePassword = password;
            this.WorkStation = workStation;
        }
    }

}
