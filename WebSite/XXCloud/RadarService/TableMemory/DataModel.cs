using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace RadarService.TableMemory
{
    public class DataModel
    {
        public bool Add(object o)
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
                        if (v != "")
                        {
                            fields += pi.Name + ",";
                            if (pi.PropertyType.FullName.Contains("DateTime"))
                                values += "'" + Convert.ToDateTime(v).ToString("yyyy-MM-dd HH:mm:ss.fff") + "',";
                            else
                                values += "'" + v.ToString() + "',";
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
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteDBLog(sql);
                throw;
            }
        }

        public string Verifiction(object o)
        {
            try
            {
                SortedDictionary<string, string> list = new SortedDictionary<string, string>();
                Type t = o.GetType();
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    var v = pi.GetValue(o, null);
                    if (v != null)
                    {
                        if (v != "" && pi.Name.ToLower() != "verifiction")
                        {
                            list.Add(pi.Name, v.ToString());
                        }
                    }
                }
                string parametArray = "";
                foreach (string key in list.Keys)
                {
                    parametArray += list[key];
                }
                parametArray += PublicHelper.SystemDefiner.AppSecret;
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
                LogHelper.LogHelper.WriteLog(ex);
                throw;
            }
        }

        public bool Update(object o, string where)
        {
            string sql = "";
            try
            {

                Type t = o.GetType();
                sql = "update " + t.Name + " set ";
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    var v = pi.GetValue(o, null);
                    if (v != null)
                    {
                        if (v != "" && pi.Name != "ID")
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
                DataAccess ac = new DataAccess();
                sql = sql.Substring(0, sql.Length - 1);
                sql += " " + where;
                ac.Execute(sql);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteDBLog(sql);
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
                LogHelper.LogHelper.WriteLog(ex);
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
                        object o = t.Assembly.CreateInstance(t.Name);
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
                LogHelper.LogHelper.WriteLog(ex);
                throw;
            }
        }
    }
}
