﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Common.Extensions;
using System.Transactions;
using XCCloudService.Model.XCCloud;
using System.Data.Entity.Validation;

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

                var list = from a in Data_GroupAreaService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)
                               && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                           join b in Data_ProjectInfoService.N.GetModels(p => p.State == 1) on a.ID equals b.AreaType into b1
                           from b in b1.DefaultIfEmpty()
                           group a by a.ID into g
                           orderby g.FirstOrDefault().AreaName
                           select new GroupAreaModel
                           {
                               ID = g.Key,
                               PID = 0,
                               AreaName = g.FirstOrDefault().AreaName,
                               ProjectCount = g.Count()
                           };

                //实例化一个根节点
                GroupAreaModel rootRoot = new GroupAreaModel();
                rootRoot.ID = 0;
                rootRoot.AreaName = "全部";
                rootRoot.ProjectCount = list.Sum(s => s.ProjectCount); 
                TreeHelper.LoopToAppendChildren(list.ToList(), rootRoot);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, rootRoot.Children);
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
                        if (Data_GroupAreaService.I.Any(p => p.ID != id && p.AreaName.Equals(areaName, StringComparison.OrdinalIgnoreCase)))
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
                if (!dicParas.Get("id").Validintnozero("区域ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("newId").Validintnozero("新区域ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();
                var newId = dicParas.Get("newId").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GroupAreaService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该区域不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GroupAreaService.I.Any(p => p.ID == newId))
                        {
                            errMsg = "新区域不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        foreach (var model in Data_ProjectInfoService.I.GetModels(p => p.AreaType == id && p.State == 1))
                        {
                            model.AreaType = newId;
                            Data_ProjectInfoService.I.UpdateModel(model);
                        }

                        if (!Data_ProjectInfoService.I.SaveChanges())
                        {
                            errMsg = "批量修改区域失败";
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