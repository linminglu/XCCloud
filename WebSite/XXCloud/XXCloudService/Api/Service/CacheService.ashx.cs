using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.Utility;

namespace XXCloudService.Api.Service
{
    /// <summary>
    /// CacheService 的摘要说明
    /// </summary>
    public class CacheService : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCGameManamAdminUserToken, SysIdAndVersionNo = false)]
        public object reloadCacheData(Dictionary<string, object> dicParas)
        {
            try
            {
                string index = dicParas.ContainsKey("index") ? dicParas["index"].ToString() : string.Empty;
                reloadSingleCacheData(int.Parse(index));
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        private void reloadSingleCacheData(int cacheDataIndex)
        {
            switch (cacheDataIndex)
            {
                case 1: ApplicationStart.StoreInit(); break;
                case 2: ApplicationStart.StoreDogInit(); break;
                case 3: ApplicationStart.MibleTokenInit(); break;
                case 4: ApplicationStart.MemberTokenInit(); break;
                case 5: ApplicationStart.RS232MibleTokenInit(); break;
                case 6: ApplicationStart.XCCloudUserInit(); break;
                case 7: ApplicationStart.XCGameManaDeviceInit(); break;
                case 8: ApplicationStart.XCCloudManaUserInit(); break;
                case 9: ApplicationStart.FilterMobileInit(); break;
                case 10: ApplicationStart.XinchenPayInit(); break;
                default: break;
            }
        }
    }
}