using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.DAL.CommonDAL;
using XCCloudService.DBService.DAL;
using XCCloudService.DBService.Model;

namespace XCCloudService.DBService.BLL
{
    public class QueryBLL
    {
        public static void GetInit(string pageName, string processName, int userId, ref List<InitModel> listInitModel, ref List<Dict_SystemModel> listDict_SystemModel)
        {
            QueryDAL.GetInit(pageName, processName, userId, ref listInitModel);

            DataAccess ac = new DataAccess(DataAccessDB.XCCloudDB);            
            foreach(InitModel init in listInitModel)
            {
                if (init.DictID > 0)
                {
                    string sql = string.Format(" select * from Dict_System where PID={0} and ISNULL(MerchID,'')=(select top 1 ISNULL(MerchID,'') from Base_UserInfo where ID={1})", init.DictID, userId);
                    DataSet ds = ac.ExecuteQuery(sql);
                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        sql = string.Format(" select * from Dict_System where PID={0}", init.DictID);
                        ds = ac.ExecuteQuery(sql);
                    }

                    init.List = new List<string>();
                    listDict_SystemModel = Utils.GetModelList<Dict_SystemModel>(ds.Tables[0]);
                    foreach (Dict_SystemModel dict in listDict_SystemModel)
                    {
                        init.List.Add(dict.DictKey);
                    }
                }                
            }
        }

