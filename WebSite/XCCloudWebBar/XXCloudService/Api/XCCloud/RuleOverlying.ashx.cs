using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Common.Extensions;
using XXCloudService.Api.XCCloud.Common;
using System.Transactions;
using XCCloudWebBar.Model.XCCloud;
using System.Data.Entity.Validation;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// RuleOverlying 的摘要说明
    /// </summary>
    public class RuleOverlying : ApiBase
    {
        /// <summary>
        /// 查询优惠券叠加规则
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryRuleOverlying(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var discountList = from b in
                                       (from a in Data_DiscountRuleService.I.GetModels(p => p.State == 1 && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                        select a).AsEnumerable()
                                   select new RuleOverlyingModel
                                   {
                                       ID = b.ID,
                                       RuleName = b.RuleName,
                                       RuleType = (int)RuleType.Discount,
                                       TypeName = RuleType.Discount.GetDescription()
                                   };
                var couponList = from b in
                                     (from a in Data_CouponInfoService.I.GetModels(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.CouponType != null && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                      select a).AsEnumerable()
                                 select new RuleOverlyingModel
                                 {
                                     ID = b.ID,
                                     CouponType = b.CouponType.Toint(),
                                     RuleName = b.CouponName,
                                     RuleType = (int)RuleType.Coupon,
                                     TypeName = ((CouponType?)b.CouponType).GetDescription()
                                 };

                var list = new List<RuleOverlyingModel>();                
                list.AddRange(discountList);
                list.AddRange(couponList);
                list = list.OrderBy(or => or.TypeName).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetRuleOverlying(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("ruleType").Validint("规则类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("ruleId").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var ruleType = dicParas.Get("ruleType").Toint();
                var ruleId = dicParas.Get("ruleId").Toint();

                var ruleOverlying = (from a in Data_RuleOverlying_ListService.N.GetModels(p => p.RuleType == ruleType && p.RuleID == ruleId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                     join b in Data_RuleOverlying_ListService.N.GetModels(p => (p.RuleType != ruleType || p.RuleID != ruleId) && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)) on a.GroupID equals b.GroupID
                                     where a.GroupID != null
                                     select new
                                     {
                                         RuleType = b.RuleType,
                                         RuleID = b.RuleID
                                     }).Distinct();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, ruleOverlying);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveRuleOverlying(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("ruleId").Validintnozero("目标规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("ruleType").Validint("目标规则类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var ruleId = dicParas.Get("ruleId").Toint();
                var ruleType = dicParas.Get("ruleType").Toint();
                var ruleOverlyings = dicParas.GetArray("ruleOverlyings");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (ruleOverlyings != null && ruleOverlyings.Count() >= 0)
                        {
                            if (ruleOverlyings.Count() == 0)
                            {
                                foreach (var model in Data_RuleOverlying_ListService.I.GetModels(p => p.RuleID == ruleId && p.RuleType == ruleType && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                                {
                                    var groupId = model.GroupID;
                                    var groupModel = Data_RuleOverlying_GroupService.I.GetModels(p => p.ID == groupId).FirstOrDefault();
                                    if (groupModel != null)
                                        Data_RuleOverlying_GroupService.I.DeleteModel(groupModel);
                                    Data_RuleOverlying_ListService.I.DeleteModel(model);
                                }

                                if (!Data_RuleOverlying_ListService.I.SaveChanges())
                                {
                                    errMsg = "保存优惠叠加规则失败";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }
                            else
                            {
                                foreach (IDictionary<string, object> el in ruleOverlyings)
                                {
                                    if (el != null)
                                    {
                                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                        if (!dicPara.Get("ruleId").Validintnozero("规则ID", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        if (!dicPara.Get("ruleType").Validint("规则类别", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                        var gRuleId = dicPara.Get("ruleId").Toint();
                                        var gRuleType = dicPara.Get("ruleType").Toint();
                                        if (ruleId != gRuleId || ruleType != gRuleType)
                                        {
                                            //目标规则与当前规则是否已分组
                                            bool grouped = (from a in Data_RuleOverlying_ListService.N.GetModels(p => p.RuleID == ruleId && p.RuleType == ruleType && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                                            join b in Data_RuleOverlying_ListService.N.GetModels(p => p.RuleID == gRuleId && p.RuleType == gRuleType && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                                            on a.GroupID equals b.GroupID
                                                            where a.GroupID != null
                                                            select 1).Any();
                                            if (!grouped)
                                            {
                                                var groupModel = new Data_RuleOverlying_Group();
                                                groupModel.MerchID = merchId;
                                                if (!Data_RuleOverlying_GroupService.I.Add(groupModel))
                                                {
                                                    errMsg = "添加分组失败";
                                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                                }

                                                var model = new Data_RuleOverlying_List();
                                                model.GroupID = groupModel.ID;
                                                model.MerchID = merchId;
                                                model.RuleID = ruleId;
                                                model.RuleType = ruleType;
                                                Data_RuleOverlying_ListService.I.AddModel(model);
                                                model = new Data_RuleOverlying_List();
                                                model.GroupID = groupModel.ID;
                                                model.MerchID = merchId;
                                                model.RuleID = gRuleId;
                                                model.RuleType = gRuleType;
                                                Data_RuleOverlying_ListService.I.AddModel(model);
                                                if (!Data_RuleOverlying_ListService.I.SaveChanges())
                                                {
                                                    errMsg = "添加优惠券叠加规则失败";
                                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errMsg = "提交数据包含空对象";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                }
                            }
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
    }
}