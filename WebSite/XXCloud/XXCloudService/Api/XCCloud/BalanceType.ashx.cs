using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
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
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// BalanceType 的摘要说明
    /// </summary>
    public class BalanceType : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetBalanceTypeList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                string errMsg = string.Empty;

                IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>(resolveNew:true);
                IData_BalanceType_StoreListService data_BalanceType_StoreListService = BLLContainer.Resolve<IData_BalanceType_StoreListService>(resolveNew: true);
                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew:true);
                var linq = from d in
                               (from a in dict_BalanceTypeService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1)
                                join b in data_BalanceType_StoreListService.GetModels() on a.ID equals b.BalanceIndex into b1
                                from b in b1.DefaultIfEmpty()
                                join c in base_StoreInfoService.GetModels() on b.StroeID equals c.StoreID into c1
                                from c in c1.DefaultIfEmpty()                                
                                select new
                                {
                                    a = a,
                                    StoreName = c != null ? c.StoreName : string.Empty
                                }).AsEnumerable()
                           group d by d.a.ID into g                           
                           select new
                           {
                               ID = g.Key,
                               TypeID = g.FirstOrDefault().a.TypeID,
                               TypeName = g.FirstOrDefault().a.TypeName,
                               Note = g.FirstOrDefault().a.Note,
                               StoreNames = string.Join("|", g.OrderBy(o => o.StoreName).Select(o => o.StoreName))
                           };
                           
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetBalanceTypeDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                string errMsg = string.Empty;

                IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>();
                var linq = dict_BalanceTypeService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1).OrderBy(o => o.TypeID)
                    .Select(o => new 
                    {
                        ID = o.ID,
                        TypeName = o.TypeName
                    });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetBalanceTypeInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;

                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);
                IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>(resolveNew: true);
                var dict_BalanceTypeModel = dict_BalanceTypeService.GetModels(p => p.ID == iId).FirstOrDefault();
                if (dict_BalanceTypeModel == null)
                {
                    errMsg = "该余额类别不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
    
                IData_BalanceType_StoreListService data_BalanceType_StoreListService = BLLContainer.Resolve<IData_BalanceType_StoreListService>(resolveNew: true);
                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew: true);
                var linq = from t in
                               (from a in dict_BalanceTypeService.GetModels(p => p.ID == iId)
                                join b in data_BalanceType_StoreListService.GetModels() on a.ID equals b.BalanceIndex into b1
                                from b in b1.DefaultIfEmpty()
                                join c in base_StoreInfoService.GetModels() on b.StroeID equals c.StoreID into c1
                                from c in c1.DefaultIfEmpty()                                
                                select new
                                {
                                    a = a,
                                    StoreID = c != null ? c.StoreID : string.Empty
                                }).AsEnumerable()
                           group t by t.a.ID into g
                           select new
                           {
                               ID = g.Key,
                               TypeID = g.FirstOrDefault().a.TypeID,
                               TypeName = g.FirstOrDefault().a.TypeName,
                               Note = g.FirstOrDefault().a.Note,
                               HKType = g.FirstOrDefault().a.HKType,
                               StoreIDs = string.Join("|", g.Select(o => o.StoreID))
                           };


                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);                
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveBalanceTypeInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                string typeId = dicParas.ContainsKey("typeId") ? (dicParas["typeId"] + "") : string.Empty;
                string typeName = dicParas.ContainsKey("typeName") ? (dicParas["typeName"] + "") : string.Empty;
                string hkType = dicParas.ContainsKey("hkType") ? (dicParas["hkType"] + "") : string.Empty;
                string note = dicParas.ContainsKey("note") ? (dicParas["note"] + "") : string.Empty;
                string storeIds = dicParas.ContainsKey("storeIds") ? (dicParas["storeIds"] + "") : string.Empty;                
                int iId = 0;
                int.TryParse(id, out iId);

                #region 验证参数

                if (string.IsNullOrEmpty(typeId))
                {
                    errMsg = "类别编号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(typeId))
                {
                    errMsg = "类别编号格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(typeName))
                {
                    errMsg = "类别名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(hkType))
                {
                    errMsg = "关联类别不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                        
                #endregion

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var iTypeId = typeId.Toint();
                        IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>();
                        if (dict_BalanceTypeService.Any(p => p.ID != iId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.TypeID == iTypeId))
                        {
                            errMsg = "同商户余额类别编号不能重复";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var iHKType = hkType.Toint();
                        if (dict_BalanceTypeService.Any(p => p.ID != iId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.HKType == iHKType && p.HKType != (int)HKType.NoBound))
                        {
                            errMsg = "同商户余额类别的关联类别不能重复";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var dict_BalanceType = dict_BalanceTypeService.GetModels(p=>p.ID == iId).FirstOrDefault() ?? new Dict_BalanceType();
                        dict_BalanceType.ID = iId;
                        dict_BalanceType.TypeID = iTypeId;
                        dict_BalanceType.TypeName = typeName;
                        dict_BalanceType.Note = note;
                        dict_BalanceType.MerchID = merchId;
                        dict_BalanceType.HKType = iHKType;
                        if (dict_BalanceType.ID == 0)
                        {
                            //新增
                            dict_BalanceType.State = 1;
                            if (!dict_BalanceTypeService.Add(dict_BalanceType))
                            {
                                errMsg = "添加余额类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            //修改
                            if (!dict_BalanceTypeService.Update(dict_BalanceType))
                            {
                                errMsg = "修改余额类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iId = dict_BalanceType.ID;

                        //先删除已有数据，后添加
                        IData_BalanceType_StoreListService data_BalanceType_StoreListService = BLLContainer.Resolve<IData_BalanceType_StoreListService>();
                        foreach (var model in data_BalanceType_StoreListService.GetModels(p => p.BalanceIndex == iId))
                        {
                            data_BalanceType_StoreListService.DeleteModel(model);
                        }

                        if (!string.IsNullOrEmpty(storeIds))
                        {
                            foreach (var storeId in storeIds.Split('|'))
                            {
                                if (!storeId.Nonempty("门店ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                var model = new Data_BalanceType_StoreList();
                                model.BalanceIndex = iId;
                                model.StroeID = storeId;
                                data_BalanceType_StoreListService.AddModel(model);
                            }
                        }
                        
                        if (!data_BalanceType_StoreListService.SaveChanges())
                        {
                            errMsg = "更新余额类别适用门店信息失败";
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
        public object DelBalanceTypeInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;                
                int iId = 0;
                int.TryParse(id, out iId);

                #region 验证参数

                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "余额类别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                

                #endregion
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>();
                        if (!dict_BalanceTypeService.Any(p => p.ID == iId))
                        {
                            errMsg = "该余额类别不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var dict_BalanceType = dict_BalanceTypeService.GetModels(p => p.ID == iId).FirstOrDefault();
                        dict_BalanceType.State = 0;
                        if (!dict_BalanceTypeService.Update(dict_BalanceType))
                        {
                            errMsg = "删除余额类别失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        IData_BalanceType_StoreListService data_BalanceType_StoreListService = BLLContainer.Resolve<IData_BalanceType_StoreListService>();
                        foreach (var model in data_BalanceType_StoreListService.GetModels(p => p.BalanceIndex == iId))
                        {
                            data_BalanceType_StoreListService.DeleteModel(model);
                        }
                        
                        if (!data_BalanceType_StoreListService.SaveChanges())
                        {
                            errMsg = "删除余额类别适用门店信息失败";
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