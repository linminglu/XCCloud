﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Reflection;

namespace DSS
{
    public class DataAccess
    {
        public static string SQLConnectString = "";
        public static string MerchID = "";
        public static string StoreID = "";
        public static string LogPath = "";
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
                using (SqlConnection conn = new SqlConnection(SQLConnectString))
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
                using (SqlConnection conn = new SqlConnection(SQLConnectString))
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
                using (SqlConnection conn = new SqlConnection(SQLConnectString))
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
        /// <summary>
        /// JSON传输时会造成时区差8小时
        /// </summary>
        /// <param name="o"></param>
        void GMTCheck(ref object o)
        {
            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.PropertyType.Name.ToLower() == "datetime")
                {
                    var v = pi.GetValue(o, null);
                    if (v != null)
                        pi.SetValue(o, ((DateTime)v).AddHours(8), null);
                }
            }
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
            GMTCheck(ref o);
            bool identity = GetTableIdentity(o);
            DataModel model = new DataModel();
            //计算MD5校验，判断数据是否合法
            if (!model.CheckVerifiction(o, secret, identity)) return false;
            //添加数据
            model.Add(o, identity);
            return true;
        }
        public bool SyncUpdateData(string tableName, string idValue, string jsonText, string secret)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            //获取命名空间准备反射对象
            Type t = Type.GetType("DSS.Table." + tableName);
            //JSON数据格式转换
            object o = jss.Deserialize(jsonText, t);
            GMTCheck(ref o);
            bool identity = GetTableIdentity(o);
            DataModel model = new DataModel();
            //计算MD5校验，判断数据是否合法
            if (!model.CheckVerifiction(o, secret, identity)) return false;
            //修改数据
            model.Update(o, "where id='" + idValue + "'", identity);
            return true;
        }
        public bool SyncDeleteData(string tableName, string idValue)
        {
            DataModel model = new DataModel();
            model.Delete(tableName, idValue);
            return true;
        }

        bool GetTableIdentity(object o)
        {
            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.Name.ToLower() == "id")
                {
                    return pi.PropertyType.Name.ToLower().Contains("int");
                }
            }
            return false;
        }
    }
}
