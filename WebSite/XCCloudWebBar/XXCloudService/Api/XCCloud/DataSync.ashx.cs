using DSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// DataSync 的摘要说明
    /// </summary>
    public class DataSync : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object dataSync(Dictionary<string, object> dicParas)
        {
            System.Threading.Thread t = new System.Threading.Thread(dataSync);
            t.Start();
            return new ResponseModel(Return_Code.T, "", Result_Code.F, "");    
        }


        private void dataSync()
        {
            DataModel model = new DataModel();
            List<object> dataList = new List<object>();
            if (model.CovertToDataModel("select * from Sync_DataList where SyncFlag=0", typeof(DSS.Table.Sync_DataList), out dataList))
            {
                foreach (DSS.Table.Sync_DataList o in dataList)
                {
                    XCCloudService.SyncService.UDP.Client.StoreDataSync(o.TableName, o.IDValue, (int)o.SyncType, false, o.SN);
                }
            }
        }
    }
}