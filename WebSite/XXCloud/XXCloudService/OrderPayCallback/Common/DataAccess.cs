using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.OrderPayCallback.Common;

namespace XCCloudService.OrderPayCallback.Common
{
    /// <summary>
    ///DataAccess 的摘要说明
    /// </summary>
    public class DataAccess : IDisposable
    {
        string connString = "";
        SqlConnection conn;

        public DataAccess()
        {
            connString = Config.DecryptDES(ConfigurationManager.AppSettings["connString"], "Xinchen1");
            conn = new SqlConnection(connString);
            conn.Open();
        }

        public DataTable GetTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                PayLogHelper.WriteError(ex);
            }
            return dt;
        }

        public int Execute(string sql)
        {
            try
            {
                return new SqlCommand(sql, conn).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                PayLogHelper.WriteError(ex, sql);
                return 0;
            }
        }

        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
            conn = null;

            GC.Collect();
        }
    }
}