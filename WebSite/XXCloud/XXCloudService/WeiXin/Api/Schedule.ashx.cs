using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.Common.Extensions;
using XCCloudService.Common.Enum;
using XCCloudService.Common;
using XCCloudService.BLL.XCCloud;

namespace XXCloudService.WeiXin.Api
{
    /// <summary>
    /// Schedule 的摘要说明
    /// </summary>
    public class Schedule : ApiBase
    {

        /// <summary>
        /// 获取班次操作信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object GetScheduleUserInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("scheduleId").Nonempty("班次ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("userId").Validintnozero("员工ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var scheduleId = dicParas.Get("scheduleId");
                var userId = dicParas.Get("userId").Toint(); 

                var flw_Schedule_UserInfoService = Flw_Schedule_UserInfoService.I;

                var flw_Schedule_UserInfo = flw_Schedule_UserInfoService.GetModels(p => p.ScheduleID.Equals(scheduleId, StringComparison.OrdinalIgnoreCase) && p.UserID == userId).Select(o => new 
                                            {
                                                ID = o.ID, //班次操作ID
                                                CheckDate = o.CheckDate, //营业日期
                                                ScheduleName = o.ScheduleName, //班次名
                                                OpenTime = o.OpenTime, //开始时间
                                                ShiftTime = o.ShiftTime, //结束时间
                                                WorkStation = o.WorkStation,  //工作站名
                                                RealCash = o.RealCash, //实点现金
                                                RealCredit = o.RealCredit //实点信用卡小票
                                            });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, flw_Schedule_UserInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 保存班次操作信息（实点现金、实点信用卡小票等）
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object SaveScheduleUserInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Nonempty("班次操作ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("realCash").Validdecimal("实点现金", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("realCredit").Validdecimal("实点信用卡小票", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id"); 
                var realCash = dicParas.Get("realCash").Todecimal(0);
                var realCredit = dicParas.Get("realCredit").Todecimal(0);

                var flw_Schedule_UserInfoService = Flw_Schedule_UserInfoService.I;

                if(!flw_Schedule_UserInfoService.Any(p=>p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该班次操作信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var flw_Schedule_UserInfo = flw_Schedule_UserInfoService.GetModels(p=>p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                flw_Schedule_UserInfo.RealCash = realCash;
                flw_Schedule_UserInfo.RealCredit = realCredit;
                if(!flw_Schedule_UserInfoService.Update(flw_Schedule_UserInfo, false))
                {
                    errMsg = "保存班次操作信息失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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