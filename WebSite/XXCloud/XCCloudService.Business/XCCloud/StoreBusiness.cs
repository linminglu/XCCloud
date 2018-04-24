using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;

namespace XCCloudService.Business.XCCloud
{
    public class XCCloudStoreBusiness
    {
        public static bool IsEffectiveStore(string storeId)
        {
            XCCloudService.BLL.IBLL.XCCloud.IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCCloud.IBase_StoreInfoService>();
            return base_StoreInfoService.Any(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
