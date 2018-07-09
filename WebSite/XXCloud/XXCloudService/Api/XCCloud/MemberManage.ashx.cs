using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// MemberManage 的摘要说明
    /// </summary>
    public class MemberManage : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@MerchID", merchId);
                parameters[1] = new SqlParameter("@StoreID", storeId);

                string sqlWhere = string.Empty;
                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string storedProcedure = "QueryMemberInfo";
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@SqlWhere", sqlWhere);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
                if (parameters[parameters.Length - 1].Value.ToString() != "1")
                {
                    errMsg = "查询会员档案数据失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                var json = string.Empty;
                if (ds.Tables.Count > 0)
                {
                    json = Utils.DataTableToJson(ds.Tables[0]);
                }

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, json);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}