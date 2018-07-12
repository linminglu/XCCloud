using System;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCGameManager;
using XCCloudService.Model.XCGame;
using XCCloudService.SocketService.UDP;
using XCCloudService.SocketService.UDP.Factory;
using XCCloudService.Model.CustomModel.Common;
using XCCloudService.Business.Common;
using XCCloudService.Model.Socket.UDP;
using XXCloudService.Utility;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCGame;

namespace XXCloudService.Api.XCGame
{    
    /// <summary>
    /// Ticket 的摘要说明
    /// </summary>
    public class Ticket : ApiBase
    {
        //检查门票使用信息
        private bool checkTicketInfo(string barCode, int projectId, out TicketModel resultModel, out string errMsg)
        {
            resultModel = new TicketModel();
            errMsg = string.Empty;

            var flw_Project_TicketInfoService = Flw_Project_TicketInfoService.N;
            var flw_ProjectTicket_BindService = Flw_ProjectTicket_BindService.N;
            var flw_ProjectTicket_EntryService = Flw_ProjectTicket_EntryService.N;

            //验证门票是否有效
            if (!(from a in flw_Project_TicketInfoService.GetModels(p => p.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase))
                  join b in flw_ProjectTicket_BindService.GetModels(p => p.ProjectID == projectId) on a.Barcode equals b.ProjectCode
                  select 1).Any())
            {
                errMsg = "该门票无效";
                return false;
            }

