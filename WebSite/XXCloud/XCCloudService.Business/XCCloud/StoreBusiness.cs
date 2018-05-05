using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.XCCloud;

namespace XCCloudService.Business.XCCloud
{
    public class XCCloudStoreBusiness
    {
        public static bool IsEffectiveStore(string storeId)
        {
            XCCloudService.BLL.IBLL.XCCloud.IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCCloud.IBase_StoreInfoService>();
            return base_StoreInfoService.Any(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsSingleStore(string merchId)
        {
            return Base_StoreInfoService.I.GetCount(p => p.MerchID.Equals(merchId)) == 1;
        }

        public static bool IsSingleStore(string merchId, out string storeId)
        {
            storeId = string.Empty;
            if (IsSingleStore(merchId))
            {
                storeId = Base_StoreInfoService.I.GetModels(p => p.MerchID.Equals(merchId)).FirstOrDefault().StoreID;
                return true;
            }

            return false;
        }
    }
}
