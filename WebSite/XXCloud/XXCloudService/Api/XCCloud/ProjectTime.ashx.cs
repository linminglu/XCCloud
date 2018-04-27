using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Business.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.DAL;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser, StoreUser")]
    /// <summary>
    /// ProjectTime 的摘要说明
    /// </summary>
    public class ProjectTime : ApiBase
    {
        private bool saveBandPrice(int iId, object[] bandPrices, out string errMsg)
        {
            errMsg = string.Empty;
            if (bandPrices != null && bandPrices.Count() >= 0)
            {
                //先删除，后添加
                foreach (var model in Data_ProjectTime_BandPriceBiz.I.GetModels(p => p.ProjectTimeID == iId))
                {
                    Data_ProjectTime_BandPriceBiz.I.DeleteModel(model);
                }

                var bandPriceList = new List<Data_ProjectTime_BandPrice>();
                foreach (IDictionary<string, object> el in bandPrices)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        string memberLevelIDs = dicPara.Get("memberLevelIDs");
                        string bandType = dicPara.Get("bandType");
                        string bandCount = dicPara.Get("bandCount");
                        string balanceType = dicPara.Get("balanceType");
                        string count = dicPara.Get("count");

                        if (!memberLevelIDs.Nonempty("适用级别", out errMsg)) return false;
                        if (!bandType.Nonempty("档位类别", out errMsg)) return false;
                        if (!balanceType.Nonempty("计费类型", out errMsg)) return false;
                        if (!bandCount.Illegalint("档位数量", out errMsg)) return false;
                        if (!count.Illegalint("扣除数量", out errMsg)) return false;

                        List<string> memberLevelIDList = memberLevelIDs.Split('|').ToList();
                        foreach (var memberLevelID in memberLevelIDList)
                        {
                            var data_ProjectTime_BandPrice = new Data_ProjectTime_BandPrice();
                            data_ProjectTime_BandPrice.ProjectTimeID = iId;
                            data_ProjectTime_BandPrice.MemberLevelID = memberLevelID.Toint();
                            data_ProjectTime_BandPrice.BandType = bandType.Toint();
                            data_ProjectTime_BandPrice.BandCount = bandCount.Toint();
                            data_ProjectTime_BandPrice.BalanceType = balanceType.Toint();
                            data_ProjectTime_BandPrice.Count = count.Toint();
                            bandPriceList.Add(data_ProjectTime_BandPrice);
                            Data_ProjectTime_BandPriceBiz.I.AddModel(data_ProjectTime_BandPrice);
                        }
                    }
                    else
                    {
                        errMsg = "提交数据包含空对象";
                        return false;
                    }
                }

                //同一级别，同一档位，同一计费类型的扣除规则必须唯一 
                if (bandPriceList.GroupBy(g => new { g.MemberLevelID, g.BandType, g.BandCount, g.BalanceType }).Select(o => new { Count = o.Count() }).Any(p => p.Count > 1))
                {
                    errMsg = "同一级别，同一档位，同一计费类型的扣除规则必须唯一";
                    return false;
                }

                if (!Data_ProjectTime_BandPriceBiz.I.SaveChanges())
                {
                    errMsg = "保存门票绑定信息失败";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取本门店项目
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private IQueryable getSutiableList(string storeId)
        {
            return from a in Data_ProjectTime_StoreListBiz.NI.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                   join b in Data_ProjectTimeInfoBiz.NI.GetModels() on a.ProjectTimeID equals b.ID
                   select b;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectInfoList(Dictionary<string, object> dicParas)
        {            
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var query = Data_ProjectTimeInfoBiz.NI.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
                if (userTokenKeyModel.LogType == (int)RoleType.StoreUser)
                {
                    query = (IQueryable<Data_ProjectTimeInfo>)getSutiableList(storeId);
                }

                var linq = from a in query
                           join b in Dict_BalanceTypeBiz.NI.GetModels(p=>p.State == 1) on a.DepositType equals b.ID into b1
                           from b in b1.DefaultIfEmpty()
                           select new
                           {
                               ID = a.ID,
                               ProjectName = a.ProjectName,
                               PayCycle = a.PayCycle,
                               Deposit = a.Deposit,
                               BackTime = a.BackTime,                               
                               DepositType = a.DepositType,
                               DepositTypeStr = b != null ? b.TypeName : string.Empty,
                               Note = a.Note
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var linq = from a in Data_ProjectTimeInfoBiz.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                           select new
                           {
                               ID = a.ID,
                               ProjectName = a.ProjectName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                if(id == 0)
                {
                    errMsg = "项目ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                if (!Data_ProjectTimeInfoBiz.I.Any(p => p.ID == id))
                {
                    errMsg = "该项目不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int BandTypeId = Dict_SystemBiz.I.GetModels(p => p.DictKey.Equals("档位类别") && p.PID == 0).FirstOrDefault().ID;
                var BandPrices = from e in (from a in Data_ProjectTime_BandPriceBiz.NI.GetModels(p => p.ProjectTimeID == id)
                                join b in Data_MemberLevelBiz.NI.GetModels(p => p.State == 1) on a.MemberLevelID equals b.MemberLevelID into b1
                                from b in b1.DefaultIfEmpty()
                                join c in Dict_BalanceTypeBiz.NI.GetModels(p => p.State == 1) on a.BalanceType equals c.ID into c1
                                from c in c1.DefaultIfEmpty()
                                join d in Dict_SystemBiz.NI.GetModels(p => p.PID == BandTypeId) on (a.BandType + "") equals d.DictValue into d1
                                from d in d1.DefaultIfEmpty()
                                orderby a.BandType, a.BandCount, a.BalanceType                                
                                select new 
                                {
                                    a = a,
                                    MemberLevelID = b.MemberLevelID,
                                    MemberLevelName = b.MemberLevelName,
                                    BalanceTypeStr = c != null ? c.TypeName : string.Empty,
                                    BandTypeStr = d != null ? d.DictKey : string.Empty,
                                }).AsEnumerable()
                                group e by new { e.a.BandType, e.a.BandCount, e.a.BalanceType } into g
                                select new
                                {
                                    MemberLevelIDs = string.Join("|", g.OrderBy(o => o.MemberLevelID).Select(s => s.MemberLevelID)),
                                    MemberLevelNames = string.Join("|", g.OrderBy(o => o.MemberLevelName).Select(s => s.MemberLevelName)),
                                    BandType = g.Key.BandType,
                                    BandTypeStr = g.FirstOrDefault().BandTypeStr,
                                    BandCount = g.Key.BandCount,
                                    BalanceType = g.Key.BalanceType,
                                    BalanceTypeStr = g.FirstOrDefault().BalanceTypeStr,
                                    Count = g.FirstOrDefault().a.Count,                                    
                                };

                var data_ProjectTimeInfo = (from a in Data_ProjectTimeInfoBiz.NI.GetModels(p => p.ID == id)
                                           join b in Dict_BalanceTypeBiz.NI.GetModels(p => p.State == 1) on a.DepositType equals b.ID into b1
                                           from b in b1.DefaultIfEmpty()
                                           select new
                                           {
                                               ID = a.ID,
                                               ProjectName = a.ProjectName,
                                               PayCycle = a.PayCycle,
                                               Deposit = a.Deposit,
                                               BackTime = a.BackTime,
                                               DepositType = a.DepositType,
                                               DepositTypeStr = b != null ? b.TypeName : string.Empty,
                                               Note = a.Note
                                           }).FirstOrDefault();

                var linq = new
                           {
                               ID = data_ProjectTimeInfo.ID,
                               ProjectName = data_ProjectTimeInfo.ProjectName,
                               PayCycle = data_ProjectTimeInfo.PayCycle,
                               Deposit = data_ProjectTimeInfo.Deposit,
                               BackTime = data_ProjectTimeInfo.BackTime,
                               DepositType = data_ProjectTimeInfo.DepositType,
                               DepositTypeStr = data_ProjectTimeInfo.DepositTypeStr,
                               Note = data_ProjectTimeInfo.Note,
                               BandPrices = BandPrices
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }     

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveProjectInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                string id = dicParas.Get("id");
                string projectName = dicParas.ContainsKey("projectName") ? (dicParas["projectName"] + "") : string.Empty;
                string payCycle = dicParas.ContainsKey("payCycle") ? (dicParas["payCycle"] + "") : string.Empty;
                string depositType = dicParas.ContainsKey("depositType") ? (dicParas["depositType"] + "") : string.Empty;
                string deposit = dicParas.ContainsKey("deposit") ? (dicParas["deposit"] + "") : string.Empty;
                string backTime = dicParas.ContainsKey("backTime") ? (dicParas["backTime"] + "") : string.Empty;                
                string note = dicParas.ContainsKey("note") ? (dicParas["note"] + "") : string.Empty;
                object[] bandPrices = dicParas.ContainsKey("bandPrices") ? (object[])dicParas["bandPrices"] : null; 
                int iId = 0;
                int.TryParse(id, out iId);

                #region 验证参数

                if (string.IsNullOrEmpty(projectName))
                {
                    errMsg = "项目名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(payCycle))
                {
                    errMsg = "计费周期不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(payCycle) || Convert.ToInt32(payCycle) < 0)
                {
                    errMsg = "计费周期格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(depositType))
                {
                    errMsg = "预扣押金类别不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(deposit))
                {
                    errMsg = "预扣押金不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(deposit) || Convert.ToInt32(deposit) < 0)
                {
                    errMsg = "预扣押金格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(backTime))
                {
                    errMsg = "后悔时间不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(backTime) || Convert.ToInt32(backTime) < 0)
                {
                    errMsg = "后悔时间格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                          

                #endregion                

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (Data_ProjectTimeInfoBiz.I.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && 
                            a.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase) && a.ID != iId))
                        {
                            errMsg = "该项目名称已存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_ProjectTimeInfo = new Data_ProjectTimeInfo();
                        data_ProjectTimeInfo.ID = iId;
                        data_ProjectTimeInfo.ProjectName = projectName;
                        data_ProjectTimeInfo.PayCycle = ObjectExt.Toint(payCycle);
                        data_ProjectTimeInfo.DepositType = ObjectExt.Toint(depositType);
                        data_ProjectTimeInfo.Deposit = ObjectExt.Toint(deposit);
                        data_ProjectTimeInfo.BackTime = ObjectExt.Toint(backTime);
                        data_ProjectTimeInfo.Note = note;
                        data_ProjectTimeInfo.MerchID = merchId;
                        if (!Data_ProjectTimeInfoBiz.I.Any(a => a.ID == iId))
                        {
                            //新增
                            if (!Data_ProjectTimeInfoBiz.I.Add(data_ProjectTimeInfo))
                            {
                                errMsg = "添加计时项目信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            //修改
                            if (!Data_ProjectTimeInfoBiz.I.Update(data_ProjectTimeInfo))
                            {
                                errMsg = "修改计时项目信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iId = data_ProjectTimeInfo.ID;

                        //保存波段设定
                        if (!saveBandPrice(iId, bandPrices, out errMsg))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
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
        public object DeleteProjectInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                string projectIds = dicParas.ContainsKey("projectIds") ? (dicParas["projectIds"] + "") : string.Empty;

                if (string.IsNullOrEmpty(projectIds))
                {
                    errMsg = "项目ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        List<string> projectIdList = projectIds.Split('|').ToList();
                        foreach (var projectId in projectIdList)
                        {
                            if (string.IsNullOrEmpty(projectId))
                            {
                                errMsg = "项目ID不能为空";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            int iProjectId = Convert.ToInt32(projectId);
                            var data_ProjectTimeInfoModel = Data_ProjectTimeInfoBiz.NI.GetModels(p => p.ID == iProjectId).FirstOrDefault();
                            if (data_ProjectTimeInfoModel == null)
                            {
                                errMsg = "项目ID" + projectId + "不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            Data_ProjectTimeInfoBiz.I.DeleteModel(data_ProjectTimeInfoModel);

                            foreach (var model in Data_ProjectTime_StoreListBiz.NI.GetModels(p=>p.ProjectTimeID == iProjectId))
                            {
                                Data_ProjectTime_StoreListBiz.NI.DeleteModel(model);
                            }

                            foreach (var model in Data_ProjectTime_BandPriceBiz.NI.GetModels(p => p.ProjectTimeID == iProjectId))
                            {
                                Data_ProjectTime_BandPriceBiz.NI.DeleteModel(model);
                            }
                        }

                        if (!Data_ProjectTimeInfoBiz.NI.SaveChanges())
                        {
                            errMsg = "删除项目失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
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
        public object GetProjectStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                var projectId = dicParas.Get("projectId").Toint(0);

                if (projectId == 0)
                {
                    errMsg = "项目ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var storeIDs = Data_ProjectTime_StoreListBiz.I.GetModels(p => p.ProjectTimeID == projectId).Select(o => new { StoreID = o.StoreID });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, storeIDs);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveProjectStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string projectId = dicParas.ContainsKey("projectId") ? (dicParas["projectId"] + "") : string.Empty;
                string storeIds = dicParas.ContainsKey("storeIds") ? (dicParas["storeIds"] + "") : string.Empty;

                if (string.IsNullOrEmpty(projectId))
                {
                    errMsg = "项目ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        int iProjectId = Convert.ToInt32(projectId);
                        foreach (var model in Data_ProjectTime_StoreListBiz.I.GetModels(p => p.ProjectTimeID == iProjectId))
                        {
                            Data_ProjectTime_StoreListBiz.I.DeleteModel(model);
                        }

                        var storeIdArr = storeIds.Split('|');
                        foreach (var storeId in storeIdArr)
                        {
                            var model = new Data_ProjectTime_StoreList();
                            model.ProjectTimeID = iProjectId;
                            model.StoreID = storeId;
                            Data_ProjectTime_StoreListBiz.I.AddModel(model);
                        }

                        if (!Data_ProjectTime_StoreListBiz.I.SaveChanges())
                        {
                            errMsg = "更新计时项目适用门店表失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
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