            //获取结果
            resultModel = (from a in flw_Project_TicketInfoService.GetModels(p => p.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase))
                               join b in flw_ProjectTicket_BindService.GetModels(p => p.ProjectID == projectId) on a.Barcode equals b.ProjectCode
                               join c in flw_ProjectTicket_EntryService.GetModels() on a.Barcode equals c.ProjectCode
                               select new TicketModel
                               {
                                   TicketName = c.TicketName,
                                   TicketType = c.TicketType,
                                   State = a.State,
                                   RemainCount = b.RemainCount ?? 0,
                                   EndTime = (DateTime?)null,
                                   Note = string.Empty
                               }).FirstOrDefault();
            //获取门票购买详情
            var flw_Project_TicketInfoModel = flw_Project_TicketInfoService.GetModels(p => p.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            //获取门票规则实体
            var flw_ProjectTicket_EntryModel = flw_ProjectTicket_EntryService.GetModels(p => p.ProjectCode.Equals(barCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            //计算生效时间, 核销时间, 有效时间, 过期时间                
            var saleTime = flw_Project_TicketInfoModel.SaleTime; //购买时间
            if (saleTime == null)
            {
                errMsg = "门票售卖时间不能为空";
                return false;
            }

            var effactTime = saleTime; //生效时间
            var writeOffDays = flw_Project_TicketInfoModel.WriteOffDays ?? 0; //核销天数
            var firstUseTime = flw_Project_TicketInfoModel.FirstUseTime; //首次使用时间
            var writeOffTime = (DateTime?)null; //核销时间
            var validTime = (DateTime?)null; //有效时间
            if (firstUseTime != null)
            {
                writeOffTime = firstUseTime.Value.AddDays(writeOffDays);
            }

            var effactType = flw_ProjectTicket_EntryModel.EffactType;
            if (effactType == (int)EffactType.Period) //有效时长
            {
                var effactPeriodType = flw_ProjectTicket_EntryModel.EffactPeriodType;
                var effactPeriodValue = flw_ProjectTicket_EntryModel.EffactPeriodValue ?? 0;
                var vaildPeriodType = flw_ProjectTicket_EntryModel.VaildPeriodType;
                var vaildPeriodValue = flw_ProjectTicket_EntryModel.VaildPeriodValue ?? 0;
                switch (effactPeriodType)
                {
                    case (int)FreqType.Day: effactTime = saleTime.Value.AddDays(effactPeriodValue); break;
                    case (int)FreqType.Week: effactTime = saleTime.Value.AddDays(effactPeriodValue * 7); break;
                    case (int)FreqType.Month: effactTime = saleTime.Value.AddDays(effactPeriodValue * 30); break;
                    case (int)FreqType.Season: effactTime = saleTime.Value.AddDays(effactPeriodValue * 90); break;
                    case (int)FreqType.Year: effactTime = saleTime.Value.AddDays(effactPeriodValue * 365); break;
                    default: errMsg = "该门票生效周期值不正确"; return false;
                }
                switch (vaildPeriodType)
                {
                    case (int)FreqType.Day: validTime = saleTime.Value.AddDays(effactPeriodValue); break;
                    case (int)FreqType.Week: validTime = saleTime.Value.AddDays(effactPeriodValue * 7); break;
                    case (int)FreqType.Month: validTime = saleTime.Value.AddDays(effactPeriodValue * 30); break;
                    case (int)FreqType.Season: validTime = saleTime.Value.AddDays(effactPeriodValue * 90); break;
                    case (int)FreqType.Year: validTime = saleTime.Value.AddDays(effactPeriodValue * 365); break;
                    default: errMsg = "该门票有效周期值不正确"; return false; 
                }
            }
            else if (effactType == (int)EffactType.Date)//指定日期
            {
                var vaildEndDate = flw_ProjectTicket_EntryModel.VaildEndDate;
                validTime = vaildEndDate;
            }
            else
            {
                errMsg = "该门票生效方式值不正确";
                return false;
            }

            resultModel.EndTime = (writeOffTime != null && writeOffTime < validTime) ? writeOffTime : validTime;

            //锁定或退票
            var state = flw_Project_TicketInfoModel.State;
            if (state == (int)TicketState.Locked || state == (int)TicketState.Returned) return true;

            if (state == (int)TicketState.UnUse || state == (int)TicketState.Used) resultModel.State = (int)TicketState.Valid;            

            //剩余次数为0不可用
            if (resultModel.RemainCount == 0)
            {
                resultModel.State = (int)TicketState.InValid;
                return true;
            }

            //未生效不可用
            if (effactTime > DateTime.Now)
            {
                resultModel.State = (int)TicketState.InValid;
                return true;
            }

            //已过期不可用
            if (resultModel.EndTime < DateTime.Now)
            {
                resultModel.State = (int)TicketState.Expired;
                return true;
            }

            //判断可用日期
            var weekType = flw_ProjectTicket_EntryModel.WeekType;
            var xC_HolidayListService = XC_HolidayListService.I;
            if (weekType == (int)TimeType.Custom)
            {
                var week = flw_ProjectTicket_EntryModel.Week ?? "";
                if (!week.Contains(Week())) resultModel.State = (int)TicketState.InValid;
            }
            else if (weekType == (int)TimeType.Workday)
            {
                if (!xC_HolidayListService.Any(a => a.DayType == 0 && System.Data.Entity.DbFunctions.DiffDays(a.WorkDay, DateTime.Now) == 0))
                    resultModel.State = (int)TicketState.InValid;
            }
            else if (weekType == (int)TimeType.Weekend)
            {
                if (!xC_HolidayListService.Any(a => a.DayType == 1 && System.Data.Entity.DbFunctions.DiffDays(a.WorkDay, DateTime.Now) == 0))
                    resultModel.State = (int)TicketState.InValid;
            }
            else if (weekType == (int)TimeType.Holiday)
            {
                if (!xC_HolidayListService.Any(a => a.DayType == 2 && System.Data.Entity.DbFunctions.DiffDays(a.WorkDay, DateTime.Now) == 0))
                    resultModel.State = (int)TicketState.InValid;
            }
            else
            {
                errMsg = "该门票周方式值不正确";
                return false;
            }                

            //判断可用时段
            var startTime = Convert.ToDateTime(Utils.TimeSpanToStr(flw_ProjectTicket_EntryModel.StartTime));
            var endTime = Convert.ToDateTime(Utils.TimeSpanToStr(flw_ProjectTicket_EntryModel.EndTime));
            var nowTime = Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss"));
            if (DateTime.Compare(startTime, nowTime) > 0 || DateTime.Compare(endTime, nowTime) < 0)
                resultModel.State = (int)TicketState.InValid;

            //判断不可用日期
            var noStateDate = flw_ProjectTicket_EntryModel.NoStartDate;
            var noEndDate = flw_ProjectTicket_EntryModel.NoEndDate;
            if (noStateDate < noEndDate)
            {
                if (DateTime.Compare(DateTime.Now, noStateDate.Value) >= 0 && DateTime.Compare(DateTime.Now, noEndDate.Value) <= 0)
                    resultModel.State = (int)TicketState.InValid;
            }

            //判断使用频率
            var ticketType = flw_Project_TicketInfoModel.TicketType;
            if (ticketType == (int)TicketType.Period) //期限票
            {
                var allowRestrict = flw_ProjectTicket_EntryModel.AllowRestrict;
                if (allowRestrict == 1) //是否使用限制
                {
                    var flw_Project_TicketUseService = Flw_Project_TicketUseService.N;
                    var data_Project_BindDeviceService = Data_Project_BindDeviceService.N;
                    var restrictShareCount = flw_ProjectTicket_EntryModel.RestrictShareCount;
                    var restrictPeriodType = flw_ProjectTicket_EntryModel.RestrictPeriodType;
                    var restrictPreiodValue = flw_ProjectTicket_EntryModel.RestrictPreiodValue;
                    var restrctCount = flw_ProjectTicket_EntryModel.RestrctCount;
                    switch (restrictPeriodType)
                    {
                        case (int)RestrictPeriodType.Hour:
                            if (restrictShareCount == 1 &&
                                flw_Project_TicketUseService.GetCount(p => System.Data.Entity.DbFunctions.DiffHours(p.InTime, DateTime.Now) <= restrictPreiodValue && p.ProjectTicketCode.Equals(barCode, StringComparison.OrdinalIgnoreCase))
                                > restrctCount) resultModel.State = (int)TicketState.InValid;
                            if (restrictShareCount != 1 &&
                                flw_Project_TicketUseService.GetCount(p => p.ProjectID == projectId && System.Data.Entity.DbFunctions.DiffHours(p.InTime, DateTime.Now) <= restrictPreiodValue && p.ProjectTicketCode.Equals(barCode, StringComparison.OrdinalIgnoreCase))
                                > restrctCount) resultModel.State = (int)TicketState.InValid;
                            break;
                        case (int)RestrictPeriodType.Day:
                            if (restrictShareCount == 1 &&
                                flw_Project_TicketUseService.GetCount(p => System.Data.Entity.DbFunctions.DiffDays(p.InTime, DateTime.Now) <= restrictPreiodValue && p.ProjectTicketCode.Equals(barCode, StringComparison.OrdinalIgnoreCase))
                                > restrctCount) resultModel.State = (int)TicketState.InValid;
                            if (restrictShareCount != 1 &&
                                flw_Project_TicketUseService.GetCount(p => p.ProjectID == projectId && System.Data.Entity.DbFunctions.DiffDays(p.InTime, DateTime.Now) <= restrictPreiodValue && p.ProjectTicketCode.Equals(barCode, StringComparison.OrdinalIgnoreCase))
                                > restrctCount) resultModel.State = (int)TicketState.InValid;
                            break;
                        default: errMsg = "该门票限制周期方式值不正确"; return false; 
                    }
                }
            }

            return true;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken)]
        public object getTicketInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string barCode = dicParas.ContainsKey("barCode") ? dicParas["barCode"].ToString() : string.Empty;
                var projectId = dicParas.ContainsKey("projectId") ? dicParas["projectId"].Toint() : (int?)null;

                if (string.IsNullOrEmpty(barCode))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入条码编号");
                }

