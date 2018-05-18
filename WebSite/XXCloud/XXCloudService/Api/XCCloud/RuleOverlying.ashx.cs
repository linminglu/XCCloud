using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Common.Extensions;
using XXCloudService.Api.XCCloud.Common;
using System.Transactions;
using XCCloudService.Model.XCCloud;
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
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var discountList = from b in
                                       (from a in Data_DiscountRuleService.I.GetModels(p => p.State == 1 && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                        select a).AsEnumerable()
                                   select new RuleOverlyingModel
                                   {
                                       ID = b.ID,
                                       PID = (int)RuleType.Discount,
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
                                     PID = b.CouponType.Toint(),
                                     RuleName = b.CouponName,
                                     RuleType = (int)RuleType.Coupon,
                                     TypeName = ((CouponType?)b.CouponType).GetDescription()
                                 };
                var list = new List<RuleOverlyingModel>();
                foreach (RuleType item in Enum.GetValues(typeof(RuleType)))
                {
                    if (item == RuleType.Discount)
                    {
                        list.Add(new RuleOverlyingModel { ID = (int)item, PID = -1, RuleName = item.GetDescription(), TypeName = string.Empty });
                        list.AddRange(discountList);
                        
                    }
                    else if (item == RuleType.Coupon)
                    {
                        list.Add(new RuleOverlyingModel { ID = (int)item, PID = -1, RuleName = item.GetDescription(), TypeName = string.Empty });
                        foreach (CouponType item1 in Enum.GetValues(typeof(CouponType)))
                        {
                            list.Add(new RuleOverlyingModel { ID = (int)item1, PID = (int)item, RuleName = item1.GetDescription(), TypeName = item.GetDescription() });
                            list.AddRange(couponList.Where(w=>w.PID == (int)item1));
                        }
                    }
                }

                //实例化一个根节点
                RuleOverlyingModel rootRoot = new RuleOverlyingModel();
                rootRoot.ID = -1;
                rootRoot.RuleName = "全部";
                rootRoot.TypeName = string.Empty;
                TreeHelper.LoopToAppendChildren(list, rootRoot);

                //生成右侧列表
                list = new List<RuleOverlyingModel>();
                list.AddRange(discountList);
                list.AddRange(couponList);

                var query = new QueryRuleOverlyingData
                {
                    TreeNodes = rootRoot.Children,                    //左侧树节点
                    List = list.OrderBy(or => or.TypeName).ToList()   //右侧列表
                };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, query);
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
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("ruleType").Validint("规则类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if(!dicParas.Get("ruleId").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var ruleType = dicParas.Get("ruleType").Toint();
                var ruleId = dicParas.Get("ruleId").Toint();

                var ruleOverlying = (from a in Data_RuleOverlying_ListService.N.GetModels(p => p.RuleType == ruleType && p.RuleID == ruleId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                     join b in Data_RuleOverlying_GroupService.N.GetModels() on a.GroupID equals b.ID
                                     join c in Data_RuleOverlying_ListService.N.GetModels() on b.ID equals c.GroupID
                                     select new
                                     {
                                         RuleType = c.RuleType,
                                         RuleID = c.RuleID
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
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

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
                                var ruleModel = Data_RuleOverlying_ListService.I.GetModels(p => p.RuleID == ruleId && p.RuleType == ruleType && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? new Data_RuleOverlying_List();
                                if (ruleModel == null)
                                {
                                    var groupModel = new Data_RuleOverlying_Group();
                                    groupModel.MerchID = merchId;
                                    if (!Data_RuleOverlying_GroupService.I.Add(groupModel))
                                    {
                                        errMsg = "添加分组信息失败";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    ruleModel.MerchID = merchId;
                                    ruleModel.RuleType = ruleType;
                                    ruleModel.RuleID = ruleId;
                                    ruleModel.GroupID = groupModel.ID;
                                    if (!Data_RuleOverlying_ListService.I.Add(ruleModel))
                                    {
                                        errMsg = "添加优惠叠加规则失败";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                }

                                foreach (IDictionary<string, object> el in ruleOverlyings)
                                {
                                    if (el != null)
                                    {
                                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                        if (!dicPara.Get("ruleId").Validintnozero("规则ID", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        if (!dicPara.Get("ruleType").Validintnozero("规则类别", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                        var gRuleId = dicPara.Get("ruleId").Toint();
                                        var gRuleType = dicPara.Get("ruleType").Toint();

                                        //目标规则与当前规则是否已分组
                                        bool grouped = (from a in Data_RuleOverlying_ListService.N.GetModels(p => p.RuleID == ruleId && p.RuleType == ruleType && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                                        join b in Data_RuleOverlying_ListService.N.GetModels(p => p.RuleID == gRuleId && p.RuleType == gRuleType && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                                        on a.GroupID equals b.GroupID
                                                        where a.GroupID != null
                                                        select 1).Any();
                                        if (!grouped)
                                        {
                                            var model = new Data_RuleOverlying_List();
                                            model.GroupID = ruleModel.GroupID;
                                            model.MerchID = merchId;
                                            model.RuleID = ruleId;
                                            model.RuleType = ruleType;
                                            Data_RuleOverlying_ListService.I.AddModel(model);
                                        }
                                    }
                                    else
                                    {
                                        errMsg = "提交数据包含空对象";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                }

                                if (!Data_RuleOverlying_ListService.I.SaveChanges())
                                {
                                    errMsg = "保存优惠叠加规则失败";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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