using System;
using System.Collections.Generic;
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

                var linq = from d in
                               (from a in Dict_BalanceTypeService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1)
                                join b in Data_BalanceType_StoreListService.N.GetModels() on a.ID equals b.BalanceIndex into b1
                                from b in b1.DefaultIfEmpty()
                                join c in Base_StoreInfoService.N.GetModels() on b.StroeID equals c.StoreID into c1
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

                var linq = Dict_BalanceTypeService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1)
                    .OrderBy(o => o.TypeID)
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
                int id = dicParas.Get("id").Toint(0);

                if (id == 0)
                {
                    errMsg = "流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Dict_BalanceTypeService.I.Any(p => p.ID == id))
                {
                    errMsg = "该余额类别不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var linq = from t in
                               (from a in Dict_BalanceTypeService.N.GetModels(p => p.ID == id)
                                join b in Data_BalanceType_StoreListService.N.GetModels() on a.ID equals b.BalanceIndex into b1
                                from b in b1.DefaultIfEmpty()
                                join c in Base_StoreInfoService.N.GetModels() on b.StroeID equals c.StoreID into c1
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
                if (!dicParas.Get("typeId").Validint("类别编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("typeName").Nonempty("类别名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("hkType").Validint("关联类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                int id = dicParas.Get("id").Toint(0);
                var typeId = dicParas.Get("typeId").Toint();
                var typeName = dicParas.Get("typeName");
                var hkType = dicParas.Get("hkType").Toint();
                var note = dicParas.Get("note");
                var storeIds = dicParas.Get("storeIds");                
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (Dict_BalanceTypeService.I.Any(p => p.ID != id && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.TypeID == typeId))
                        {
                            errMsg = "同一商户下余额类别编号不能重复";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (Dict_BalanceTypeService.I.Any(p => p.ID != id && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.HKType != (int)HKType.NoBound && p.HKType == hkType))
                        {
                            errMsg = "同一商户下余额类别的关联类别不能重复";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var dict_BalanceType = Dict_BalanceTypeService.I.GetModels(p=>p.ID == id).FirstOrDefault() ?? new Dict_BalanceType();
                        dict_BalanceType.ID = id;
                        dict_BalanceType.TypeID = typeId;
                        dict_BalanceType.TypeName = typeName;
                        dict_BalanceType.Note = note;
                        dict_BalanceType.MerchID = merchId;
                        dict_BalanceType.HKType = hkType;
                        if (id == 0)
                        {
                            //新增
                            dict_BalanceType.State = 1;
                            if (!Dict_BalanceTypeService.I.Add(dict_BalanceType))
                            {
                                errMsg = "添加余额类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!Dict_BalanceTypeService.I.Any(p => p.ID == id))
                            {
                                errMsg = "该余额类别不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //修改
                            if (!Dict_BalanceTypeService.I.Update(dict_BalanceType))
                            {
                                errMsg = "修改余额类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = dict_BalanceType.ID;

                        //先删除，后添加
                        foreach (var model in Data_BalanceType_StoreListService.I.GetModels(p => p.BalanceIndex == id))
                        {
                            Data_BalanceType_StoreListService.I.DeleteModel(model);
                        }

                        if (!string.IsNullOrEmpty(storeIds))
                        {
                            foreach (var storeId in storeIds.Split('|'))
                            {
                                if (!storeId.Nonempty("门店ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                var model = new Data_BalanceType_StoreList();
                                model.BalanceIndex = id;
                                model.StroeID = storeId;
                                Data_BalanceType_StoreListService.I.AddModel(model);
                            }
                        }

                        if (!Data_BalanceType_StoreListService.I.SaveChanges())
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
                int id = dicParas.Get("id").Toint(0);

                if (id == 0)
                {
                    errMsg = "余额类别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Dict_BalanceTypeService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该余额类别不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var dict_BalanceType = Dict_BalanceTypeService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        dict_BalanceType.State = 0;
                        if (!Dict_BalanceTypeService.I.Update(dict_BalanceType))
                        {
                            errMsg = "删除余额类别失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        foreach (var model in Data_BalanceType_StoreListService.I.GetModels(p => p.BalanceIndex == id))
                        {
                            Data_BalanceType_StoreListService.I.DeleteModel(model);
                        }
                        
                        if (!Data_BalanceType_StoreListService.I.SaveChanges())
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