using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using System.Transactions;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Business.XCGameMana;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Schedule 的摘要说明
    /// </summary>
    public class Schedule : ApiBase
    {
        /// <summary>
        /// 交班
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DoSchedule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!checkDog(dicParas.Get("dogToken"), storeId, out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var forceScheduleToNow = dicParas.Get("forceScheduleToNow").Toint(); //是否强制结账到当前日期
                var store_CheckDateService = Store_CheckDateService.I;
                var flw_ScheduleService = Flw_ScheduleService.I;
                var flw_Schedule_UserInfoService = Flw_Schedule_UserInfoService.I;
                var flw_OrderService = Flw_OrderService.I;

                //获取当前营业日期
                var currCheckDate = store_CheckDateService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)).OrderByDescending(or => or.CheckDate).Select(o => o.CheckDate).FirstOrDefault();
                if (currCheckDate == null)
                {
                    errMsg = "无法找到当前营业日期";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //上一营业日期是否审核
                var lastCheckDate = currCheckDate.Value.AddDays(-1);
                if (flw_ScheduleService.Any(p => (p.ID ?? "") != "" && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.CheckDate == lastCheckDate && p.State != (int)ScheduleState.Checked))
                {
                    errMsg = "上一个营业日期存在未审核的班次, 禁止交班";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //是否有且仅有一个正在进行中的班次
                if (flw_ScheduleService.GetCount(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.CheckDate == currCheckDate && p.State == (int)ScheduleState.Starting) != 1)
                {
                    errMsg = "当前应该有且仅有一个正在进行中的班次";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        //修改班次状态, 清理吧台用户令牌, 通知所有吧台用户
                        var currScheduleModel = flw_ScheduleService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.CheckDate == currCheckDate && p.State == (int)ScheduleState.Starting).FirstOrDefault();
                        currScheduleModel.State = (int)ScheduleState.Submitted;
                        flw_ScheduleService.UpdateModel(currScheduleModel, false);
                        
                        //统计每个员工的订单现金、网络支付金额
                        var scheduleId = currScheduleModel.ID;
                        var linq = from a in flw_Schedule_UserInfoService.GetModels(p => p.ScheduleID.Equals(scheduleId))
                                   join b in flw_OrderService.GetModels() on new { a.ScheduleID, a.UserID } equals new { b.ScheduleID, b.UserID }
                                   group b by new { b.ScheduleID, b.UserID } into g
                                   select new
                                   {
                                       ScheduleID = g.Key.ScheduleID,
                                       UserID = g.Key.UserID,
                                       CashTotle = g.Where(w => w.PayType == 0).Sum(s => s.RealPay),
                                       NetTotle = g.Where(w => w.PayType == 1 || w.PayType == 2).Sum(s => s.RealPay)
                                   };
                        foreach (var model in linq)
                        {
                            var userId = model.UserID;
                            var currScheduleUserInfoModel = flw_Schedule_UserInfoService.GetModels(p => p.ScheduleID.Equals(scheduleId, StringComparison.OrdinalIgnoreCase) && p.UserID == userId).FirstOrDefault();
                            currScheduleUserInfoModel.CashTotle = model.CashTotle;
                            currScheduleUserInfoModel.NetTotle = model.NetTotle;
                            flw_Schedule_UserInfoService.UpdateModel(currScheduleUserInfoModel, false);
                        }

                        //清理吧台用户令牌
                        XCCloudUserTokenBusiness.RemoveWorkStationUserToken(storeId);

                        //通知所有吧台用户

                        //是否为当前营业日期最后一个班次
                        if (flw_ScheduleService.GetCount(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.CheckDate == currCheckDate && p.State != (int)ScheduleState.Checked && p.State != (int)ScheduleState.Submitted) == 1)
                        {
                            //创建新营业日期和班次             
                            var newCheckDate = currCheckDate.Value.AddDays(1);
                            List<string> scheduleNames = null;
                            if (!getScheduleCount(storeId, ref scheduleNames, out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            
                            //系统时间大于新营业日期, 提示用户是否强制结账到当前日期
                            int diffDays = (DateTime.Now - newCheckDate).Days;
                            if (diffDays > 0)
                            {
                                if (forceScheduleToNow == null)
                                    return ResponseModelFactory.CreateConfirmModel(isSignKeyReturn, "系统时间大于新营业日期, 是否强制结账到当前日期");

                                if (forceScheduleToNow == 1)
                                {
                                    do
                                    {                                        
                                        //循环创建营业日期和空班直到当前日期
                                        if (!createCheckDateAndSchedule(merchId, storeId, newCheckDate, scheduleNames, out errMsg, true))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        newCheckDate = newCheckDate.AddDays(1);
                                        diffDays--;
                                    }
                                    while (diffDays > 0);
                                }
                            }

                            if (!createCheckDateAndSchedule(merchId, storeId, newCheckDate, scheduleNames, out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //保存营业日期信息
                        if (!store_CheckDateService.SaveChanges())
                        {
                            errMsg = "保存营业日期信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }  

                        //保存班次信息
                        if (!flw_ScheduleService.SaveChanges())
                        {
                            errMsg = "保存班次信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }  

                        //保存班次用户信息
                        if (!flw_Schedule_UserInfoService.SaveChanges())
                        {
                            errMsg = "保存班次用户信息失败";
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
        
    }
}