        public static bool GenDynamicSql(object[] conditions, string prefix, ref string sql, ref SqlParameter[] parameters, out string errMsg)
        {
            errMsg = string.Empty;
            if (conditions == null || conditions.Length == 0)
            {
                errMsg = "查询条件为空";
                return false;
            }

            var sb = new StringBuilder();
            var initModel = new InitModel();  
            var alias = !string.IsNullOrEmpty(prefix) ? (prefix.Substring(prefix.Length - 1, 1) == "." ? prefix : (prefix + ".")) : string.Empty;
            foreach (IDictionary<string, object> el in conditions)
            {
                if (el != null)
                {
                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);

                    int id = (dicPara.ContainsKey("id") && Utils.isNumber(dicPara["id"])) ? Convert.ToInt32(dicPara["id"]) : 0;
                    string field = (dicPara.ContainsKey("field") && dicPara["field"] != null) ? dicPara["field"].ToString() : string.Empty;
                    var condition = dicPara.Get("condition").Toint();
                    if (string.IsNullOrEmpty(field))
                    {
                        errMsg = "查询字段不明确";
                        return false;
                    }

                    if (condition.IsNull())
                    {
                        errMsg = "查询条件不明确";
                        return false;
                    }

                    if (!Enum.IsDefined(typeof(QueryTemplateCondition), condition ?? 0))
                    {
                        errMsg = "查询条件为不支持的类型";
                        return false;
                    }

                    string dataType = (dicPara.ContainsKey("dataType") && dicPara["dataType"] != null) ? dicPara["dataType"].ToString() : string.Empty;
                    int dictId = (dicPara.ContainsKey("dictId") && Utils.isNumber(dicPara["dictId"])) ? Convert.ToInt32(dicPara["dictId"]) : -1;
                    if (id > 0)
                    {
                        QueryDAL.GetInitModel(id, field, ref initModel);
                        dataType = initModel.DataType;
                        dictId = initModel.DictID;
                    }                    

                    if (dataType.Equals("literals") || dataType.Equals("bit"))
                    {
                        var values = (dicPara.ContainsKey("values") && dicPara["values"] != null) ? dicPara["values"].ToString() : string.Empty;
                        if (dataType.Equals("literals"))
                        {
                            var dict_SystemModel = Dict_SystemService.I.GetModels(p => p.PID == dictId && p.Enabled == 1 && p.DictKey.Equals(values, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            values = (dict_SystemModel != null) ? dict_SystemModel.DictValue : string.Empty;
                        }

                        if (!values.IsNull())
                        {
                            sb.Append(string.Format(" and {1}{0} = @{0} ", field, alias));
                            Array.Resize(ref parameters, parameters.Length + 1);
                            parameters[parameters.Length - 1] = new SqlParameter("@" + field, values);
                        }                        
                    }                   
                    else
                    {                        
                        if (condition == (int)QueryTemplateCondition.Between)
                        {
                            var values = dicPara.ContainsKey("values") ? (object[])dicPara["values"] : null;
                            if (values != null && values.Length > 0)
                            {
                                if (values.Length >= 1)
                                {
                                    var v0 = values[0];
                                    if (!v0.IsNull())
                                    {
                                        sb.Append(string.Format(" and {1}{0} >= @{0}lower ", field, alias));
                                        Array.Resize(ref parameters, parameters.Length + 1);
                                        parameters[parameters.Length - 1] = new SqlParameter("@" + field + "lower", v0);
                                    }
                                }

                                if (values.Length >= 2)
                                {
                                    var v1 = values[1];
                                    if (!v1.IsNull())
                                    {
                                        sb.Append(string.Format(" and {1}{0} <= @{0}upper ", field, alias));
                                        Array.Resize(ref parameters, parameters.Length + 1);
                                        parameters[parameters.Length - 1] = new SqlParameter("@" + field + "upper", v1);
                                    }
                                }
                            }
                        }
                        else if (condition == (int)QueryTemplateCondition.Like)
                        {
                            var values = (dicPara.ContainsKey("values") && dicPara["values"] != null) ? dicPara["values"].ToString() : string.Empty;
                            if (!values.IsNull())
                            {
                                sb.Append(string.Format(" and {1}{0} like '%' + @{0} + '%' ", field, alias));
                                Array.Resize(ref parameters, parameters.Length + 1);
                                parameters[parameters.Length - 1] = new SqlParameter("@" + field, values);
                            }
                        }
                        else
                        {
                            var values = (dicPara.ContainsKey("values") && dicPara["values"] != null) ? dicPara["values"].ToString() : string.Empty;
                            if (!values.IsNull())
                            {
                                sb.Append(string.Format(" and {1}{0} {2} @{0} ", field, alias, ((QueryTemplateCondition?)condition).GetDescription()));
                                Array.Resize(ref parameters, parameters.Length + 1);
                                parameters[parameters.Length - 1] = new SqlParameter("@" + field, values);
                            }                            
                        }
                    }                                      
                }
                else
                {
                    errMsg = "提交数据包含空对象";
                    return false;
                }
            }

            sql = sql + sb.ToString();

            return true;
        }

        public static bool GenDynamicSql(object[] conditions, string prefix, ref string sql, out string errMsg)
        {
            errMsg = string.Empty;
            if (conditions == null || conditions.Length == 0)
            {
                errMsg = "查询条件为空";
                return false;
            }

            var sb = new StringBuilder();
            var initModel = new InitModel();
            var alias = !string.IsNullOrEmpty(prefix) ? (prefix.Substring(prefix.Length - 1, 1) == "." ? prefix : (prefix + ".")) : string.Empty;
            foreach (IDictionary<string, object> el in conditions)
            {
                if (el != null)
                {
                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);

                    int id = (dicPara.ContainsKey("id") && Utils.isNumber(dicPara["id"])) ? Convert.ToInt32(dicPara["id"]) : 0;
                    string field = (dicPara.ContainsKey("field") && dicPara["field"] != null) ? dicPara["field"].ToString() : string.Empty;                    
                    var condition = dicPara.Get("condition").Toint();
                    if (string.IsNullOrEmpty(field))
                    {
                        errMsg = "查询字段不明确";
                        return false;
                    }

                    if (condition.IsNull())
                    {
                        errMsg = "查询条件不明确";
                        return false;
                    }

                    if (!Enum.IsDefined(typeof(QueryTemplateCondition), condition ?? 0))
                    {
                        errMsg = "查询条件为不支持的类型";
                        return false;
                    }

                    string dataType = (dicPara.ContainsKey("dataType") && dicPara["dataType"] != null) ? dicPara["dataType"].ToString() : string.Empty;
                    int dictId = (dicPara.ContainsKey("dictId") && Utils.isNumber(dicPara["dictId"])) ? Convert.ToInt32(dicPara["dictId"]) : -1;
                    if (id > 0)
                    {
                        QueryDAL.GetInitModel(id, field, ref initModel);
                        dataType = initModel.DataType;
                        dictId = initModel.DictID;
                    }                    

                    if (dataType.Equals("literals") || dataType.Equals("bit"))
                    {
                        var values = (dicPara.ContainsKey("values") && dicPara["values"] != null) ? dicPara["values"].ToString() : string.Empty;
                        if (dataType.Equals("literals"))
                        {
                            var dict_SystemModel = Dict_SystemService.I.GetModels(p => p.PID == dictId && p.Enabled == 1 && p.DictKey.Equals(values, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            values = (dict_SystemModel != null) ? dict_SystemModel.DictValue : string.Empty;
                        }

                        if (!values.IsNull())
                        {
                            sb.Append(string.Format(" and {1}{0} = {2} ", field, alias, values));
                        }                                                
                    }
                    else
                    {
                        if (condition == (int)QueryTemplateCondition.Between)
                        {
                            var values = dicPara.ContainsKey("values") ? (object[])dicPara["values"] : null;
                            if (values != null && values.Length > 0)
                            {
                                if (values.Length >= 1)
                                {
                                    var v0 = values[0];
                                    if (!v0.IsNull())
                                    {
                                        sb.Append(string.Format(" and {1}{0} >= {2} ", field, alias, dataType.Equals("number") ? v0 : ("'" + v0 + "'")));
                                    }
                                }

                                if (values.Length >= 2)
                                {
                                    var v1 = values[1];
                                    if (!v1.IsNull())
                                    {
                                        sb.Append(string.Format(" and {1}{0} <= {2} ", field, alias, dataType.Equals("number") ? v1 : ("'" + v1 + "'")));
                                    }
                                }
                            }
                        }
                        else if (condition == (int)QueryTemplateCondition.Like)
                        {
                            var values = (dicPara.ContainsKey("values") && dicPara["values"] != null) ? dicPara["values"].ToString() : string.Empty;
                            if (!values.IsNull())
                            {
                                sb.Append(string.Format(" and {1}{0} like '%{2}%' ", field, alias, values));
                            } 
                        }
                        else
                        {
                            var values = (dicPara.ContainsKey("values") && dicPara["values"] != null) ? dicPara["values"].ToString() : string.Empty;
                            if (!values.IsNull())
                            {
                                sb.Append(string.Format(" and {1}{0} {2} {3} ", field, alias, ((QueryTemplateCondition?)condition).GetDescription(), dataType.Equals("number") ? values : ("'" + values + "'")));
                            }
                        }
                    }                                       
                }
                else
                {
                    errMsg = "提交数据包含空对象";
                    return false;
                }
            }

            sql = sql + sb.ToString();

            return true;
        }        
    }
}