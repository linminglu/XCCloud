using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser,MerchUser")]
    /// <summary>
    /// Workstation 的摘要说明
    /// </summary>
    public class Workstation : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryWorkstation(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string sql = @"select a.* from Data_Workstation a where a.StoreID='" + storeId + "'";
                sql = sql + sqlWhere;
                IData_WorkstationService data_WorkstationService = BLLContainer.Resolve<IData_WorkstationService>();
                var data_Workstation = data_WorkstationService.SqlQuery(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_Workstation);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetWorkstationDic(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? Convert.ToString(dicParas["storeId"]) : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "storeId参数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IData_WorkstationService data_WorkstationService = BLLContainer.Resolve<IData_WorkstationService>();
                var workstationList = data_WorkstationService.GetModels(p=>p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                    .Select(o => new { ID = o.ID, WorkStation = o.WorkStation }).Distinct()
                    .ToDictionary(d => d.ID, d => d.WorkStation);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, workstationList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}