﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.DAL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Common.Extensions;
using System.Data;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// Dictionary 的摘要说明
    /// </summary>
    public class Dictionary : ApiBase
    {
        private bool checkParams(Dictionary<string, object> dicParas, out string errMsg)
        {
            errMsg = string.Empty;
            string dictKey = dicParas.ContainsKey("dictKey") ? dicParas["dictKey"].ToString() : string.Empty;
            string dictValue = dicParas.ContainsKey("dictValue") ? dicParas["dictValue"].ToString() : string.Empty;
            string comment = dicParas.ContainsKey("comment") ? dicParas["comment"].ToString() : string.Empty;
            string enabled = dicParas.ContainsKey("enabled") ? dicParas["enabled"].ToString() : string.Empty;
            string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(dictKey))
            {
                errMsg = "节点名称不能为空";
                return false;
            }

            if (dictKey.Length > 50)
            {
                errMsg = "节点名称不能超过50个字符";
                return false;
            }

            if (!string.IsNullOrEmpty(dictValue) && dictValue.Length > 50)
            {
                errMsg = "节点值不能超过50个字符";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(comment) && comment.Length > 500)
            {
                errMsg = "说明不能超过500个字符";
                return false;
            }

            if (string.IsNullOrEmpty(enabled))
            {
                errMsg = "使用状态不能为空";
                return false;
            }
            try
            {
                Convert.ToInt32(enabled);
            }
            catch
            {
                errMsg = "使用状态数据格式转换异常";
                return false;
            }

            //if (!orderId.Validint("节点序号", out errMsg))
            //    return false;

            return true;
        }        

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetNodes(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string merchId = dicParas.Get("merchId");
                string dictKey = dicParas.Get("dictKey");
                string enabled = dicParas.Get("enabled");
                var pDictKey = dicParas.Get("pDictKey");

                string sql = " exec  SP_DictionaryNodes @MerchID,@DictKey,@PDictKey,@RootID output ";
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@MerchID", merchId);
                parameters[1] = new SqlParameter("@DictKey", dictKey);
                parameters[2] = new SqlParameter("@PDictKey", pDictKey);
                parameters[3] = new SqlParameter("@RootID", SqlDbType.Int);
                parameters[3].Direction = System.Data.ParameterDirection.Output;
                System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
                if (ds.Tables.Count == 0)
                {
                    errMsg = "没有找到节点信息";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                var dictionaryResponse = Utils.GetModelList<DictionaryResponseModel>(ds.Tables[0]);
                if (!enabled.IsNull())
                {
                    dictionaryResponse = dictionaryResponse.Where(w => w.Enabled == enabled.Toint()).ToList();
                }

                int rootId = 0;
                int.TryParse(parameters[3].Value.ToString(), out rootId);

                //实例化一个根节点
                DictionaryResponseModel rootRoot = new DictionaryResponseModel();
                rootRoot.ID = rootId;
                TreeHelper.LoopToAppendChildren(dictionaryResponse, rootRoot);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, rootRoot);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }            
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object GetNodesFromProgram(Dictionary<string, object> dicParas)
        {
            return GetNodes(dicParas);
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetNodeInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? dicParas["id"].ToString() : string.Empty;

                if (string.IsNullOrWhiteSpace(id))
                {
                    errMsg = "节点id参数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                try
                {
                    Convert.ToInt32(id);
                }
                catch (Exception ex)
                {
                    return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
                }

                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>();
                int iId = Convert.ToInt32(id);
                if (!dict_SystemService.Any(p => p.ID.Equals(iId)))
                {
                    errMsg = "节点id数据库不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var dict_SystemModel = dict_SystemService.GetModels(p => p.ID.Equals(iId)).FirstOrDefault();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, dict_SystemModel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }            
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddRoot(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int pId = dicParas.ContainsKey("pId") ? dicParas["pId"].Toint(0) : 0;
                string dictKey = dicParas.ContainsKey("dictKey") ? dicParas["dictKey"].ToString() : string.Empty;
                string dictValue = dicParas.ContainsKey("dictValue") ? dicParas["dictValue"].ToString() : string.Empty;
                string comment = dicParas.ContainsKey("comment") ? dicParas["comment"].ToString() : string.Empty;
                string enabled = dicParas.ContainsKey("enabled") ? dicParas["enabled"].ToString() : string.Empty;
                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;
                string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                }

                //验证参数信息
                if (!checkParams(dicParas, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>();

                if (dict_SystemService.Any(p => (p.PID ?? 0) == 0 && p.DictKey.Equals(dictKey, StringComparison.OrdinalIgnoreCase) && (p.MerchID ?? "").Equals((merchId ?? ""), StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "存在重名的主节点";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrWhiteSpace(merchId))
                {
                    
                    if (dict_SystemService.Any(p => p.DictKey.Equals(dictKey, StringComparison.OrdinalIgnoreCase) && (p.MerchID ?? "") == ""))
                    {
                        errMsg = "不能与公有节点重名";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }                

                Dict_System dict_System = new Dict_System();
                dict_System.PID = pId;
                dict_System.DictKey = dictKey;
                dict_System.DictValue = dictValue;
                dict_System.Comment = comment;
                dict_System.Enabled = Convert.ToInt32(enabled);
                dict_System.OrderID = orderId.Toint();
                dict_System.DictLevel = 0;
                dict_System.MerchID = merchId;

                if (!dict_SystemService.Add(dict_System))
                {
                    errMsg = "添加主节点失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //默认把DictValue设置为ID
                if(dictValue.IsNull())
                {                    
                    dict_System.DictValue = dict_System.ID.ToString();
                    if (!dict_SystemService.Update(dict_System))
                    {
                        errMsg = "添加主节点失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }            
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddSub(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? dicParas["id"].ToString() : string.Empty;
                string dictKey = dicParas.ContainsKey("dictKey") ? dicParas["dictKey"].ToString() : string.Empty;
                string dictValue = dicParas.ContainsKey("dictValue") ? dicParas["dictValue"].ToString() : string.Empty;
                string comment = dicParas.ContainsKey("comment") ? dicParas["comment"].ToString() : string.Empty;
                string enabled = dicParas.ContainsKey("enabled") ? dicParas["enabled"].ToString() : string.Empty;
                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;
                string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    errMsg = "选中节点id不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                try
                {
                    Convert.ToInt32(id);
                }
                catch (Exception ex)
                {
                    return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
                }

                //验证参数信息
                if (!checkParams(dicParas, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>();
                int iId = Convert.ToInt32(id);
                if (!dict_SystemService.Any(p => p.ID.Equals(iId)))
                {
                    errMsg = "主节点id数据库不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (dict_SystemService.Any(p => p.PID.Value.Equals(iId) && (p.MerchID ?? "").Equals((merchId ?? ""), StringComparison.OrdinalIgnoreCase) && p.DictKey.Equals(dictKey, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "同一级别下存在重名的节点";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrWhiteSpace(merchId))
                {
                    if (dict_SystemService.Any(p => p.DictKey.Equals(dictKey, StringComparison.OrdinalIgnoreCase) && (p.MerchID ?? "") == ""))
                    {
                        errMsg = "不能与公有节点重名";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                var pModel = dict_SystemService.GetModels(p => p.ID == iId).FirstOrDefault();
                if (pModel.DictLevel > 3)
                {
                    errMsg = "节点层数不能超过3层";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                Dict_System dict_System = new Dict_System();
                dict_System.PID = iId;
                dict_System.MerchID = merchId;
                dict_System.DictKey = dictKey;
                dict_System.DictValue = dictValue;
                dict_System.Comment = comment;
                dict_System.Enabled = Convert.ToInt32(enabled);
                dict_System.OrderID = orderId.Toint();
                dict_System.DictLevel = (pModel.DictLevel ?? 0) + 1;

                if (!dict_SystemService.Add(dict_System))
                {
                    errMsg = "添加子节点失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //默认把DictValue设置为ID
                if (dictValue.IsNull())
                {
                    dict_System.DictValue = dict_System.ID.ToString();
                    if (!dict_SystemService.Update(dict_System))
                    {
                        errMsg = "添加主节点失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }            
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SetSub(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? dicParas["id"].ToString() : string.Empty;
                string dictKey = dicParas.ContainsKey("dictKey") ? dicParas["dictKey"].ToString() : string.Empty;
                string dictValue = dicParas.ContainsKey("dictValue") ? dicParas["dictValue"].ToString() : string.Empty;
                string comment = dicParas.ContainsKey("comment") ? dicParas["comment"].ToString() : string.Empty;
                string enabled = dicParas.ContainsKey("enabled") ? dicParas["enabled"].ToString() : string.Empty;
                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;
                string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    errMsg = "选中节点id不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                try
                {
                    Convert.ToInt32(id);
                }
                catch (Exception ex)
                {
                    return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
                }

                //验证参数信息
                if (!checkParams(dicParas, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>();
                int iId = Convert.ToInt32(id);
                if (!dict_SystemService.Any(p => p.ID == iId))
                {
                    errMsg = "选中节点数据库不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                Dict_System dict_System = dict_SystemService.GetModels(p => p.ID == iId).FirstOrDefault<Dict_System>();
                //if (dict_System.PID == null || !dict_SystemService.Any(p => p.ID == dict_System.PID))
                //{
                //    errMsg = "主节点不可修改";
                //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //}

                int pId = Convert.ToInt32(dict_System.PID.Value);
                if (dict_SystemService.Any(p => p.ID != iId && (p.MerchID ?? "").Equals((merchId ?? ""), StringComparison.OrdinalIgnoreCase) && p.PID.Value == pId && p.DictKey.Equals(dictKey, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "同一级别下存在重名的节点";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //if (!string.IsNullOrWhiteSpace(merchId))
                //{
                //    if (dict_SystemService.Any(p => p.DictKey.Equals(dictKey, StringComparison.OrdinalIgnoreCase) && (p.MerchID == null || p.MerchID.Trim() == string.Empty)))
                //    {
                //        errMsg = "不能与公有节点重名";
                //        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //    }
                //}

                var pModel = dict_SystemService.GetModels(p => p.ID == pId).FirstOrDefault();
                if (pModel != null && pModel.DictLevel > 3)
                {
                    errMsg = "节点层数不能超过3层";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }             

                dict_System.DictKey = dictKey;
                dict_System.DictValue = dictValue.IsNull() ? iId.ToString() : dictValue; //默认把ID设置为DictValue
                dict_System.Comment = comment;
                dict_System.Enabled = Convert.ToInt32(enabled);
                dict_System.OrderID = orderId.Toint();
                dict_System.DictLevel = pModel != null ? (pModel.DictLevel ?? 0) + 1 : 1;

                if (dicParas.ContainsKey("merchId"))
                {
                    dict_System.MerchID = dicParas["merchId"].ToString();
                }

                if (!dict_SystemService.Update(dict_System))
                {
                    errMsg = "修改节点失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, default(Dict_System));
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }                       
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DeleteRoot(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = (dicParas.ContainsKey("id") && Utils.isNumber(dicParas["id"])) ? Convert.ToInt32(dicParas["id"]) : 0;

                if (id == 0)
                {
                    errMsg = "选中节点id不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Dict_System).Namespace);
                        if (!dbContext.Set<Dict_System>().Any(p => p.ID == id))
                        {
                            errMsg = "选中节点数据库不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var dict_System = dbContext.Set<Dict_System>().Where(p => p.PID == id).ToList();
                        foreach (var item in dict_System)
                        {
                            dbContext.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                        }

                        var dict_SystemModel = dbContext.Set<Dict_System>().Where(p => p.ID == id).FirstOrDefault();
                        dbContext.Entry(dict_SystemModel).State = System.Data.Entity.EntityState.Deleted;

                        if (dbContext.SaveChanges() < 0)
                        {
                            errMsg = "更新数据库失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}