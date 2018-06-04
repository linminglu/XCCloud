using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser, StoreUser")]
    /// <summary>
    /// GoodsInfo 的摘要说明
    /// </summary>
    public class DepotInfo : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDepotDic(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                if (!dicParas.Get("merchId").IsNull())
                    merchId = dicParas.Get("merchId");
                if (!dicParas.Get("storeId").IsNull())
                    storeId = dicParas.Get("storeId");

                IBase_DepotInfoService base_DepotInfoService = BLLContainer.Resolve<IBase_DepotInfoService>();
                IQueryable<Base_DepotInfo> query = null;
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    query = base_DepotInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "") == "");
                }
                else
                {
                    query = base_DepotInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }

                var result = from a in query
                             orderby a.ID
                             select new
                             {
                                 ID = a.ID,
                                 DepotName = a.DepotName
                             };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryDepotInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                
                IQueryable<Base_DepotInfo> query = null;
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    query = Base_DepotInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    query = Base_DepotInfoService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }

                var result = from a in query
                             join b in Base_StoreInfoService.N.GetModels() on a.StoreID equals b.StoreID into b1
                             from b in b1.DefaultIfEmpty()
                             orderby a.ID
                             select new
                             {
                                 ID = a.ID,
                                 DepotName = a.DepotName,
                                 MinusEN = a.MinusEN,
                                 StoreName = b != null ? b.StoreName : "总店"
                             };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDepotInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if(!dicParas.Get("id").Nonempty("仓库编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("depotName").Nonempty("仓库名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("minusEn").Validint("是否运行为负", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                if(!Base_DepotInfoService.I.Any(p=>p.ID == id))
                {
                    errMsg = "该仓库信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var base_DepotInfo = Base_DepotInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, base_DepotInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveDepotInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("depotName").Nonempty("仓库名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("minusEn").Validint("是否允许为负", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg); 

                var id = dicParas.Get("id").Toint(0);
                var depotName = dicParas.Get("depotName");
                var minusEn = dicParas.Get("minusEn").Toint();

                var base_DepotInfo = Base_DepotInfoService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Base_DepotInfo();
                base_DepotInfo.MerchID = merchId;
                base_DepotInfo.StoreID = storeId;
                base_DepotInfo.DepotName = depotName;
                base_DepotInfo.MinusEN = minusEn;
                if (id == 0)
                {                    
                    //新增
                    if (!Base_DepotInfoService.I.Add(base_DepotInfo))
                    {
                        errMsg = "添加仓库信息失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    if (base_DepotInfo.ID == 0)
                    {
                        errMsg = "该仓库信息不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    //修改
                    if (!Base_DepotInfoService.I.Update(base_DepotInfo))
                    {
                        errMsg = "修改仓库信息失败";
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
        public object DelDepotInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Nonempty("仓库编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();
                
                if (!Base_DepotInfoService.I.Any(a => a.ID == id))
                {
                    errMsg = "该仓库信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var base_DepotInfo = Base_DepotInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (!Base_DepotInfoService.I.Update(base_DepotInfo))
                {
                    errMsg = "删除仓库信息失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryBindingInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("depotId").Validint("仓库编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var depotId = dicParas.Get("depotId").Toint(0) ;

                var query = Base_GoodsInfoService.N.GetModels(p => p.AllowStorage == 1 && p.Status == 1); //只显示允许入库的同级商品
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    //商户仅看到总店的库存商品
                    query = query.Where(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "") == "");
                }
                else
                {
                    //门店能看到自己门店和总店的商品
                    query = query.Where(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) ||
                        (p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "") == ""));
                }

                var result = from a in query
                             join b in Data_GoodsStockService.N.GetModels(p => p.DepotID == depotId) on a.ID equals b.GoodID into b1
                             from b in b1.DefaultIfEmpty()
                             join c in Dict_SystemService.N.GetModels() on a.GoodType equals c.ID into c1
                             from c in c1.DefaultIfEmpty()                             
                             orderby a.StoreID, a.ID
                             select new
                             {
                                 ID = b != null ? b.ID : 0,
                                 GoodID = a.ID,
                                 Barcode = a.Barcode,
                                 GoodTypeStr = c != null ? c.DictKey : string.Empty,
                                 GoodName = a.GoodName,
                                 MinValue = b != null ? b.MinValue : (int?)null,
                                 MaxValue = b != null ? b.MaxValue : (int?)null
                             };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveBindingInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("depotId").Validint("仓库编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("bindingInfos").Validarray("绑定信息", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var depotId = dicParas.Get("depotId").Toint();
                var bindingInfos = dicParas.GetArray("bindingInfos");
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (IDictionary<string, object> el in bindingInfos)
                        {
                            if (el != null)
                            {
                                var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                if (!dicPara.Get("goodId").Validint("商品ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                if (!dicPara.Get("minValue").Validint("库存下限", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                if (!dicPara.Get("maxValue").Validint("库存上限", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                
                                var id = dicPara.Get("id").Toint(0);
                                var goodId = dicPara.Get("goodId").Toint();
                                var minValue = dicPara.Get("minValue").Toint();
                                var maxValue = dicPara.Get("maxValue").Toint();

                                var data_GoodsStock = Data_GoodsStockService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GoodsStock();                                
                                data_GoodsStock.DepotID = depotId;                                
                                data_GoodsStock.GoodID = goodId;
                                data_GoodsStock.MerchID = merchId;
                                data_GoodsStock.StoreID = storeId;
                                data_GoodsStock.MinValue = minValue;
                                data_GoodsStock.MaxValue = maxValue;
                                if (id == 0)
                                {                                    
                                    data_GoodsStock.InitialValue = 0;
                                    data_GoodsStock.InitialTime = DateTime.Now;
                                    Data_GoodsStockService.I.AddModel(data_GoodsStock);
                                }
                                else
                                {
                                    if (data_GoodsStock.ID == 0)
                                    {
                                        errMsg = "该库存信息不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    Data_GoodsStockService.I.Update(data_GoodsStock);
                                }
                                
                            }
                            else
                            {
                                errMsg = "提交数据包含空对象";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        if (!Data_GoodsStockService.I.SaveChanges())
                        {
                            errMsg = "保存仓库绑定信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }           
             
                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        errMsg = e.Message;
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