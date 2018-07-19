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
                            if (pi.Name.ToLower() == "id" && t.Name.ToLower() == "sync_datalist") continue;
                            fields += pi.Name + ",";
                            if (pi.PropertyType.FullName.Contains("DateTime"))
                                values += "'" + (Convert.ToDateTime(v) == DateTime.MinValue ? "" : Convert.ToDateTime(v).ToString("yyyy-MM-dd HH:mm:ss.fff")) + "',";
                            else
                                values += "'" + v.ToString() + "',";
                        }
                    }
                }
                DataAccess ac = new DataAccess();
                fields = fields.Substring(0, fields.Length - 1);
                values = values.Substring(0, values.Length - 1);
                sql += "insert into " + t.Name + " (" + fields + ") values (" + values + ") \r\n";
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
                                if (pi.PropertyType.FullName.Contains("DateTime"))
                                    sql += pi.Name + "='" + Convert.ToDateTime(v).ToString("yyyy-MM-dd HH:mm:ss.fff") + "',";
                                else
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
            if (data != "0")
            {
                string v = data.TrimEnd('0');
                if (v.Substring(v.Length - 1, 1) == ".")
                    v = v.Substring(0, v.Length - 1);
                return v;
            }
            return data;
        }
        public bool CheckVerifiction(object o, string secret, bool Identity = false)
        {
            try
            {
                string vs = "";
                bool NeedVerifaction = ContainProperty(o, "verifiction");
                if (!NeedVerifaction) return true;
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
                            string typeName = GetObjectTypeRootName(pi);
                            if (v.ToString() != "" && pi.Name.ToLower() != "verifiction")
                            {
                                if (typeName == "decimal")
                                {
                                    list.Add(pi.Name, CovertDecimalValue(v.ToString()));
                                }
                                else if (typeName == "datetime")
                                    list.Add(pi.Name, Convert.ToDateTime(v).ToString("yyyy-MM-dd HH:mm:ss.fff"));
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
                bool NeedVerifaction = ContainProperty(o, "verifiction");
                if (!NeedVerifaction) return "";
                SortedDictionary<string, string> list = new SortedDictionary<string, string>();
                Type t = o.GetType();

                foreach (PropertyInfo pi in t.GetProperties())
                {
                    if (pi.Name.ToLower() == "verifiction")
                        NeedVerifaction = true;
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
                            string typename = GetObjectTypeRootName(pi);
                            switch (typename)
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
                                string typeName = GetObjectTypeRootName(pi);
                                switch (typeName)
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
        string GetObjectTypeRootName(PropertyInfo pi)
        {
            string typename = "";
            if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                typename = pi.PropertyType.GetGenericArguments()[0].Name.ToLower(); //如果数据类型可为空时，获取起根数据类型
            else
                typename = pi.PropertyType.Name.ToLower();//正常情况获取
            return typename;
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
        /// <summary>
        /// 判断对象是否包含指定属性名
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        bool ContainProperty(object instance, string propertyName)
        {
            if (instance != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyInfo _findedPropertyInfo = instance.GetType().GetProperty(propertyName);
                return (_findedPropertyInfo != null);
            }
            return false;
        }
    }
}
