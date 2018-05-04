using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.CommonBLL;

namespace XCCloudService.Business.XCCloud
{
    public class RoleBusiness
    {
        public static bool CheckRoleByOpenId(string storeId,string openId,string roleItemName,out int userId,out string errMsg )
        {
            userId = 0;
            errMsg = string.Empty;
            string storedProcedure = "CheckRoleByOpenId";
            SqlParameter[] sqlParameter = new SqlParameter[6];
            sqlParameter[0] = new SqlParameter("@StoreID", SqlDbType.VarChar);
            sqlParameter[0].Value = storeId;
            sqlParameter[1] = new SqlParameter("@OpenId", SqlDbType.VarChar);
            sqlParameter[1].Value = openId;
            sqlParameter[2] = new SqlParameter("@RoleItemName", SqlDbType.VarChar);
            sqlParameter[2].Value = roleItemName;
            sqlParameter[3] = new SqlParameter("@UserId", SqlDbType.Int);
            sqlParameter[3].Direction = ParameterDirection.Output;
            sqlParameter[4] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
            sqlParameter[4].Direction = ParameterDirection.Output;
            sqlParameter[5] = new SqlParameter("@Return", SqlDbType.Int);
            sqlParameter[5].Direction = ParameterDirection.ReturnValue;

            XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, sqlParameter);
            if (sqlParameter[5].Value.ToString() == "1")
            {
                userId = Convert.ToInt32(sqlParameter[3].Value);
                return true;
            }
            else
            {
                errMsg = sqlParameter[4].Value.ToString();
                return false;
            }
        }
    }
}
