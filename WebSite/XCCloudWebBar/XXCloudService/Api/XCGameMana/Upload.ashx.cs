using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;

namespace XXCloudService.Api.XCGameMana
{
    /// <summary>
    /// PhotoUpload 的摘要说明
    /// </summary>
    public class Upload : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MobileToken,SysIdAndVersionNo=false)]
        #region "会员图片上传"
        public object upload(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string url = string.Empty;
            string fileType = string.Empty;
            string clientType = dicParas.ContainsKey("clientType") ? dicParas["clientType"].ToString() : string.Empty;

            if (FileUploadBusiness.SaveFile(HttpContext.Current.Request.Files[0], clientType,out fileType, out url,out errMsg))
            {
                var obj = new { url = url };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }
        }
        #endregion
        
    }
}