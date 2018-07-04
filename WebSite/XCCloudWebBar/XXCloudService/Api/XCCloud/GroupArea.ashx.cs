using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XXCloudService.Api.XCCloud.Common;
using XCCloudWebBar.Common.Extensions;
using System.Transactions;
using XCCloudWebBar.Model.XCCloud;
using System.Data.Entity.Validation;
using XCCloudWebBar.CacheService;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// GroupArea 的摘要说明
    /// </summary>
    public class GroupArea : ApiBase
    {
        /// <summary>
        /// 查询区域列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGroupArea(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var linq = from a in
                               (from a in Data_GroupAreaService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                                join b in Data_ProjectInfoService.N.GetModels(p => p.State == 1) on a.ID equals b.AreaType into b1
                                from b in b1.DefaultIfEmpty()
                                select new { a.ID, a.AreaName, ProjectID = b != null ? b.ID : (int?)null })
                           group a by a.ID into g
                           orderby g.FirstOrDefault().AreaName
                           select new GroupAreaModel
                           {
                               ID = g.Key,
                               AreaName = g.FirstOrDefault().AreaName,
                               ProjectCount = g.Count(c => c.ProjectID != null)
                           };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, linq.ToList());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 查询区域列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object QueryGroupAreaFromProgram(Dictionary<string, object> dicParas)
        {
            try
            {
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string storeId = userTokenModel.StoreId;
                string merchId = storeId.Substring(0, 6);

                var linq = from a in
                               (from a in Data_GroupAreaService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                                join b in Data_ProjectInfoService.N.GetModels(p => p.State == 1) on a.ID equals b.AreaType into b1
                                from b in b1.DefaultIfEmpty()
                                select new { a.ID, a.AreaName, ProjectID = b != null ? b.ID : (int?)null })
                           group a by a.ID into g
                           orderby g.FirstOrDefault().AreaName
                           select new GroupAreaModel
                           {
                               ID = g.Key,
                               AreaName = g.FirstOrDefault().AreaName,
                               ProjectCount = g.Count(c => c.ProjectID != null)
                           };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, linq.ToList());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取区域信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGroupArea(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var errMsg = string.Empty;
                if(!dicParas.Get("id").Validintnozero("区域ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var list = from a in Data_GroupAreaService.N.GetModels(p => p.ID == id)
                           join b in Data_ProjectInfoService.N.GetModels(p => p.State == 1) on a.ID equals b.AreaType
                           join c in Dict_SystemService.N.GetModels() on b.ProjectType equals c.ID
                           join d in Dict_SystemService.N.GetModels() on c.PID equals d.ID
                           select new
                           {
                               ID = b.ID,
                               ProjectName = b.ProjectName,
                               ProjectType = b.ProjectType,
                               ProjectTypeName = c.DictKey,
                               ProjectTypeParent = d.DictKey,
                               AreaType = b.AreaType,
                               AreaName = a.AreaName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGroupArea(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("areaName").Nonempty("区域名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                int id = dicParas.Get("id").Toint(0);
                var areaName = dicParas.Get("areaName");
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (Data_GroupAreaService.I.Any(p => p.ID != id && p.AreaName.Equals(areaName, StringComparison.OrdinalIgnoreCase) && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "区域名称不能重复";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GroupAreaService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GroupArea();
                        model.ID = id;
                        model.AreaName = areaName;
                        model.MerchID = merchId;
                        model.StoreID = storeId;
                        if (id == 0)
                        {
                            //新增
                            if (!Data_GroupAreaService.I.Add(model))
                            {
                                errMsg = "添加区域设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!Data_GroupAreaService.I.Any(p => p.ID == id))
                            {
                                errMsg = "该区域设置不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //修改
                            if (!Data_GroupAreaService.I.Update(model))
                            {
                                errMsg = "修改区域设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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
        public object DelGroupArea(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if(!dicParas.Get("id").Validintnozero("区域ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GroupAreaService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该区域设置不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        foreach (var model in Data_ProjectInfoService.I.GetModels(p => p.AreaType == id && p.State == 1))
                        {
                            model.AreaType = (int?)null;
                            Data_ProjectInfoService.I.UpdateModel(model);
                        }

                        var areaModel = Data_GroupAreaService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        Data_GroupAreaService.I.DeleteModel(areaModel);

                        if (!Data_GroupAreaService.I.SaveChanges())
                        {
                            errMsg = "删除区域设置失败";
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

        /// <summary>
        /// 批量修改区域
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object ChangeGroupAreaBatch(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.GetArray("projectAreaList").Validarray("项目区域列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var projectAreaList = dicParas.GetArray("projectAreaList");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (projectAreaList != null && projectAreaList.Count() > 0)
                        {
                            foreach (IDictionary<string, object> el in projectAreaList)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("projectId").Validintnozero("游乐项目ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("areaType").Validintnozero("区域ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var projectId = dicPara.Get("projectId").Toint();
                                    var areaType = dicPara.Get("areaType").Toint();
                                    if (!Data_ProjectInfoService.I.Any(p => p.ID == projectId))
                                    {
                                        errMsg = "该游乐项目不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    if (!Data_GroupAreaService.I.Any(p => p.ID == areaType))
                                    {
                                        errMsg = "该区域不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var projectInfo = Data_ProjectInfoService.I.GetModels(p => p.ID == projectId).FirstOrDefault();
                                    projectInfo.AreaType = areaType;
                                    Data_ProjectInfoService.I.UpdateModel(projectInfo);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_ProjectInfoService.I.SaveChanges())
                            {
                                errMsg = "批量修改区域失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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