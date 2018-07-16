using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSS
{
    public class DataModel
    {
        public bool Add(object o, bool Identity = false)
        {
            string sql = "";
            try
            {
                string fields = "";
                string values = "";
                Type t = o.GetType();
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    var v = pi.GetValue(o, null);
                    if (v != null)
                    {
                        if (v.ToString() != "")
                        {
                            if (Identity && pi.Name.ToLower() == "id")
                            { }
                            else
                            {
                                fields += pi.Name + ",";
                                if (pi.PropertyType.FullName.Contains("DateTime"))
                                    values += "'" + Convert.ToDateTime(v).ToString("yyyy-MM-dd HH:mm:ss.fff") + "',";
                                else
                                    values += "'" + v.ToString() + "',";
                            }
                        }
                    }
                }
                DataAccess ac = new DataAccess();
                fields = fields.Substring(0, fields.Length - 1);
                values = values.Substring(0, values.Length - 1);
                sql = "insert into " + t.Name + " (" + fields + ") values (" + values + ")";
                ac.Execute(sql);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                WriteDBLog(sql);
                throw;
            }
        }
        public bool Update(object o, string where, bool Identity = false)
        {
            string sql = "";
            try
            {

                Type t = o.GetType();
                sql = "update " + t.Name + " set ";
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    if (Identity && pi.Name.ToLower() == "id")
                    { }
                    else
                    {
                        var v = pi.GetValue(o, null);
                        if (v != null)
                        {
                            if (v.ToString() != "" && pi.Name != "ID")
                            {
                                sql += pi.Name + "='" + v.ToString() + "',";
                            }
                        }
                        else
                        {
                            if (pi.PropertyType.Name.ToLower().Contains("time") || pi.PropertyType.Name.ToLower().Contains("date"))
                                sql += pi.Name + "is null,";
                        }
                    }
                }
                DataAccess ac = new DataAccess();
                sql = sql.Substring(0, sql.Length - 1);
                sql += " " + where;
                ac.Execute(sql);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                WriteDBLog(sql);
                throw;
            }
        }
        public bool Delete(string tableName, string where)
        {
            DataAccess ac = new DataAccess();
            string sql = "delete from " + tableName + " " + where;
            ac.Execute(sql);
            return true;
        }
        /// <summary>
        /// 当数值为Decimal类型时，将尾部的0去掉
        /// 取完0后，末尾为小数点则小数点也去掉
        /// 1.00转换为1
        /// 1.10转换为1.1
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string CovertDecimalValue(string data)
        {
            string v = data.TrimEnd('0');
            if (v.Substring(v.Length - 1, 1) == ".")
                v = v.Substring(0, v.Length - 1);
            return v;
        }
        public bool CheckVerifiction(object o, string secret, bool Identity = false)
        {
            try
            {
                string vs = "";
                SortedDictionary<string, string> list = new SortedDictionary<string, string>();
                Type t = o.GetType();
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    if (Identity && pi.Name.ToLower() == "id")
                    { }
                    else
                    {
                        var v = pi.GetValue(o, null);
                        if (v != null)
                        {
                            if (v.ToString() != "" && pi.Name.ToLower() != "verifiction")
                            {
                                if (pi.PropertyType.Name == "Decimal")
                                {
                                    list.Add(pi.Name, CovertDecimalValue(v.ToString()));
                                }
                                else
                                    list.Add(pi.Name, v.ToString());
                            }
                            else if (pi.Name.ToLower() == "verifiction")
                                vs = v.ToString();
                        }
                    }
                }
                string parametArray = "";
                foreach (string key in list.Keys)
                {
                    parametArray += list[key];
                }
                parametArray += secret;
                var md5 = MD5.Create();
                var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(parametArray));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }

                return (sb.ToString() == vs);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }
        public string Verifiction(object o, string secret, bool Identity = false)
        {
            try
            {
                SortedDictionary<string, string> list = new SortedDictionary<string, string>();
                Type t = o.GetType();
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    if (Identity && pi.Name.ToLower() == "id")
                    { }
                    else
                    {
                        var v = pi.GetValue(o, null);
                        if (v != null)
                        {
                            if (v.ToString() != "" && pi.Name.ToLower() != "verifiction")
                            {
                                list.Add(pi.Name, v.ToString());
                            }
                        }
                    }
                }
                string parametArray = "";
                foreach (string key in list.Keys)
                {
                    parametArray += list[key];
                }
                parametArray += secret;
                var md5 = MD5.Create();
                var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(parametArray));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }
        public bool CovertToDataModel(string sql, ref object o)
        {
            try
            {
                DataAccess ac = new DataAccess();
                DataTable dt = ac.ExecuteQueryReturnTable(sql);
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    Type t = o.GetType();
                    foreach (PropertyInfo pi in t.GetProperties())
                    {
                        if (row[pi.Name] != DBNull.Value)
                        {
                            switch (pi.PropertyType.Name.ToLower())
                            {
                                case "string":
                                    pi.SetValue(o, row[pi.Name].ToString(), null);
                                    break;
                                case "int32":
                                case "int":
                                    pi.SetValue(o, Convert.ToInt32(row[pi.Name]), null);
                                    break;
                                case "datetime":
                                    pi.SetValue(o, Convert.ToDateTime(row[pi.Name]), null);
                                    break;
                                case "decimal":
                                    pi.SetValue(o, Convert.ToDecimal(row[pi.Name]), null);
                                    break;
                                default:
                                    if (pi.PropertyType.FullName.Contains("DateTime"))
                                        pi.SetValue(o, Convert.ToDateTime(row[pi.Name]), null);
                                    else
                                        pi.SetValue(o, row[pi.Name].ToString(), null);
                                    break;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }
        public bool CovertToDataModel(string sql, Type t, out List<object> outList)
        {
            outList = new List<object>();
            try
            {
                DataAccess ac = new DataAccess();
                DataTable dt = ac.ExecuteQueryReturnTable(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        object o = t.Assembly.CreateInstance(t.FullName);
                        foreach (PropertyInfo pi in t.GetProperties())
                        {
                            if (row[pi.Name] != DBNull.Value)
                            {
                                switch (pi.PropertyType.Name.ToLower())
                                {
                                    case "string":
                                        pi.SetValue(o, row[pi.Name].ToString(), null);
                                        break;
                                    case "int32":
                                    case "int":
                                        pi.SetValue(o, Convert.ToInt32(row[pi.Name]), null);
                                        break;
                                    case "datetime":
                                        pi.SetValue(o, Convert.ToDateTime(row[pi.Name]), null);
                                        break;
                                    case "decimal":
                                        pi.SetValue(o, Convert.ToDecimal(row[pi.Name]), null);
                                        break;
                                    default:
                                        if (pi.PropertyType.FullName.Contains("DateTime"))
                                            pi.SetValue(o, Convert.ToDateTime(row[pi.Name]), null);
                                        else
                                            pi.SetValue(o, row[pi.Name].ToString(), null);
                                        break;
                                }
                            }
                        }
                        outList.Add(o);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }
        void WriteLog(Exception ex)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(DataAccess.LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("错误时间：" + d.ToString());
                sw.WriteLine("错误内容：" + ex.Message);
                sw.WriteLine("堆栈信息：" + ex.StackTrace);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }
        void WriteDBLog(string sql)
        {
            DateTime d = DateTime.Now;
            string filePath = "";
            string fileName = "";

            filePath = string.Format(DataAccess.LogPath + "\\{0}\\{1}", d.ToString("yyyy"), d.ToString("yyyy-MM"));
            fileName = d.ToString("yyyy-MM-dd") + "db.txt";
            try
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, true, Encoding.GetEncoding("gb2312"));
                sw.WriteLine("执行时间：" + d.ToString());
                sw.WriteLine("执行语句：" + sql);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }
    }
}
