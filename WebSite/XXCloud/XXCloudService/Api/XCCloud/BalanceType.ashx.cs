using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var linq = from t in
                               (from a in Dict_BalanceTypeService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1)
                                join d in
                                    (
                                        from d in Dict_SystemService.N.GetModels()
                                        join e in Dict_SystemService.N.GetModels() on d.PID equals e.ID
                                        where e.DictKey == "关联类别"
                                        select d
                                        ) on (a.MappingType + "") equals d.DictValue into d1
                                from d in d1.DefaultIfEmpty()
                                join b in
                                    (
                                        from b in Data_BalanceType_StoreListService.N.GetModels()
                                        join c in Base_StoreInfoService.N.GetModels() on b.StroeID equals c.StoreID
                                        select new { b.BalanceIndex, c.StoreName }
                                        ) on a.ID equals b.BalanceIndex into b1
                                from b in b1.DefaultIfEmpty()
                                select new
                                {
                                    a = a,
                                    HkTypeStr = d != null ? d.DictKey : string.Empty,
                                    StoreName = b != null ? b.StoreName : string.Empty
                                }).AsEnumerable()
                           group t by t.a.ID into g
                           orderby g.Key
                           select new
                           {
                               ID = g.Key,
                               //TypeID = g.FirstOrDefault().a.TypeID,
                               TypeName = g.FirstOrDefault().a.TypeName,
                               Note = g.FirstOrDefault().a.Note,
                               AddingType = g.FirstOrDefault().a.AddingType,
                               AddingTypeStr = ((AddingType?)g.FirstOrDefault().a.AddingType).GetDescription(),
                               DecimalNumber = g.FirstOrDefault().a.DecimalNumber,
                               HkTypeStr = g.FirstOrDefault().HkTypeStr,
                               //StoreNames = string.Join("|", g.OrderBy(o => o.StoreName).Select(o => o.StoreName))
                               StoreNames = g.OrderBy(o => o.StoreName).Select(o => o.StoreName).FirstOrDefault() + (g.Count() > 1 ? "等多个" : string.Empty),
                               Unit = g.FirstOrDefault().a.Unit ?? string.Empty
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                var hkType = dicParas.Get("hkType").Toint();
                if (!dicParas.Get("storeId").IsNull())
                {
                    storeId = dicParas.Get("storeId");
                }

                var query = Dict_BalanceTypeService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1);
                if (hkType != null)
                    query = query.Where(w => w.MappingType == hkType);
                if (!storeId.IsNull())
                    query = from a in query
                            join b in Data_BalanceType_StoreListService.N.GetModels(p => p.StroeID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.ID equals b.BalanceIndex
                            select a;    

                var linq = from a in query
                           orderby a.ID
                           select new
                           {
                               ID = a.ID,
                               TypeName = a.TypeName,
                               Unit = a.Unit ?? string.Empty
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object GetBalanceTypeDicFromProgram(Dictionary<string, object> dicParas)
        {
            try
            {
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string storeId = userTokenModel.StoreId;
                string merchId = storeId.Substring(0, 6);

                string errMsg = string.Empty;
                var hkType = dicParas.Get("hkType").Toint();
                if (!dicParas.Get("storeId").IsNull())
                {
                    storeId = dicParas.Get("storeId");
                }

                var query = Dict_BalanceTypeService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1);
                if (hkType != null)
                    query = query.Where(w => w.MappingType == hkType);
                if (!storeId.IsNull())
                    query = from a in query
                            join b in Data_BalanceType_StoreListService.N.GetModels(p => p.StroeID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.ID equals b.BalanceIndex
                            select a;

                var linq = from a in query
                           orderby a.ID
                           select new
                           {
                               ID = a.ID,
                               TypeName = a.TypeName,
                               Unit = a.Unit ?? string.Empty
                           };

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
                                select new
                                {
                                    a = a,
                                    StoreID = b != null ? b.StroeID : string.Empty
                                }).AsEnumerable()
                           group t by t.a.ID into g
                           select new
                           {
                               ID = g.Key,
                               //TypeID = g.FirstOrDefault().a.TypeID,
                               TypeName = g.FirstOrDefault().a.TypeName,
                               Note = g.FirstOrDefault().a.Note,
                               AddingType = g.FirstOrDefault().a.AddingType,
                               AddingTypeStr = ((AddingType?)g.FirstOrDefault().a.AddingType).GetDescription(),
                               HKType = g.FirstOrDefault().a.MappingType,
                               StoreIDs = string.Join("|", g.Select(o => o.StoreID)),
                               Unit = g.FirstOrDefault().a.Unit ?? string.Empty
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq.FirstOrDefault());                
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                //if (!dicParas.Get("typeId").Validint("类别编号", out errMsg))
                //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("typeName").Nonempty("类别名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("hkType").Validint("关联类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("addingType").Validint("小数位舍弃方式", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("decimalNumber").Validint("小数位", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                int id = dicParas.Get("id").Toint(0);
                //var typeId = dicParas.Get("typeId").Toint();
                var typeName = dicParas.Get("typeName");
                var hkType = dicParas.Get("hkType").Toint();
                var note = dicParas.Get("note");
                var storeIds = dicParas.Get("storeIds");
                var addingType = dicParas.Get("addingType").Toint();
                var decimalNumber = dicParas.Get("decimalNumber").Toint();
                var unit = dicParas.Get("unit");

                if (unit.Length > 10)
                {
                    errMsg = "余额类别的单位不能超过10个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        //if (Dict_BalanceTypeService.I.Any(p => p.ID != id && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.TypeID == typeId))
                        //{
                        //    errMsg = "同一商户下余额类别编号不能重复";
                        //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        //}

                        if (Dict_BalanceTypeService.I.Any(p => p.ID != id && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.MappingType == (int)HKType.Money && p.MappingType == hkType))
                        {
                            errMsg = "储值金类别不能重复";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var dict_BalanceType = Dict_BalanceTypeService.I.GetModels(p=>p.ID == id).FirstOrDefault() ?? new Dict_BalanceType();
                        dict_BalanceType.ID = id;
                        //dict_BalanceType.TypeID = typeId;
                        dict_BalanceType.TypeName = typeName;
                        dict_BalanceType.AddingType = addingType;
                        dict_BalanceType.DecimalNumber = decimalNumber;
                        dict_BalanceType.Note = note;
                        dict_BalanceType.MerchID = merchId;
                        dict_BalanceType.MappingType = hkType;
                        dict_BalanceType.Unit = unit;
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
                string errMsg = string.Empty;
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("余额类别ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if(!id.Validintnozero("余额类别ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            if (!Dict_BalanceTypeService.I.Any(p => p.ID == (int)id))
                            {
                                errMsg = "该余额类别不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var dict_BalanceType = Dict_BalanceTypeService.I.GetModels(p => p.ID == (int)id).FirstOrDefault();
                            dict_BalanceType.State = 0;
                            if (!Dict_BalanceTypeService.I.Update(dict_BalanceType))
                            {
                                errMsg = "删除余额类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            foreach (var model in Data_BalanceType_StoreListService.I.GetModels(p => p.BalanceIndex == (int)id))
                            {
                                Data_BalanceType_StoreListService.I.DeleteModel(model);
                            }

                            if (!Data_BalanceType_StoreListService.I.SaveChanges())
                            {
                                errMsg = "删除余额类别适用门店信息失败";
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
    }
}