using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
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
        IData_ProjectTimeInfoService data_ProjectTimeInfoService = BLLContainer.Resolve<IData_ProjectTimeInfoService>(resolveNew: true);
        IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>(resolveNew: true);
        IData_ProjectTime_BandPriceService data_ProjectTime_BandPriceService = BLLContainer.Resolve<IData_ProjectTime_BandPriceService>(resolveNew: true);
        IData_ProjectTime_StoreListService data_ProjectTime_StoreListService = BLLContainer.Resolve<IData_ProjectTime_StoreListService>(resolveNew: true);
        IData_MemberLevelService data_MemberLevelService = BLLContainer.Resolve<IData_MemberLevelService>(resolveNew: true);
        IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);

        private bool saveBandPrice(int iId, object[] bandPrices, out string errMsg)
        {
            errMsg = string.Empty;
            if (bandPrices != null && bandPrices.Count() >= 0)
            {
                //先删除，后添加
                foreach (var model in data_ProjectTime_BandPriceService.GetModels(p => p.ProjectTimeID == iId))
                {
                    data_ProjectTime_BandPriceService.DeleteModel(model);
                }

                var bandPriceList = new List<Data_ProjectTime_BandPrice>();
                foreach (IDictionary<string, object> el in bandPrices)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        string memberLevelIDs = dicPara.ContainsKey("memberLevelIDs") ? dicPara["memberLevelIDs"].ToString() : string.Empty;
                        string bandType = dicPara.ContainsKey("bandType") ? (dicPara["bandType"] + "") : string.Empty;
                        string bandCount = dicPara.ContainsKey("bandCount") ? (dicPara["bandCount"] + "") : string.Empty;
                        string balanceType = dicPara.ContainsKey("balanceType") ? (dicPara["balanceType"] + "") : string.Empty;
                        string count = dicPara.ContainsKey("count") ? (dicPara["count"] + "") : string.Empty;

                        #region 验证参数
                        if (string.IsNullOrEmpty(memberLevelIDs))
                        {
                            errMsg = "适用级别不能为空";
                            return false;
                        }
                        if (string.IsNullOrEmpty(bandType))
                        {
                            errMsg = "档位类别不能为空";
                            return false;
                        }
                        if (string.IsNullOrEmpty(bandCount))
                        {
                            errMsg = "档位数量不能为空";
                            return false;
                        }
                        if (!Utils.isNumber(bandCount) || Convert.ToInt32(bandCount) < 0)
                        {
                            errMsg = "档位数量格式不正确，须为非负整数";
                            return false;
                        }
                        if (string.IsNullOrEmpty(balanceType))
                        {
                            errMsg = "计费类型不能为空";
                            return false;
                        }
                        if (string.IsNullOrEmpty(count))
                        {
                            errMsg = "扣除数量不能为空";
                            return false;
                        }
                        if (!Utils.isNumber(count) || Convert.ToInt32(count) < 0)
                        {
                            errMsg = "扣除数量格式不正确，须为非负整数";
                            return false;
                        }
                        #endregion

                        List<string> memberLevelIDList = memberLevelIDs.Split('|').ToList();
                        foreach (var memberLevelID in memberLevelIDList)
                        {
                            var data_ProjectTime_BandPrice = new Data_ProjectTime_BandPrice();
                            data_ProjectTime_BandPrice.ProjectTimeID = iId;
                            data_ProjectTime_BandPrice.MemberLevelID = ObjectExt.Toint(memberLevelID);
                            data_ProjectTime_BandPrice.BandType = ObjectExt.Toint(bandType);
                            data_ProjectTime_BandPrice.BandCount = ObjectExt.Toint(bandCount);
                            data_ProjectTime_BandPrice.BalanceType = ObjectExt.Toint(balanceType);
                            data_ProjectTime_BandPrice.Count = ObjectExt.Toint(count);
                            bandPriceList.Add(data_ProjectTime_BandPrice);
                            data_ProjectTime_BandPriceService.AddModel(data_ProjectTime_BandPrice);
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

                if (!data_ProjectTime_BandPriceService.SaveChanges())
                {
                    errMsg = "保存门票绑定信息失败";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取适用门店的主体数据
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private IQueryable getSutiableList(string storeId)
        {
            return from a in data_ProjectTime_StoreListService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                   join b in data_ProjectTimeInfoService.GetModels() on a.ProjectTimeID equals b.ID
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
                var query = data_ProjectTimeInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
                if (userTokenKeyModel.LogType == (int)RoleType.StoreUser)
                {
                    query = (IQueryable<Data_ProjectTimeInfo>)getSutiableList(storeId);
                }

                var linq = from a in query
                           join b in dict_BalanceTypeService.GetModels(p=>p.State == 1) on a.DepositType equals b.ID into b1
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

                IData_ProjectTimeInfoService data_ProjectTimeInfoService = BLLContainer.Resolve<IData_ProjectTimeInfoService>();
                var linq = from a in data_ProjectTimeInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
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
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "项目ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);
                if (!data_ProjectTimeInfoService.Any(p => p.ID == iId))
                {
                    errMsg = "该项目不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int BandTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("档位类别") && p.PID == 0).FirstOrDefault().ID;
                var BandPrices = from e in (from a in data_ProjectTime_BandPriceService.GetModels(p => p.ProjectTimeID == iId)
                                join b in data_MemberLevelService.GetModels(p => p.State == 1) on a.MemberLevelID equals b.MemberLevelID into b1
                                from b in b1.DefaultIfEmpty()
                                join c in dict_BalanceTypeService.GetModels(p => p.State == 1) on a.BalanceType equals c.ID into c1
                                from c in c1.DefaultIfEmpty()
                                join d in dict_SystemService.GetModels(p => p.PID == BandTypeId) on (a.BandType + "") equals d.DictValue into d1
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
                                    BandTypeStr = g.FirstOrDefault().BalanceTypeStr,
                                    BandCount = g.Key.BandCount,
                                    BalanceType = g.Key.BalanceType,
                                    BalanceTypeStr = g.FirstOrDefault().BalanceTypeStr,
                                    Count = g.FirstOrDefault().a.Count,                                    
                                };

                var data_ProjectTimeInfo = (from a in data_ProjectTimeInfoService.GetModels(p => p.ID == iId)
                                           join b in dict_BalanceTypeService.GetModels(p => p.State == 1) on a.DepositType equals b.ID into b1
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
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
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
                        if (data_ProjectTimeInfoService.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && 
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
                        if (!data_ProjectTimeInfoService.Any(a => a.ID == iId))
                        {
                            //新增
                            if (!data_ProjectTimeInfoService.Add(data_ProjectTimeInfo))
                            {
                                errMsg = "添加计时项目信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            //修改
                            if (!data_ProjectTimeInfoService.Update(data_ProjectTimeInfo))
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
                            var data_ProjectTimeInfoModel = data_ProjectTimeInfoService.GetModels(p => p.ID == iProjectId).FirstOrDefault();
                            if (data_ProjectTimeInfoModel == null)
                            {
                                errMsg = "项目ID" + projectId + "不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            data_ProjectTimeInfoService.DeleteModel(data_ProjectTimeInfoModel);

                            foreach (var model in data_ProjectTime_StoreListService.GetModels(p=>p.ProjectTimeID == iProjectId))
                            {
                                data_ProjectTime_StoreListService.DeleteModel(model);
                            }

                            foreach (var model in data_ProjectTime_BandPriceService.GetModels(p => p.ProjectTimeID == iProjectId))
                            {
                                data_ProjectTime_BandPriceService.DeleteModel(model);
                            }
                        }

                        if (!data_ProjectTimeInfoService.SaveChanges())
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
                string projectId = dicParas.ContainsKey("projectId") ? (dicParas["projectId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(projectId))
                {
                    errMsg = "项目ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iProjectId = Convert.ToInt32(projectId);
                var storeIDs = data_ProjectTime_StoreListService.GetModels(p => p.ProjectTimeID == iProjectId).Select(o => new { StoreID = o.StoreID });
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
                        foreach (var model in data_ProjectTime_StoreListService.GetModels(p => p.ProjectTimeID == iProjectId))
                        {
                            data_ProjectTime_StoreListService.DeleteModel(model);
                        }

                        var storeIdArr = storeIds.Split('|');
                        foreach (var storeId in storeIdArr)
                        {
                            var model = new Data_ProjectTime_StoreList();
                            model.ProjectTimeID = iProjectId;
                            model.StoreID = storeId;
                            data_ProjectTime_StoreListService.AddModel(model);
                        }

                        if (!data_ProjectTime_StoreListService.SaveChanges())
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