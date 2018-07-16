using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{    

    public class XCCloudUserTokenModel
    {
        public XCCloudUserTokenModel(string token, string logId, long endTime, int logType, TokenDataModel dataModel = null)
        {
            this.Token = token;
            this.LogId = logId;
            this.EndTime = endTime;
            this.LogType = logType;
            this.DataModel = dataModel;
        }

        public string Token { get; set; }
        
        public string LogId { set; get; }
        
        public int LogType { set; get; }

        public long EndTime { set; get; }

        public TokenDataModel DataModel { set; get; }
    }

    public class TokenDataModel
    {
        public string MerchID { get; set; }

        public string MerchSecret { get; set; }

        public string StoreID { get; set; }

        public Nullable<int> WorkStationID { get; set; }

        public Nullable<int> MerchType { get; set; }

        public Nullable<int> CreateType { get; set; }

        public string CreateUserID { get; set; }

        public string StorePassword { set; get; }

        public string WorkStation { set; get; }

        public TokenDataModel()
        { }

        public TokenDataModel(string merchId, string storeId, string password, string workStation,int userId,int workStationId)
        {
            this.MerchID = merchId;
            this.StoreID = storeId;
            this.StorePassword = password;
            this.WorkStation = workStation;
            this.CreateUserID = userId.ToString();
            this.WorkStationID = workStationId;
        }
    }

    //public class TokenDataModel : TokenDataModel
    //{
    //    public string MerchID { get; set; }

    //    public string StoreID { get; set; }

    //    public Nullable<int> WorkStationID { get; set; }

    //    public Nullable<int> MerchType { get; set; }

    //    public Nullable<int> CreateType { get; set; }

    //    public string CreateUserID { get; set; }
    //}

    //public class TokenDataModel : TokenDataModel
    //{
    //    public string MerchId { set; get; }

    //    public string StoreId { set; get; }

    //    public string StorePassword { set; get; }

    //    public string WorkStation { set; get; }

    //    public TokenDataModel(string merchId,string storeId, string password, string workStation)
    //    {
    //        this.MerchId = merchId;
    //        this.StoreId = storeId;
    //        this.StorePassword = password;
    //        this.WorkStation = workStation;
    //    }
    //}

}
