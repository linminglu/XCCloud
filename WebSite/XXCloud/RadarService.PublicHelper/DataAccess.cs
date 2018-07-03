using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.PublicHelper
{
    public class DataAccess
    {        
        /// <summary>
        /// 数据库查询
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns></returns>
        public DataTable ExecuteQueryReturnTable(string strSQL)
        {
            strSQL = strSQL.Replace("`", "");
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(PublicHelper.SystemDefiner.SQLConnectString))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(strSQL, conn))
                    {
                        da.Fill(dt);
                    }
                    conn.Close();
                    conn.Dispose();
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dt;
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>影响条数</returns>
        public int Execute(string strSQL)
        {
            strSQL = strSQL.Replace("`", "");
            int count = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(PublicHelper.SystemDefiner.SQLConnectString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                    {
                        count = cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                    conn.Dispose();
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return count;
        }
        /// <summary>
        /// 数据库查询
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns></returns>
        public DataSet ExecuteQuery(string strSQL)
        {
            strSQL = strSQL.Replace("`", "");
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(PublicHelper.SystemDefiner.SQLConnectString))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(strSQL, conn))
                    {
                        da.Fill(ds);
                    }
                    conn.Close();
                    conn.Dispose();
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }
    }
}
