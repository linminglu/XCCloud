using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Transactions;
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
using System.Data;
using XCCloudService.BLL.CommonBLL;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// PushRule 的摘要说明
    /// </summary>
    public class PushRule : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryPushRule(Dictionary<string, object> dicParas)
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
                                    a.ID, a.Week, a.PushCoin1, a.PushCoin2, a.Allow_In, a.Allow_Out, a.Level, a.StartTime, a.EndTime,
                                    (case when ISNULL(a.StartDate,'')='' then '' else convert(varchar,a.StartDate,23) end) AS StartDate,
                                    (case when ISNULL(a.EndDate,'')='' then '' else convert(varchar,a.EndDate,23) end) AS EndDate,
                                    (b.GameName + (case when b.Cnt > 1 then '等多个' else '' end)) AS GameName,
                                    (c.MemberLevelName + (case when c.Cnt > 1 then '等多个' else '' end)) AS MemberLevelName
                                FROM
                                	Data_PushRule a
                                LEFT JOIN (
                                	SELECT
                                		a.PushRuleID, c.Cnt, b.GameName, ROW_NUMBER() over(partition by a.PushRuleID order by a.ID) as RowNum
                                	FROM
                                		Data_PushRule_GameList a 
                                    INNER JOIN (SELECT PushRuleID, COUNT(PushRuleID) AS Cnt from Data_PushRule_GameList group by PushRuleID) c on a.PushRuleID=c.PushRuleID  
                                    INNER JOIN
                                        Data_GameInfo b ON a.GameID = b.ID                                                                                          
                                	WHERE
                                		a.StoreID='" + storeId + "'" +
                              @") b ON a.ID = b.PushRuleID and b.RowNum <= 1
                                LEFT JOIN (
                                	SELECT
                                		a.PushRuleID, c.Cnt, b.MemberLevelName, ROW_NUMBER() over(partition by a.PushRuleID order by a.ID) as RowNum
                                	FROM
                                		Data_PushRule_MemberLevelList a 
                                    INNER JOIN (SELECT PushRuleID, COUNT(PushRuleID) AS Cnt from Data_PushRule_MemberLevelList group by PushRuleID) c on a.PushRuleID=c.PushRuleID
                                    INNER JOIN 
                                        Data_MemberLevel b ON a.MemberLevelID = b.MemberLevelID                                                      
                                	WHERE
                                		a.StoreID='" + storeId + "'" +
                              @") c ON a.ID = c.PushRuleID and c.RowNum <= 1
                                WHERE 1=1
                            ";
                sql = sql + " AND a.StoreID='" + storeId + "'";
                sql = sql + sqlWhere;

                var list = Data_PushRuleService.I.SqlQuery<Data_Push_RuleList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetPushRule(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var model = Data_PushRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (model == null)
                {
                    errMsg = "该规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_PushRule = (new
                {
                    model = model,
                    GameList = Data_PushRule_GameListService.I.GetModels(p => p.PushRuleID == id).Select(o => o.GameID),
                    MemberLevelList = Data_PushRule_MemberLevelListService.I.GetModels(p => p.PushRuleID == id).Select(o => new { MemberLevelID = o.MemberLevelID })
                }).AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_PushRule);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SavePushRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if(!dicParas.GetArray("gameList").Validarray("游戏机列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("memberLevelList").Validarray("会员级别列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);
                var gameList = dicParas.GetArray("gameList");
                var memberLevelList = dicParas.GetArray("memberLevelList");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_PushRuleService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_PushRule();                        
                        Utils.GetModel(dicParas, ref model);
                        model.MerchID = merchId;
                        model.StoreID = storeId;
                        model.StartDate = model.StartDate.Todate();
                        model.EndDate = model.EndDate.Todate();
                        if (id == 0)
                        {
                            if (!Data_PushRuleService.I.Add(model))
                            {
                                errMsg = "保存投币优惠规则失败";
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

                            if (!Data_PushRuleService.I.Update(model))
                            {
                                errMsg = "保存投币优惠规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = model.ID;

                        //保存游戏机绑定信息
                        if (gameList != null && gameList.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var gameModel in Data_PushRule_GameListService.I.GetModels(p => p.PushRuleID == id))
                            {
                                Data_PushRule_GameListService.I.DeleteModel(gameModel);
                            }

                            foreach (IDictionary<string, object> el in gameList)
                            {
                                if (el != null)
                                {
                                    var dicPar = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPar.Get("gameId").Validintnozero("游戏机ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var gameModel = new Data_PushRule_GameList();
                                    gameModel.PushRuleID = id;
                                    gameModel.GameID = dicPar.Get("gameId").Toint();
                                    gameModel.MerchID = merchId;
                                    gameModel.StoreID = storeId;
                                    Data_PushRule_GameListService.I.AddModel(gameModel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_PushRule_GameListService.I.SaveChanges())
                            {
                                errMsg = "保存游戏机绑定信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //保存会员级别绑定信息
                        if (memberLevelList != null && memberLevelList.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var memberLevelModel in Data_PushRule_MemberLevelListService.I.GetModels(p => p.PushRuleID == id))
                            {
                                Data_PushRule_MemberLevelListService.I.DeleteModel(memberLevelModel);
                            }

                            foreach (IDictionary<string, object> el in memberLevelList)
                            {
                                if (el != null)
                                {
                                    var dicPar = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPar.Get("memberLevelId").Validintnozero("会员级别ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var memberLevelModel = new Data_PushRule_MemberLevelList();
                                    memberLevelModel.PushRuleID = id;
                                    memberLevelModel.MemberLevelID = dicPar.Get("memberLevelId").Toint();
                                    memberLevelModel.MerchID = merchId;
                                    memberLevelModel.StoreID = storeId;
                                    Data_PushRule_MemberLevelListService.I.AddModel(memberLevelModel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_PushRule_MemberLevelListService.I.SaveChanges())
                            {
                                errMsg = "保存会员级别绑定信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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
        public object DelPushRule(Dictionary<string, object> dicParas)
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
                        if (!Data_PushRuleService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该规则不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        foreach (var gameModel in Data_PushRule_GameListService.I.GetModels(p => p.PushRuleID == id))
                        {
                            Data_PushRule_GameListService.I.DeleteModel(gameModel);
                        }

                        foreach (var memberLevelModel in Data_PushRule_MemberLevelListService.I.GetModels(p => p.PushRuleID == id))
                        {
                            Data_PushRule_MemberLevelListService.I.DeleteModel(memberLevelModel);
                        }

                        var model = Data_PushRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        Data_PushRuleService.I.DeleteModel(model);

                        if (!Data_PushRuleService.I.SaveChanges())
                        {
                            errMsg = "删除投币规则失败";
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