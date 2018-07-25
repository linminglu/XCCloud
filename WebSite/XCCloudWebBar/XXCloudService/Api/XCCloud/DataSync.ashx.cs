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
            return null;
        }
    }
}