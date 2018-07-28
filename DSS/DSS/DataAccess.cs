using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;

namespace DSS
{
    public class DataAccess
    {
        public static string SQLConnectString = "";
        public static string RadarConnectString = "";
        public static string MerchID = "";
        public static string StoreID = "";
        public static string LogPath = "";
        /// <summary>
        /// 数据库查询
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns></returns>
        public DataTable ExecuteQueryReturnTable(string strSQL, string connString = "")
        {
            string curConnString = connString;
            if (connString == "")
                curConnString = SQLConnectString;
            strSQL = strSQL.Replace("`", "");
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(curConnString))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(strSQL, conn))
                    {
                        da.Fill(dt);
                    }
                    conn.Close();
                    //conn.Dispose();
                    //GC.Collect();
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
        public int Execute(string strSQL, string connString = "")
        {
            string curConnString = connString;
            if (connString == "")
                curConnString = SQLConnectString;

            strSQL = strSQL.Replace("`", "");
            int count = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(curConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                    {
                        count = cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                    //conn.Dispose();
                    //GC.Collect();
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
        public DataSet ExecuteQuery(string strSQL, string connString = "")
        {
            string curConnString = connString;
            if (connString == "")
                curConnString = SQLConnectString;

            strSQL = strSQL.Replace("`", "");
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(curConnString))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(strSQL, conn))
                    {
                        da.Fill(ds);
                    }
                    conn.Close();
                    //conn.Dispose();
                    //GC.Collect();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }
        /// <summary>
        /// 同步新增的数据
        /// 来自门店数据新增
        /// </summary>
        /// <param name="idValue">数据索引号</param>
        /// <param name="tableName">数据表名</param>
        /// <returns></returns>
        public bool SyncAddData(string tableName, string jsonText, string secret)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            //获取命名空间准备反射对象
            Type t = Type.GetType("DSS.Table." + tableName);
            //JSON数据格式转换
            object o = jss.Deserialize(jsonText, t);
            DataModel model = new DataModel();
            //计算MD5校验，判断数据是否合法
            if (!model.CheckVerifiction(o, secret)) return false;
            //添加数据
            model.Add(o);
            return true;
        }
        public bool SyncUpdateData(string tableName, string idValue, string jsonText, string secret)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            //获取命名空间准备反射对象
            Type t = Type.GetType("DSS.Table." + tableName);
            //JSON数据格式转换
            object o = jss.Deserialize(jsonText, t);
            DataModel model = new DataModel();
            //计算MD5校验，判断数据是否合法
            if (!model.CheckVerifiction(o, secret)) return false;
            //修改数据
            model.Update(o, "where ID='" + idValue + "'");
            return true;
        }
        public bool SyncDeleteData(string tableName, string idValue)
        {
            DataModel model = new DataModel();
            model.Delete(tableName, "where ID='" + idValue + "'");
            return true;
        }
        public bool SyncExists(string tableName, string idValue)
        {
            string sql = "select * from " + tableName + " where id='" + idValue + "'";
            DataTable dt = ExecuteQueryReturnTable(sql);
            return dt.Rows.Count > 0;
        }
        /// <summary>
        /// 雷达数据接收日志
        /// </summary>
        /// <param name="merchID">商户编号</param>
        /// <param name="storeID">门店编号</param>
        /// <param name="segment">路由器段号</param>
        /// <param name="directType">数据方向 0 接收 1 发送</param>
        /// <param name="data"></param>
        public void RadarDataLog(string merchID, string storeID, string segment, int directType, byte[] data, DateTime createTime)
        {
            string 分库日期 = DateTime.Now.ToString("yyyyMM");
            //检查当前雷达日志分库是否存在
            string sql = "select count(name) from sysobjects where xtype='u' and name='Data_RadarLog_" + 分库日期 + "'";
            DataTable dt = ExecuteQueryReturnTable(sql, RadarConnectString);
            if (dt.Rows.Count == 0)
            {
                //找不到当月的分库时自动创建
                string script = string.Format(@"
create table Data_RadarLog_{0} (
   ID                   int                  identity,
   MerchID              varchar(15)          null,
   StoreID              varchar(15)          null,
   Segment              varchar(10)          null,
   CreateTime           datetime             null,
   DataDirect           int                  null,
   DataContext          binary(500)          null,
   constraint PK_DATA_RADARLOG primary key (ID)
)", 分库日期);
                Execute(script, RadarConnectString);
            }
            sql = string.Format("insert into Data_RadarLog_" + 分库日期 + " values ('{0}','{1}','{2}','{5}',,'{3}',{4})"
                , merchID, storeID, segment, directType, data, createTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                );
            //Execute(sql, RadarConnectString);
        }
    }
}
