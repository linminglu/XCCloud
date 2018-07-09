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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

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
                                	(case when ISNULL(a.StartTime,'')='' then '' else convert(varchar,a.StartTime,23) end) AS StartTime,
                                    /*结束时间*/
                                	(case when ISNULL(a.EndTime,'')='' then '' else convert(varchar,a.EndTime,23) end) AS EndTime,
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
                                    a.RuleLevel,
                                    a.State
                                FROM
                                	Data_FreeGiveRule a
                                LEFT JOIN Dict_BalanceType b ON a.FreeBalanceIndex = b.ID                                
                                WHERE 1=1
                            ";
                sql = sql + " AND a.MerchID='" + merchId + "'";
                sql = sql + sqlWhere; 
                sql = sql + " ORDER BY a.RuleLevel ";
                
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
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var freeGiveRules = (from c in
                                         (from a in Data_FreeGiveRuleService.N.GetModels(p => p.ID == id)
                                          join b in Data_FreeGiveRule_MemberlevelService.N.GetModels() on a.ID equals b.RuleID into b1
                                          from b in b1.DefaultIfEmpty()
                                          select new { a = a, MemberLevelID = b != null ? b.MemberLevelID : (int?)null }).AsEnumerable()
                                     group c by c.a.ID into g
                                     select new
                                     {
                                         a = g.FirstOrDefault().a,
                                         MemberLevelIDs = string.Join("|", g.Select(o => o.MemberLevelID))
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;

                var memberLevelIds = dicParas.Get("memberLevelIds");
                var id = dicParas.Get("id").Toint(0);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_FreeGiveRuleService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_FreeGiveRule();
                        Utils.GetModel(dicParas, ref model);
                        model.MerchID = merchId;
                        if (id == 0)
                        {
                            if (!Data_FreeGiveRuleService.I.Add(model))
                            {
                                errMsg = "添加免费赠送规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);  
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
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

                        id = model.ID;

                        //先删除，添加适用级别
                        foreach (var memberLevel in Data_FreeGiveRule_MemberlevelService.I.GetModels(p => p.RuleID == id))
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
                                memberLevel.RuleID = id;
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
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("规则ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if (!id.Validintnozero("规则ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            if (!Data_FreeGiveRuleService.I.Any(a => a.ID == (int)id))
                            {
                                errMsg = "该免费赠送规则不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var model = Data_FreeGiveRuleService.I.GetModels(p => p.ID == (int)id).FirstOrDefault();
                            model.State = 0;
                            if (!Data_FreeGiveRuleService.I.Update(model))
                            {
                                errMsg = "删除免费赠送规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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

        /// <summary>
        /// 升降优先级
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UpdateRuleLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if(!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("updateState").Nonempty("升降级状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("updateState").Toint() != 1 && dicParas.Get("updateState").Toint() != -1)
                {
                    errMsg = "升降级状态参数值不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var id = dicParas.Get("id").Toint();
                var updateState = dicParas.Get("updateState").Toint(0);
                                                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_FreeGiveRuleService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该赠送规则不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //设置当前优惠券级别，默认为1,且不超过最大优先级
                        var freeGiveRule = Data_FreeGiveRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        var oldLevel = freeGiveRule.RuleLevel;
                        freeGiveRule.RuleLevel = (freeGiveRule.RuleLevel ?? 0) + updateState;
                        if (freeGiveRule.RuleLevel < 1)
                        {
                            freeGiveRule.RuleLevel = 1;
                        }

                        var max = Data_FreeGiveRuleService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).Max(m => m.RuleLevel);
                        if (freeGiveRule.RuleLevel > max)
                        {
                            freeGiveRule.RuleLevel = max;
                        }

                        Data_FreeGiveRuleService.I.UpdateModel(freeGiveRule);

                        var newLevel = freeGiveRule.RuleLevel;
                        if (oldLevel != newLevel || oldLevel == null)
                        {
                            if (oldLevel == null)
                            {
                                //后续优惠券降一级
                                var linq = Data_FreeGiveRuleService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != id
                                                               && p.RuleLevel >= newLevel);
                                foreach (var model in linq)
                                {
                                    model.RuleLevel = model.RuleLevel + 1;
                                    Data_FreeGiveRuleService.I.UpdateModel(model);
                                }
                            }
                            else
                            {
                                //与下一个优惠券交换优先级
                                var nextModel = Data_FreeGiveRuleService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != id
                                                               && p.RuleLevel == newLevel).FirstOrDefault();
                                if (nextModel != null)
                                {
                                    nextModel.RuleLevel = nextModel.RuleLevel - updateState;
                                    Data_FreeGiveRuleService.I.UpdateModel(nextModel);
                                }
                            }
                        }

                        if (!Data_FreeGiveRuleService.I.SaveChanges())
                        {
                            errMsg = "更新优惠券优先级失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        errMsg = e.Message;
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
    }
}