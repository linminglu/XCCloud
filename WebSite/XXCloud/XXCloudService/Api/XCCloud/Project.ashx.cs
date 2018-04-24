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
    /// Project 的摘要说明
    /// </summary>
    public class Project : ApiBase
    {
        IData_ProjectInfoService data_ProjectInfoService = BLLContainer.Resolve<IData_ProjectInfoService>(resolveNew: true);
        IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>(resolveNew: true);
        IData_Project_StoreListService data_Project_StoreListService = BLLContainer.Resolve<IData_Project_StoreListService>(resolveNew: true);
        IData_Project_DeviceService data_Project_DeviceService = BLLContainer.Resolve<IData_Project_DeviceService>(resolveNew: true);

        /// <summary>
        /// 获取适用门店的主体数据
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private IQueryable getSutiableList(string storeId)
        {
            return from a in data_Project_StoreListService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                    join b in data_ProjectInfoService.GetModels() on a.ProjectID equals b.ID
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
                var query = data_ProjectInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
                if (userTokenKeyModel.LogType == (int)RoleType.StoreUser)
                {
                    query = (IQueryable<Data_ProjectInfo>)getSutiableList(storeId);
                }
                
                var linq = from a in query
                           join b in dict_BalanceTypeService.GetModels(p=>p.State == 1) on a.BalanceType equals b.ID into b1
                           from b in b1.DefaultIfEmpty()
                           select new
                           {
                               ID = a.ID,
                               ProjectName = a.ProjectName,
                               PlayCount = a.PlayCount,
                               ExpireDays = a.ExpireDays,
                               AccompanyFlag = a.AccompanyFlag,
                               AccompanyFlagStr = a.AccompanyFlag != null ? (a.AccompanyFlag == 1 ? "是" : a.AccompanyFlag == 0 ? "否" : string.Empty) : string.Empty,
                               AccompanyCash = a.AccompanyCash,
                               BalanceType = a.BalanceType,
                               BalanceTypeStr = b != null ? b.TypeName : string.Empty,
                               BalanceCount = a.BalanceCount,
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

                IData_ProjectInfoService data_ProjectInfoService = BLLContainer.Resolve<IData_ProjectInfoService>();
                var linq = from a in data_ProjectInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
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
                    errMsg = "门票ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);
                var data_ProjectInfo = data_ProjectInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                if (data_ProjectInfo == null)
                {
                    errMsg = "该项目不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_ProjectInfo);
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
                string playCount = dicParas.ContainsKey("playCount") ? (dicParas["playCount"] + "") : string.Empty;
                string expireDays = dicParas.ContainsKey("expireDays") ? (dicParas["expireDays"] + "") : string.Empty;
                string playOnceFlag = dicParas.ContainsKey("playOnceFlag") ? (dicParas["playOnceFlag"] + "") : string.Empty;
                string accompanyFlag = dicParas.ContainsKey("accompanyFlag") ? (dicParas["accompanyFlag"] + "") : string.Empty;
                string accompanyCash = dicParas.ContainsKey("accompanyCash") ? (dicParas["accompanyCash"] + "") : string.Empty;
                string balanceType = dicParas.ContainsKey("balanceType") ? (dicParas["balanceType"] + "") : string.Empty;
                string balanceCount = dicParas.ContainsKey("balanceCount") ? (dicParas["balanceCount"] + "") : string.Empty;                
                string note = dicParas.ContainsKey("note") ? (dicParas["note"] + "") : string.Empty;
                int iId = 0;
                int.TryParse(id, out iId);

                #region 验证参数

                if (string.IsNullOrEmpty(projectName))
                {
                    errMsg = "项目名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(playCount))
                {
                    errMsg = "游玩次数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(playCount) || Convert.ToInt32(playCount) < 0)
                {
                    errMsg = "游玩次数格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(expireDays))
                {
                    errMsg = "有效天数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(expireDays) || Convert.ToInt32(expireDays) < 0)
                {
                    errMsg = "有效天数格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(playOnceFlag))
                {
                    errMsg = "是否扣除次数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(playOnceFlag) || Convert.ToInt32(playOnceFlag) < 0)
                {
                    errMsg = "是否扣除次数格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(accompanyCash) && (!Utils.isNumber(accompanyCash) || Convert.ToInt32(accompanyCash) < 0))
                {
                    errMsg = "陪同票现金格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(balanceCount) && (!Utils.isNumber(balanceCount) || Convert.ToInt32(balanceCount) < 0))
                {
                    errMsg = "扣除数量格式不正确，须为非负整数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                

                #endregion

                IData_ProjectInfoService data_ProjectInfoService = BLLContainer.Resolve<IData_ProjectInfoService>();
                if (data_ProjectInfoService.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) &&
                    a.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase) && a.ID != iId))
                {
                    errMsg = "该项目名称已存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_ProjectInfo = new Data_ProjectInfo();
                data_ProjectInfo.ID = iId;
                data_ProjectInfo.ProjectName = projectName;
                data_ProjectInfo.PlayCount = ObjectExt.Toint(playCount); 
                data_ProjectInfo.ExpireDays = ObjectExt.Toint(expireDays);
                data_ProjectInfo.PlayOnceFlag = ObjectExt.Toint(playOnceFlag);
                data_ProjectInfo.AccompanyFlag = ObjectExt.Toint(accompanyFlag);
                data_ProjectInfo.AccompanyCash = ObjectExt.Toint(accompanyCash);
                data_ProjectInfo.BalanceType = ObjectExt.Toint(balanceType);
                data_ProjectInfo.BalanceCount = ObjectExt.Toint(balanceCount);
                data_ProjectInfo.Note = note;
                data_ProjectInfo.MerchID = merchId;
                if (!data_ProjectInfoService.Any(a => a.ID == iId))
                {
                    //新增
                    if (!data_ProjectInfoService.Add(data_ProjectInfo))
                    {
                        errMsg = "添加门票项目信息失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    //修改
                    if (!data_ProjectInfoService.Update(data_ProjectInfo))
                    {
                        errMsg = "修改门票项目信息失败";
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
                            var data_ProjectInfoModel = data_ProjectInfoService.GetModels(p => p.ID == iProjectId).FirstOrDefault();
                            if (data_ProjectInfoModel == null)
                            {
                                errMsg = "项目ID" + projectId + "不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            data_ProjectInfoService.DeleteModel(data_ProjectInfoModel);

                            foreach (var model in data_Project_StoreListService.GetModels(p=>p.ProjectID == iProjectId))
                            {
                                data_Project_StoreListService.DeleteModel(model);
                            }

                            foreach (var model in data_Project_DeviceService.GetModels(p => p.ProjectID == iProjectId))
                            {
                                data_Project_DeviceService.DeleteModel(model);
                            }
                        }

                        if (!data_ProjectInfoService.SaveChanges())
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
                    errMsg = "门票ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iProjectId = Convert.ToInt32(projectId);
                var storeIDs = data_Project_StoreListService.GetModels(p => p.ProjectID == iProjectId).Select(o => new { StoreID = o.StoreID });
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
                    errMsg = "门票ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        int iProjectId = Convert.ToInt32(projectId);
                        foreach (var model in data_Project_StoreListService.GetModels(p => p.ProjectID == iProjectId))
                        {
                            data_Project_StoreListService.DeleteModel(model);
                        }

                        var storeIdArr = storeIds.Split('|');
                        foreach (var storeId in storeIdArr)
                        {
                            var model = new Data_Project_StoreList();
                            model.ProjectID = iProjectId;
                            model.StoreID = storeId;
                            data_Project_StoreListService.AddModel(model);
                        }

                        if (!data_Project_StoreListService.SaveChanges())
                        {
                            errMsg = "更新门票适用门店表失败";
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