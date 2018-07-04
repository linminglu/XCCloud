using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.CacheService;

namespace XCCloudWebBar.Business.XCCloud
{
    public class XCCloudMD5KeyBusiness
    {
        public static string GetMd5Key(string token, string merchId, string storeId, string workStation)
        {
            XCCloudMD5KeyCache.AddToken(token,merchId,storeId,workStation);
            return XCCloudMD5KeyCache.GetTokenModel(token).Token;
        }
    }
}
