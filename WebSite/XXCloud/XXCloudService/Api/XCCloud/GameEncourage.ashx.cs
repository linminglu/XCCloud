using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Common.Extensions;
using System.Data.SqlClient;
using System.Transactions;
using System.Data.Entity.Validation;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// GameEncourage 的摘要说明
    /// </summary>
    public class GameEncourage : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string sql = @"SELECT
                                    a.ID, b.GameName,
                                    ((case when ISNULL(a.StartDate,'')='' then '' else convert(varchar,a.StartDate,23) end) + '~' +
                                    (case when ISNULL(a.EndDate,'')='' then '' else convert(varchar,a.EndDate,23) end)) AS ValidDate,
                                    a.Note
                                FROM
                                	Data_GameEncourage a
                                LEFT JOIN 
                                    Data_GameInfo b ON a.GameIndex = b.ID                                
                                WHERE 1=1
                            ";
                sql += " AND a.StoreID=" + storeId;

                var list = Data_PushRuleService.I.SqlQuery<Data_GameEncourageList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var model = Data_GameEncourageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (model == null)
                {
                    errMsg = "该规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_GameEncourage = (new
                {
                    model = model,
                    //EncourageList = Data_GameEncourage_ListService.I.GetModels(p => p.EncourageID == id)
                }).AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_GameEncourage);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.GetArray("encourageList").Validarray("鼓励续玩列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var id = dicParas.Get("id").Toint(0);
                var encourageList = dicParas.GetArray("encourageList");
               
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_GameEncourageService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GameEncourage();
                        model.MerchID = merchId;
                        model.StoreID = storeId;
                        Utils.GetModel(dicParas, ref model);
                        if (id == 0)
                        {
                            if (!Data_GameEncourageService.I.Add(model))
                            {
                                errMsg = "保存鼓励续玩规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该规则不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GameEncourageService.I.Update(model))
                            {
                                errMsg = "保存鼓励续玩规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = model.ID;

                        //保存鼓励续玩信息
                        if (encourageList != null && encourageList.Count() >= 0)
                        {
                            //先删除，后添加
                            //foreach (var encourageModel in Data_GameEncourage_ListService.I.GetModels(p => p.EncourageID == id))
                            //{
                            //    Data_GameEncourage_ListService.I.DeleteModel(encourageModel);
                            //}

                            foreach (IDictionary<string, object> el in encourageList)
                            {
                                if (el != null)
                                {
                                    var dicPar = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPar.Get("gameTims").Validintnozero("连续启动局数", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPar.Get("encouragePrice").Validdecimalnozero("减免币数", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    //var encourageModel = new Data_GameEncourage_List();
                                    //encourageModel.GameTims = dicPar.Get("gameTims").Toint();
                                    //encourageModel.EncouragePrice = dicPar.Get("encouragePrice").Todecimal();
                                    //encourageModel.EncourageID = id;
                                    //encourageModel.MerchID = merchId;
                                    //encourageModel.StoreID = storeId;
                                    //Data_GameEncourage_ListService.I.AddModel(encourageModel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            //if (!Data_GameEncourage_ListService.I.SaveChanges())
                            //{
                            //    errMsg = "保存鼓励续玩信息失败";
                            //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            //}
                        }                        

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
                    }
                    catch (Exception e)
                    {
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
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
        public object DelGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GameEncourageService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该规则不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //foreach (var gameEncourageModel in Data_GameEncourage_ListService.I.GetModels(p => p.EncourageID == id))
                        //{
                        //    Data_GameEncourage_ListService.I.DeleteModel(gameEncourageModel);
                        //}

                        var model = Data_GameEncourageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        Data_GameEncourageService.I.DeleteModel(model);

                        if (!Data_GameEncourageService.I.SaveChanges())
                        {
                            errMsg = "删除鼓励续玩规则失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
                    }
                    catch (Exception e)
                    {
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
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