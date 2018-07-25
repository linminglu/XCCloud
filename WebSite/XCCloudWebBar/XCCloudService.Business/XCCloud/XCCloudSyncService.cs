using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.CacheService.XCCloud;
using XCCloudWebBar.Common;

namespace XCCloudService.Business.XCCloud
{
    public class XCCloudSyncService
    {
        public static void Init()
        {
            if (StoreBusinessCache.storeInfo != null)
            {
                string secret = StoreBusinessCache.storeInfo.Password;
                XCCloudService.SyncService.UDP.Client.Init(CommonConfig.DataSyncServerIP, int.Parse(CommonConfig.DataSyncServerPort), CommonConfig.StoreId, CommonConfig.MerchId, secret);                
            }
        }
    }
}
