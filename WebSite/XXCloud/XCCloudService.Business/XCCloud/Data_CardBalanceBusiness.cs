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
    public class Data_CardBalanceBusiness
    {
        public bool UpdateBalance(decimal balance, int balanceIndex)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update Data_Card_Balance set ");
            strSql.Append("Banlance=@Banlance,");
            strSql.Append("UpdateTime=@UpdateTime");
            strSql.Append(" where BalanceIndex=@BalanceIndex");
            SqlParameter[] parameters = {
					new SqlParameter("@Banlance", SqlDbType.Decimal,9),
                    new SqlParameter("@UpdateTime", SqlDbType.DateTime),
					new SqlParameter("@BalanceIndex", SqlDbType.Int,4)};

            parameters[0].Value = balance;
            parameters[1].Value = DateTime.Now;
            parameters[2].Value = balanceIndex;

            int rows = XCCloudBLL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
