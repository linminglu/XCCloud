using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Common.Extensions;
using System.Transactions;
using XCCloudService.Model.XCCloud;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// FreeGiveRule 的摘要说明
    /// </summary>
    public class FreeGiveRule : ApiBase
    {

        /// <summary>
        /// 查询免费赠送规则
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryFreeGiveRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);


                #region Sql语句
                string sql = @"SELECT
                                    /*规则ID*/
                                	a.ID,
                                    /*规则名称*/
                                	a.RuleName,
                                	/*开始时间*/
                                	(case when ISNULL(a.StartTime,'')='' then '' else convert(varchar,a.StartTime,20) end) AS StartTime,
                                    /*结束时间*/
                                	(case when ISNULL(a.EndTime,'')='' then '' else convert(varchar,a.EndTime,20) end) AS EndTime,
                                	/*赠送类别*/
                                	a.FreeBalanceIndex AS FreeBalanceIndex,
                                    /*赠送类别*/
                                	b.TypeName AS FreeBalanceStr,
                                	/*赠送数量*/
                                	a.FreeCount AS FreeCount,                                	
                                	/*允许散客*/
                                	a.AllowGuest,
                                    /*散客身份确定方式*/
                                    a.IDReadType,                                    
                                    /*赠送周期*/
                                    a.PeriodType,
                                    /*间隔数量*/
                                    a.SpanCount,                                	
                                	/*间隔类别*/
                                    a.SpanType, 
                                    /*领取次数*/
                                    a.GetTimes, 
                                	/*优先级别*/
                                    a.RuleLevel

                                FROM
                                	Data_FreeGiveRule a
                                LEFT JOIN Dict_BalanceType b ON a.FreeBalanceIndex = b.ID                                
                                WHERE 1=1
                            ";
                sql += " AND a.MerchID='" + merchId + "'";
                
                #endregion

                var list = Data_FreeGiveRuleService.I.SqlQuery<Data_FreeGiveRuleList>(sql, parameters).ToList();
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetFreeGiveRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var freeGiveRules = (from c in
                                        (from a in Data_FreeGiveRuleService.N.GetModels(p => p.ID == id)
                                         join b in Data_FreeGiveRule_MemberlevelService.N.GetModels() on a.ID equals b.RuleID into b1
                                         from b in b1.DefaultIfEmpty()
                                         select new { a = a, b = b }).AsEnumerable()
                                    group c by c.a.ID into g
                                    select new
                                    {
                                        a = g.FirstOrDefault().a,
                                        MemberLevelIDs = string.Join("|", g.Select(o => o.b.MemberLevelID))
                                    }).AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, freeGiveRules);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveFreeGiveRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                if(!dicParas.Get("state").Validint("状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);  

                var memberLevelIds = dicParas.Get("memberLevelIds");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = new Data_FreeGiveRule();
                        Utils.GetModel(dicParas, ref model);
                        model.MerchID = merchId;
                        if (model.ID == 0)
                        {
                            if (!Data_FreeGiveRuleService.I.Add(model))
                            {
                                errMsg = "添加免费赠送规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);  
                            }
                        }
                        else
                        {
                            if (!Data_FreeGiveRuleService.I.Any(p => p.ID == model.ID))
                            {
                                errMsg = "该免费赠送规则不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);  
                            }

                            if (!Data_FreeGiveRuleService.I.Update(model))
                            {
                                errMsg = "更新免费赠送规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        var ruleId = model.ID;

                        //先删除，添加适用级别
                        foreach (var memberLevel in Data_FreeGiveRule_MemberlevelService.I.GetModels(p => p.RuleID == ruleId))
                        {
                            Data_FreeGiveRule_MemberlevelService.I.DeleteModel(memberLevel);
                        }

                        if (!memberLevelIds.IsNull())
                        {                            
                            foreach (var memberLevelId in memberLevelIds.Split('|'))
                            {
                                if(!memberLevelId.Validintnozero("会员级别ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                var memberLevel = new Data_FreeGiveRule_Memberlevel();
                                memberLevel.MerchID = merchId;
                                memberLevel.RuleID = ruleId;
                                Data_FreeGiveRule_MemberlevelService.I.AddModel(memberLevel);
                            }
                        }

                        if (!Data_FreeGiveRule_MemberlevelService.I.SaveChanges())
                        {
                            errMsg = "更新免费赠送适用级别信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
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
        public object DelFreeGiveRule(Dictionary<string, object> dicParas)
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
                        if (!Data_FreeGiveRuleService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该免费赠送规则不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_FreeGiveRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        model.State = 0;
                        if (!Data_FreeGiveRuleService.I.Update(model))
                        {
                            errMsg = "删除免费赠送规则失败";
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