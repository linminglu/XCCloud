using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.XCCloud;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.Business.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// XCCloudMD5Key 的摘要说明
    /// </summary>
    
    public class XCCloudMD5Key : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken, SysIdAndVersionNo = false)]
        public object getMd5TokenByRegWorkStation(Dictionary<string, object> dicParas)
        {
            string dogId = dicParas.Get("dogId").ToString();
            string workStation = dicParas.Get("workStation").ToString();

            string merchId = string.Empty;
            string storeId = string.Empty;
            if (WorkStationBusiness.RegisteWorkStation(dogId, workStation,out merchId,out storeId))
            {
                IData_WorkstationService workstationService = BLLContainer.Resolve<IData_WorkstationService>();
                var wsCount = workstationService.GetModels(p => p.WorkStation.Equals(workStation)).Count();
                if (wsCount > 0)
                {
                    string token = System.Guid.NewGuid().ToString().Replace("-", "");
                    XCCloudMD5KeyBusiness.GetMd5Key(token, merchId, storeId, workStation);
                    var obj = new
                    {
                        token = token
                    };
                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
                }
                else
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "获取token失败");
                }
            }
            else
            {
                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "注册工作站失败");
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken, SysIdAndVersionNo = false)]
        public object getMd5Token(Dictionary<string, object> dicParas)
        {
            string merchId = dicParas.Get("merchId").ToString();
            string storeId = dicParas.Get("storeId").ToString();
            string workStation = dicParas.Get("workStation").ToString();
            string token = System.Guid.NewGuid().ToString().Replace("-", "");
            XCCloudMD5KeyBusiness.GetMd5Key(token, merchId, storeId, workStation);
            var obj = new
            {
                token = token
            };
            return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
        }
    }
}