                if (projectId.IsNull())
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请选择门票项目");
                }

                //验证是否有效门店
                StoreCacheModel storeModel = null;
                StoreBusiness storeBusiness = new StoreBusiness();
                if (!storeBusiness.IsEffectiveStore(userTokenModel.StoreId, ref storeModel, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                //检查票使用信息
                TicketModel resultModel = null;
                if (!checkTicketInfo(barCode, projectId.Value, out resultModel, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, resultModel);
                //}
                //else if (storeModel.StoreDBDeployType == 1)
                //{
                //    string sn = System.Guid.NewGuid().ToString().Replace("-","");
                //    UDPSocketCommonQueryAnswerModel answerModel = null;
                //    string radarToken = string.Empty;
                //    if (DataFactory.SendDataTicketQuery(sn, storeModel.StoreID, storeModel.StorePassword, barCode,out radarToken, out errMsg))
                //    {

                //    }
                //    else
                //    {
                //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                //    }

                //    answerModel = null;
                //    int whileCount = 0;
                //    while (answerModel == null && whileCount <= 25)
                //    {
                //        //获取应答缓存数据
                //        whileCount++;
                //        System.Threading.Thread.Sleep(1000);
                //        answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
                //    }

                //    if (answerModel != null)
                //    {
                //        TicketQueryResultNotifyRequestModel model = (TicketQueryResultNotifyRequestModel)(answerModel.Result);
                //        //移除应答缓存数据
                //        UDPSocketCommonQueryAnswerBusiness.Remove(sn);

                //        if (model.Result_Code == "1")
                //        {
                //            var obj = new
                //            {
                //                id = model.Result_Data.Id,
                //                projectName = model.Result_Data.ProjectName,
                //                state = model.Result_Data.State,
                //                projectType = model.Result_Data.ProjectType,
                //                remainCount = model.Result_Data.RemainCount,
                //                endTime = model.Result_Data.endtime
                //            };
                //            return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
                //        }
                //        else
                //        {
                //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, model.Result_Msg);
                //        }
                //    }
                //    else
                //    {
                //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "系统没有响应");
                //    }
                //}
                //else
                //{
                //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店设置错误");
                //}
            }
            catch(Exception e)
            {
                throw e;   
            }
            
        }

        private string getTicketStuatsName(int state)
        {
            switch (state)
            {
                case 0: return "未使用";
                case 1: return "已使用";
                case 2: return "被锁定";
            }
            return "";
        }

        private string getConsumptionType(int projectType)
        { 
            switch (projectType)
            {
                case 0: return "次数";
                case 1: return "有效期";
            }
            return "";
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken)]
        public object addTicket(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string barCode = dicParas.ContainsKey("barCode") ? dicParas["barCode"].ToString() : string.Empty;
                var projectId = dicParas.ContainsKey("projectId") ? dicParas["projectId"].Toint() : (int?)null;
                var useType = dicParas.ContainsKey("useType") ? dicParas["useType"].Toint() : (int?)null; //0进1出

                if (string.IsNullOrEmpty(barCode))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入条码编号");
                }

                if (projectId.IsNull())
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请选择门票项目");
                }

                if (useType.IsNull())
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请选择验票方式");
                }

                //验证是否有效门店
                StoreCacheModel storeModel = null;
                StoreBusiness storeBusiness = new StoreBusiness();
                if (!storeBusiness.IsEffectiveStore(userTokenModel.StoreId,ref storeModel,out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                //if (storeModel.StoreDBDeployType == 0)
                //{

                //检查门票使用信息
                TicketModel resultModel = null;
                if (!checkTicketInfo(barCode, projectId.Value, out resultModel, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                if (resultModel.State != (int)TicketState.Valid)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该门票已不可用");
                }

                //校验顺序
                var data_ProjectInfoService = Data_ProjectInfoService.I;
                var adjOrder = data_ProjectInfoService.GetModels(p => p.ID == projectId).Select(o => o.AdjOrder).FirstOrDefault() ?? 0;
                if (adjOrder == 1) //校验顺序开启
                {
                    if (useType == 0)
                    {
                        
                    }
                }
                else
                { }



                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);

                //}
                //else if (storeModel.StoreDBDeployType == 1)
                //{
                //    string sn = System.Guid.NewGuid().ToString().Replace("-","");
                //    UDPSocketCommonQueryAnswerModel answerModel = null;
                //    string radarToken = string.Empty;
                //    if (DataFactory.SendDataTicketOperate(sn,storeModel.StoreID, storeModel.StorePassword, barCode, "0", out radarToken,out errMsg))
                //    {

                //    }
                //    else
                //    {
                //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                //    }

                //    answerModel = null;
                //    int whileCount = 0;
                //    while (answerModel == null && whileCount <= 25)
                //    {
                //        //获取应答缓存数据
                //        whileCount++;
                //        System.Threading.Thread.Sleep(1000);
                //        answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
                //    }

                //    if (answerModel != null)
                //    {
                //        TicketOperateResultNotifyRequestModel model = (TicketOperateResultNotifyRequestModel)(answerModel.Result);
                //        //移除应答缓存数据
                //        UDPSocketCommonQueryAnswerBusiness.Remove(sn);

                //        if (model.Result_Code == "1" && model.Result_Data == "1")
                //        {
                //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                //        }
                //        else
                //        {
                //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, model.Result_Msg);
                //        }
                //    }
                //    else
                //    {
                //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "系统没有响应");
                //    }
                //}
                //else
                //{
                //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店设置错误");
                //}
            }
            catch(Exception e)
            {
                throw e;
            }

        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken)]
        public object lockTicket(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
            string barCode = dicParas.ContainsKey("barCode") ? dicParas["barCode"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(barCode))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入条码编号");
            }

            //验证是否有效门店
            StoreCacheModel storeModel = null;
            StoreBusiness storeBusiness = new StoreBusiness();
            if (!storeBusiness.IsEffectiveStore(userTokenModel.StoreId, ref storeModel, out errMsg))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }

            //if (storeModel.StoreDBDeployType == 0)
            //{
            var flw_Project_TicketInfoService = Flw_Project_TicketInfoService.I;

            if (!flw_Project_TicketInfoService.Any(a => a.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该门票信息不存在");
            }

            var flw_Project_TicketInfoModel = flw_Project_TicketInfoService.GetModels(p => p.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (flw_Project_TicketInfoModel.State == (int)TicketState.Returned)
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门票已退票");
            }

            if (flw_Project_TicketInfoModel.State == (int)TicketState.Locked)
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门票已锁定");
            }
            
            flw_Project_TicketInfoModel.State = (int)TicketState.Locked;

            if (!flw_Project_TicketInfoService.Update(flw_Project_TicketInfoModel))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门票锁定出错");
            }

            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            //}
            //else if (storeModel.StoreDBDeployType == 1)
            //{
            //    string sn = System.Guid.NewGuid().ToString().Replace("-", "");
            //    UDPSocketCommonQueryAnswerModel answerModel = null;
            //    string radarToken = string.Empty;
            //    if (DataFactory.SendDataTicketOperate(sn, storeModel.StoreID, storeModel.StorePassword, barCode, "2", out radarToken,out errMsg))
            //    {

            //    }
            //    else
            //    {
            //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            //    }

            //    answerModel = null;
            //    int whileCount = 0;
            //    while (answerModel == null && whileCount <= 25)
            //    {
            //        //获取应答缓存数据
            //        whileCount++;
            //        System.Threading.Thread.Sleep(1000);
            //        answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
            //    }

            //    if (answerModel != null)
            //    {
            //        TicketOperateResultNotifyRequestModel model = (TicketOperateResultNotifyRequestModel)(answerModel.Result);
            //        //移除应答缓存数据
            //        UDPSocketCommonQueryAnswerBusiness.Remove(sn);

            //        if (model.Result_Code == "1" && model.Result_Data == "1")
            //        {
            //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            //        }
            //        else
            //        {
            //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, model.Result_Msg);
            //        }
            //    }
            //    else
            //    {
            //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "系统没有响应");
            //    }
            //}
            //else
            //{
            //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "系统没有响应");
            //}
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken)]
        public object unlockTicket(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string barCode = dicParas.ContainsKey("barCode") ? dicParas["barCode"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(barCode))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入条码编号");
                }

                //验证是否有效门店
                StoreCacheModel storeModel = null;
                StoreBusiness storeBusiness = new StoreBusiness();
                if (!storeBusiness.IsEffectiveStore(userTokenModel.StoreId, ref storeModel, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                //if (storeModel.StoreDBDeployType == 0)
                //{ 
                var flw_Project_TicketInfoService = Flw_Project_TicketInfoService.I;
                var flw_Project_TicketUseService = Flw_Project_TicketUseService.I;

                if (!flw_Project_TicketInfoService.Any(a => a.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该门票信息不存在");
                }

                var flw_Project_TicketInfoModel = flw_Project_TicketInfoService.GetModels(p => p.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (flw_Project_TicketInfoModel.State != (int)TicketState.Returned)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门票已退票");
                }

                if (flw_Project_TicketInfoModel.State != (int)TicketState.Locked)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门票未锁定");
                }

                flw_Project_TicketInfoModel.State = flw_Project_TicketUseService.Any(a => a.ProjectTicketCode.Equals(barCode, StringComparison.OrdinalIgnoreCase)) ? (int)TicketState.Used : (int)TicketState.UnUse;

                if (!flw_Project_TicketInfoService.Update(flw_Project_TicketInfoModel))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门票解锁出错");
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");             
                //}
                //else if (storeModel.StoreDBDeployType == 1)
                //{
                //    string sn = System.Guid.NewGuid().ToString().Replace("-", "");
                //    UDPSocketCommonQueryAnswerModel answerModel = null;
                //    string radarToken = string.Empty;
                //    if (DataFactory.SendDataTicketOperate(sn,storeModel.StoreID, storeModel.StorePassword, barCode, "1",out radarToken, out errMsg))
                //    {

                //    }
                //    else
                //    {
                //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                //    }

                //    answerModel = null;
                //    int whileCount = 0;
                //    while (answerModel == null && whileCount <= 25)
                //    {
                //        //获取应答缓存数据
                //        whileCount++;
                //        System.Threading.Thread.Sleep(1000);
                //        answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
                //    }

                //    if (answerModel != null)
                //    {
                //        TicketOperateResultNotifyRequestModel model = (TicketOperateResultNotifyRequestModel)(answerModel.Result);
                //        //移除应答缓存数据
                //        UDPSocketCommonQueryAnswerBusiness.Remove(sn);

                //        if (model.Result_Code == "1" && model.Result_Data == "1")
                //        {
                //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                //        }
                //        else
                //        {
                //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, model.Result_Msg);
                //        }
                //    }
                //    else
                //    {
                //        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "系统没有响应");
                //    }
                //}
                //else
                //{
                //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店设置错误");
                //}
            }
            catch(Exception e)
            {
                throw e;   
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken)]
        public object exchangeOutTicket(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            decimal money = 0;
            XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
            string barCode = dicParas.ContainsKey("barCode") ? dicParas["barCode"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string mobileName = dicParas.ContainsKey("mobileName") ? dicParas["mobileName"].ToString() : string.Empty;
            string moneyStr = dicParas.ContainsKey("money") ? dicParas["money"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(barCode))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入条码编号");
            }

            if (string.IsNullOrEmpty(icCardId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入会员卡号");
            }

            if (!String.IsNullOrEmpty(moneyStr) && !Utils.IsDecimal(moneyStr))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑币金额无效");
            }

            StoreCacheModel storeModel = null;
            StoreBusiness storeBusiness = new StoreBusiness();
            if (!storeBusiness.IsEffectiveStore(userTokenModel.StoreId, ref storeModel, out errMsg))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }

            if (storeModel.StoreDBDeployType == 0)
            {
                XCCloudService.BLL.IBLL.XCGame.IFlwTicketExitService flwTicketExitService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IFlwTicketExitService>(storeModel.StoreDBName);
                XCCloudService.BLL.IBLL.XCGame.IMemberService memberService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IMemberService>(storeModel.StoreDBName);
                XCCloudService.BLL.IBLL.XCGame.IScheduleService scheduleService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IScheduleService>(storeModel.StoreDBName);
                XCCloudService.BLL.IBLL.XCGame.IParametersService parametersService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IParametersService>(storeModel.StoreDBName);

                var memberModel = memberService.GetModels(p => p.ICCardID.ToString().Equals(icCardId)).FirstOrDefault<t_member>();
                if (memberModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员信息不存在");
                }

                System.DateTime startTime = System.DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                System.DateTime endTime = System.DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
                var scheduleModel = scheduleService.GetModels(p => (p.OpenTime >= startTime && p.OpenTime <= endTime && p.UserID == userTokenModel.UserId && p.State.Equals("0"))).FirstOrDefault<flw_schedule>();
                if (scheduleModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户班次信息不存在");
                }

                var flwTicketExitModel = flwTicketExitService.GetModels(p => p.Barcode.Equals(barCode)).FirstOrDefault<flw_ticket_exit>();
                if (flwTicketExitModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "出票信息不存在");
                }

                if (flwTicketExitModel.State == 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "票已兑换");
                }

                if (flwTicketExitModel.isNoAllow == 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "票禁止兑换");
                }

                var paramCoinPriceModel = parametersService.GetModels(p => p.System.Equals("txtCoinPrice", StringComparison.OrdinalIgnoreCase)).FirstOrDefault<t_parameters>();
                var paramDateValidityModel = parametersService.GetModels(p => p.System.Equals("rbnBackDateValidity", StringComparison.OrdinalIgnoreCase)).FirstOrDefault<t_parameters>();

                if (paramCoinPriceModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑币单价参数未设置，不能兑换");
                }

                if (paramDateValidityModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "返分卡有效期参数未设置，不能兑换");
                }

                if (flwTicketExitModel.Coins <= 0)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "票兑换金额无效，不能兑换");
                }

                decimal exCoinMoney = Convert.ToDecimal(flwTicketExitModel.Coins) * decimal.Parse(paramCoinPriceModel.ParameterValue);

                DateTime dateTime = Convert.ToDateTime(flwTicketExitModel.RealTime).AddDays(Convert.ToDouble(paramDateValidityModel.ParameterValue));
                if (System.DateTime.Now > dateTime)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "已过期，不能兑换");
                }

                using (TransactionScope ts = new TransactionScope())
                {
                    memberModel.Balance += flwTicketExitModel.Coins;
                    memberModel.ModTime = System.DateTime.Now;
                    memberService.Update(memberModel);

                    flwTicketExitModel.ICCardID = int.Parse(icCardId);
                    flwTicketExitModel.ChargeTime = System.DateTime.Now;
                    flwTicketExitModel.WorkStation = userTokenModel.Mobile;
                    flwTicketExitModel.MacAddress = userTokenModel.Mobile;
                    flwTicketExitModel.DiskID = userTokenModel.Mobile;
                    flwTicketExitModel.Note = "小程序兑换";
                    flwTicketExitModel.CoinMoney = exCoinMoney;
                    flwTicketExitModel.UserID = userTokenModel.UserId.ToString();
                    flwTicketExitModel.State = 1;
                    flwTicketExitModel.ScheduleID = scheduleModel.ID;
                    flwTicketExitService.Update(flwTicketExitModel);
                    ts.Complete();
                }
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            else if (storeModel.StoreDBDeployType == 1)
            {
                string sn = System.Guid.NewGuid().ToString().Replace("-", "");
                UDPSocketCommonQueryAnswerModel answerModel = null;
                string radarToken = string.Empty;
                if (DataFactory.SendDataOutTicketOperate(sn, storeModel.StoreID, storeModel.StorePassword, barCode, icCardId, mobileName, userTokenModel.Mobile, money, "0", out radarToken, out errMsg))
                {

                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                answerModel = null;
                int whileCount = 0;
                while (answerModel == null && whileCount <= 25)
                {
                    //获取应答缓存数据
                    whileCount++;
                    System.Threading.Thread.Sleep(1000);
                    answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
                }

                if (answerModel != null)
                {
                    OutTicketOperateResultNotifyRequestModel model = (OutTicketOperateResultNotifyRequestModel)(answerModel.Result);
                    //移除应答缓存数据
                    UDPSocketCommonQueryAnswerBusiness.Remove(sn);

                    if (model.Result_Code == "1")
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                    }
                    else
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, model.Result_Msg);
                    }
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "系统没有响应");
                }
            }
            else
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "门店设置错误");
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken)]
        public object getOutTicket(Dictionary<string, object> dicParas)
        {
            XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
            string barCode = dicParas.ContainsKey("barCode") ? dicParas["barCode"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(barCode))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入条码编号");
            }

            StoreBusiness store = new StoreBusiness();
            string errMsg = string.Empty;
            StoreCacheModel storeModel = null;
            StoreBusiness storeBusiness = new StoreBusiness();
            if (!storeBusiness.IsEffectiveStore(userTokenModel.StoreId, ref storeModel, out errMsg))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }

            if (string.IsNullOrEmpty(barCode))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入票编号");
            }

            if (storeModel.StoreDBDeployType == 0)
            {
                XCCloudService.BLL.IBLL.XCGame.IParametersService parametersService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IParametersService>(storeModel.StoreDBName);
                var paramDateValidityModel = parametersService.GetModels(p => p.System.Equals("rbnBackDateValidity", StringComparison.OrdinalIgnoreCase)).FirstOrDefault<t_parameters>();
                if (paramDateValidityModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "返分卡有效期参数未设置，不能兑换");
                }

                var paramCoinPriceModel = parametersService.GetModels(p => p.System.Equals("txtCoinPrice", StringComparison.OrdinalIgnoreCase)).FirstOrDefault<t_parameters>();
                if (paramCoinPriceModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑币单价参数未设置，不能兑换");
                }
                decimal exchangeCoinPrice = decimal.Parse(paramCoinPriceModel.ParameterValue);

                XCCloudService.BLL.IBLL.XCGame.IFlwTicketExitService flwTicketExitService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IFlwTicketExitService>(storeModel.StoreDBName);
                var flwTicketExitModel = flwTicketExitService.GetModels(p => p.Barcode.Equals(barCode,StringComparison.OrdinalIgnoreCase)).FirstOrDefault<flw_ticket_exit>();
                if (flwTicketExitModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "出票信息不存在");
                }

                XCCloudService.BLL.IBLL.XCGame.IHeadService headService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IHeadService>(storeModel.StoreDBName);
                var headModel = headService.GetModels(p => p.HeadAddress.Equals(flwTicketExitModel.HeadAddress) && p.Segment.Equals(flwTicketExitModel.Segment)).FirstOrDefault<t_head>();
                if (headModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "彩票出票设备信息不存在");
                }

                XCCloudService.BLL.IBLL.XCGame.IGameService gameService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IGameService>(storeModel.StoreDBName);
                var gameModel = gameService.GetModels(p => p.GameID.Equals(headModel.GameID)).FirstOrDefault<t_game>();
                if (gameModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "彩票对应游戏信息不存在");
                }

                int state = Convert.ToInt32(flwTicketExitModel.State);
                string stateName = flwTicketExitModel.State == 1 ? "已兑" : "未兑";
                if (state != 1)
                {
                    DateTime dateTime = Convert.ToDateTime(flwTicketExitModel.RealTime).AddDays(Convert.ToDouble(paramDateValidityModel.ParameterValue));
                    if (System.DateTime.Now > dateTime)
                    {
                        state = 2;
                        stateName = "已过期";
                    }

                    if (flwTicketExitModel.isNoAllow == 1)
                    {
                        state = 3;
                        stateName = "被锁定";
                    }
                }

                var obj = new {
                    id = flwTicketExitModel.ID,
                    coin = flwTicketExitModel.Coins,
                    gameName = gameModel.GameName,
                    headInfo = headModel.HeadID,
                    state = stateName,
                    makeTime = Convert.ToDateTime(flwTicketExitModel.RealTime).ToString("yyyy-MM-dd HH:mm:ss"),
                    exchangeCoinPrice = exchangeCoinPrice
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else if (storeModel.StoreDBDeployType == 1)
            {
                string txtCoinPrice = string.Empty;
                string txtTicketDate = string.Empty;
                ParamQueryResultModel paramQueryResultModel = null;
                if (UDPApiService.GetParam(userTokenModel.StoreId, "0", ref paramQueryResultModel, out errMsg))
                {
                    txtCoinPrice = paramQueryResultModel.TxtCoinPrice;
                    txtTicketDate = paramQueryResultModel.TxtTicketDate;
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg); 
                }

                string sn = System.Guid.NewGuid().ToString().Replace("-", "");
                UDPSocketCommonQueryAnswerModel answerModel = null;
                string radarToken = string.Empty;
                if (DataFactory.SendDataOutTicketQuery(sn,storeModel.StoreID, storeModel.StorePassword, barCode,out radarToken,out errMsg))
                {

                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                answerModel = null;
                int whileCount = 0;
                while (answerModel == null && whileCount <= 25)
                {
                    //获取应答缓存数据
                    whileCount++;
                    System.Threading.Thread.Sleep(1000);
                    answerModel = UDPSocketCommonQueryAnswerBusiness.GetAnswerModel(sn, 1);
                }

                if (answerModel != null)
                {
                    OutTicketQueryResultNotifyRequestModel model = (OutTicketQueryResultNotifyRequestModel)(answerModel.Result);
                    //移除应答缓存数据
                    UDPSocketCommonQueryAnswerBusiness.Remove(sn);

                    if (model.Result_Code == "1")
                    {
                        var obj = new
                        {
                            id = model.Result_Data.Id,
                            coin = model.Result_Data.Coins,
                            gameName = model.Result_Data.GameName,
                            headInfo = model.Result_Data.HeadInfo,
                            state = model.Result_Data.State,
                            makeTime = model.Result_Data.PrintDate,
                            exchangeCoinPrice = txtCoinPrice
                        };

                        return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
                    }
                    else
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, model.Result_Msg);
                    }
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "系统没有响应");
                }
            }
            else
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店设置错误");
            }
        }
    }
}