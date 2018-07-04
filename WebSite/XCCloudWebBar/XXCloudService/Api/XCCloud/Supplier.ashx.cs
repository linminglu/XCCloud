using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.DBService.BLL;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser, StoreUser")]
    /// <summary>
    /// GoodsInfo 的摘要说明
    /// </summary>
    public class Supplier : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetSupplierDic(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                IData_SupplierListService data_SupplierListService = BLLContainer.Resolve<IData_SupplierListService>();
                IQueryable<Data_SupplierList> query = null;
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    query = data_SupplierListService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    query = data_SupplierListService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }

                var result = from a in query                            
                             orderby a.ID
                             select new
                             {
                                 ID = a.ID,
                                 Supplier = a.Supplier
                             };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }


    }
}