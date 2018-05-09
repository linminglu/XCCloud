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
    public class GoodsInfo : ApiBase
    {
        IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
        IData_GoodInventoryService data_GoodInventoryService = BLLContainer.Resolve<IData_GoodInventoryService>(resolveNew: true);
        IBase_DepotInfoService base_DepotInfoService = BLLContainer.Resolve<IBase_DepotInfoService>(resolveNew: true);
        IData_GoodsStockService data_GoodsStockService = BLLContainer.Resolve<IData_GoodsStockService>(resolveNew: true);
        IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew: true);
        IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>(resolveNew: true);

        #region 商品档案维护
        /// <summary>
        /// 商品查询列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodsInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);


                #region Sql语句
                string sql = @"SELECT
                                	a.ID,
                                	/*商品条码*/
                                	a.Barcode,
                                	/*商品名称*/
                                	a.GoodName,
                                	/*入库状态*/
                                	a.AllowStorage,
                                	/*备注*/
                                	a.Note,
                                	/*商品类别*/
                                	a.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	c.DictKey AS GoodTypeStr,
                                	/*门店ID*/
                                	a.StoreID,
                                	/*门店名称*/
                                	b.StoreName,
                                	/*总店ID*/
                                	a.MerchID,
                                	/*总店名称*/
                                	m.MerchName,
                                    a.Price,
                                    a.AllowCreatePoint,
                                    a.Note
                                FROM
                                	Base_GoodsInfo a
                                LEFT JOIN Base_StoreInfo b ON a.StoreID = b.StoreID
                                LEFT JOIN Base_MerchantInfo m ON a.MerchID = m.MerchID
                                LEFT JOIN (
                                	SELECT
                                		b.*
                                	FROM
                                		Dict_System a
                                	INNER JOIN Dict_System b ON a.ID = b.PID
                                	WHERE
                                		a.DictKey = '商品类别'
                                	AND a.PID = 0
                                ) c ON CONVERT (VARCHAR, a.GoodType) = c.DictValue
                                WHERE
                                	a.Status = 1";
                sql += " AND a.MerchID='" + merchId + "'";
                #endregion

                var list = Base_GoodsInfoService.I.SqlQuery<Base_GoodsInfoList>(sql, parameters).ToList();
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取商品字典
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodsDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var linq = from a in Base_GoodsInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.Status == 1)
                           select new
                           {
                               ID = a.ID,
                               GoodName = a.GoodName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
        /// <summary>
        /// 单个商品信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodsInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Nonempty("商品ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id");

                if (!Base_GoodsInfoService.I.Any(a => a.ID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var goodInfoPrice = from a in Base_Goodinfo_PriceService.N.GetModels(p => p.GoodID.Equals(id, StringComparison.OrdinalIgnoreCase))
                                    join b in Dict_BalanceTypeService.N.GetModels() on a.BalanceIndex equals b.ID
                                    select new 
                                    {
                                        OperateTypei = a.OperateTypei,
                                        BalanceIndex = a.BalanceIndex,
                                        BalanceIndexStr = b.TypeName,
                                        Count = a.Count
                                    };

                var linq = new
                {
                    base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(),
                    goodInfoPrice = goodInfoPrice
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);

            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取商品条码：门店ID+5位随机数，商户ID向右补0
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodsBarCode(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                var barcode = "";
                do
                {
                    string num5 = Utils.getNumRandomCode(5);
                    barcode = (userTokenKeyModel.LogType == (int)RoleType.MerchUser) ? merchId : storeId;
                    barcode = barcode.Toint(0).ToString().PadRight(15, '0') + num5;
                }
                while (Base_GoodsInfoService.I.Any(a => a.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase)));
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, new { BarCode = barcode });

            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 新增/修改商品信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGoodsInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var errMsg = string.Empty;
                if (!dicParas.Get("goodType").Validint("商品类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("barCode").Nonempty("商品条码", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id");
                var barCode = dicParas.Get("barCode");
                var goodInfoPrice = dicParas.GetArray("goodInfoPrice");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (Base_GoodsInfoService.I.Any(a => !a.ID.Equals(id, StringComparison.OrdinalIgnoreCase) && a.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品条码已存在");
                        }

                        var base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? new Base_GoodsInfo();
                        Utils.GetModel(dicParas, ref base_GoodsInfo);
                        if (id.IsNull())
                        {
                            //总店添加的商品是默认入库的？
                            base_GoodsInfo.AllowStorage = userTokenKeyModel.LogType == (int)RoleType.MerchUser ? 1 : 0;
                            base_GoodsInfo.MerchID = merchId;
                            base_GoodsInfo.StoreID = storeId;
                            if (!Base_GoodsInfoService.I.Add(base_GoodsInfo))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "添加商品信息失败");
                        }
                        else
                        {
                            if (base_GoodsInfo.ID.IsNull())
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");
                            if (!storeId.IsNull() && base_GoodsInfo.StoreID != storeId)
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "禁止跨门店修改商品信息");
                            if (base_GoodsInfo.MerchID != merchId)
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "禁止修改非此商户下的商品信息");
                            if (!Base_GoodsInfoService.I.Update(base_GoodsInfo))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "修改商品信息失败");
                        }

                        id = base_GoodsInfo.ID;

                        //添加兑换/回购单品属性
                        if (goodInfoPrice != null && goodInfoPrice.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Base_Goodinfo_PriceService.I.GetModels(p => p.GoodID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                            {
                                Base_Goodinfo_PriceService.I.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in goodInfoPrice)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("operateTypei").Validint("操作类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("balanceIndex").Validint("余额类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("count").Validdecimal("数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    //回购价大于兑换价需要系统提示，operateTypei=1回购 0兑换
                                    var operateTypei = dicPara.Get("operateTypei").Toint();
                                    var balanceIndex = dicPara.Get("balanceIndex").Toint();
                                    var count = dicPara.Get("count").Todecimal();
                                    if ((operateTypei == 1 && Base_Goodinfo_PriceService.I.Any(a => a.OperateTypei == 0 && a.BalanceIndex == balanceIndex && a.Count < count)) ||
                                        (operateTypei == 0 && Base_Goodinfo_PriceService.I.Any(a => a.OperateTypei == 1 && a.BalanceIndex == balanceIndex && a.Count > count)))
                                    {
                                        errMsg = "同一余额类别，回购价不能大于兑换价";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var base_Goodinfo_Price = new Base_Goodinfo_Price();
                                    Utils.GetModel(dicPara, ref base_Goodinfo_Price);
                                    base_Goodinfo_Price.GoodID = id;
                                    Base_Goodinfo_PriceService.I.AddModel(base_Goodinfo_Price);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Base_Goodinfo_PriceService.I.SaveChanges())
                            {
                                errMsg = "保存兑换/回购单品属性失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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
            catch (DbEntityValidationException e)
            {
                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 允许商品入库
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AllowGoodsStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var errMsg = string.Empty;
                if (!dicParas.Get("id").Nonempty("商品ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id");

                if (!Base_GoodsInfoService.I.Any(a => a.ID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                base_GoodsInfo.AllowStorage = 1;
                if (!Base_GoodsInfoService.I.Update(base_GoodsInfo))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "修改商品信息失败");

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (DbEntityValidationException e)
            {
                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
        /// <summary>
        /// 删除商品信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelGoodsInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                var userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var errMsg = string.Empty;
                if (!dicParas.Get("id").Nonempty("商品ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id");

                if (!Base_GoodsInfoService.I.Any(a => a.ID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (!storeId.IsNull() && base_GoodsInfo.StoreID != storeId)
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "无法删除其他门店的商品信息");
                if (base_GoodsInfo.MerchID != merchId)
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "无法删除非此商户下的商品信息");

                base_GoodsInfo.Status = 0;

                if (!Base_GoodsInfoService.I.Update(base_GoodsInfo))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "商品信息删除失败");

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #endregion

        #region 商品入库管理

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string sql = @"select a.ID, d.Barcode, e.DictKey as GoodTypeStr, d.GoodName, g.StoreName, a.RealTime, a.Supplier, c.StorageCount, c.Price, c.TotalPrice, b.LogName, f.DepotName from Data_GoodStorage a " +
                    " left join Base_UserInfo b on a.UserID=b.UserID " +
                    " left join Data_GoodStorage_Detail c on a.ID=c.StorageID " +
                    " left join Base_GoodsInfo d on c.GoodID=d.ID " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='商品类别' and a.PID=0) e on convert(varchar, d.GoodType)=e.DictValue " +
                    " left join Base_DepotInfo f on a.DepotID=f.ID " +
                    " left join Base_StoreInfo g on a.StoreID=g.StoreID " +
                    " where 1=1 ";
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    sql = sql + " and a.storeId='" + storeId + "'";
                }
                else
                {
                    sql = sql + " and a.merchId='" + merchId + "'";
                }
                sql = sql + sqlWhere;
                IData_GoodStorageService data_GoodStorageService = BLLContainer.Resolve<IData_GoodStorageService>();
                var data_GoodStorage = data_GoodStorageService.SqlQuery<Data_GoodStorageList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GoodStorage);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        //public object GetGoodStorage(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        string errMsg = string.Empty;
        //        string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            errMsg = "入库流水号不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        int iId = Convert.ToInt32(id);
        //        IData_GoodStorageService data_GoodStorageService = BLLContainer.Resolve<IData_GoodStorageService>(resolveNew: true);
        //        if (!data_GoodStorageService.Any(a => a.ID == iId))
        //        {
        //            errMsg = "该入库信息不存在";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }
                
        //        IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew:true);                
        //        IData_GoodStorage_DetailService data_GoodStorage_DetailService = BLLContainer.Resolve<IData_GoodStorage_DetailService>(resolveNew:true);
        //        IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>(resolveNew:true);
        //        IData_GoodsStockService data_GoodsStockService = BLLContainer.Resolve<IData_GoodsStockService>(resolveNew:true);
        //        IBase_DepotInfoService base_DepotInfoService = BLLContainer.Resolve<IBase_DepotInfoService>(resolveNew:true);

        //        int goodTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("商品类别") && p.PID == 0).FirstOrDefault().ID;
        //        var GoodStorageDetails = from a in data_GoodStorage_DetailService.GetModels(p => p.StorageID == iId)
        //                                 join b in base_GoodsInfoService.GetModels() on a.GoodID equals b.ID
        //                                 join c in dict_SystemService.GetModels() on (b.GoodType + "") equals c.DictValue into c1
        //                                 from c in c1.DefaultIfEmpty()
        //                                 join d in data_GoodStorageService.GetModels() on a.StorageID equals d.ID
        //                                 join e in data_GoodsStockService.GetModels() on new { a.GoodID, d.DepotID } equals new { e.GoodID, e.DepotID }
        //                                 select new
        //                                 {
        //                                     Barcode = b.Barcode,
        //                                     GoodName = b.GoodName,
        //                                     GoodTypeStr = c != null ? c.DictKey : string.Empty,
        //                                     RemainCount = e.RemainCount,
        //                                     Price = a.Price,
        //                                     StorageCount = a.StorageCount
        //                                 };

        //        var result = (from a in data_GoodStorageService.GetModels(p => p.ID == iId)
        //                      //join b in base_DepotInfoService.GetModels() on a.DepotID equals b.ID
        //                      select new
        //                      {
        //                          DepotID = a.DepotID,
        //                          //DepotName = b.DepotName,
        //                          Supplier = a.Supplier,
        //                          Note = a.Note,
        //                          GoodStorageDetails = GoodStorageDetails
        //                      }).FirstOrDefault();
                            
        //        return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
        //    }
        //    catch (Exception e)
        //    {
        //        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
        //    }
        //}

        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        //public object AddGoodStorage(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
        //        string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
        //        string logId = userTokenKeyModel.LogId;

        //        string errMsg = string.Empty;
        //        string depotId = dicParas.ContainsKey("depotId") ? (dicParas["depotId"] + "") : string.Empty;
        //        string supplier = dicParas.ContainsKey("supplier") ? (dicParas["supplier"] + "") : string.Empty;
        //        string payable = dicParas.ContainsKey("payable") ? (dicParas["payable"] + "") : string.Empty;
        //        string payment = dicParas.ContainsKey("payment") ? (dicParas["payment"] + "") : string.Empty;
        //        string discount = dicParas.ContainsKey("discount") ? (dicParas["discount"] + "") : string.Empty;
        //        string note = dicParas.ContainsKey("note") ? (dicParas["note"] + "") : string.Empty;

        //        #region 参数验证

        //        if (string.IsNullOrEmpty(barCode))
        //        {
        //            errMsg = "商品条码barCode不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (string.IsNullOrEmpty(price))
        //        {
        //            errMsg = "入库单价price不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (string.IsNullOrEmpty(storageCount))
        //        {
        //            errMsg = "入库数量storageCount不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (string.IsNullOrEmpty(totalPrice))
        //        {
        //            errMsg = "入库总额totalPrice不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (!Utils.isNumber(storageCount))
        //        {
        //            errMsg = "入库数量storageCount格式不正确";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (!Utils.IsDecimal(price))
        //        {
        //            errMsg = "入库单价price格式不正确";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (!Utils.IsDecimal(totalPrice))
        //        {
        //            errMsg = "入库总额totalPrice格式不正确";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (!string.IsNullOrEmpty(discount) && !Utils.IsDecimal(discount))
        //        {
        //            errMsg = "优惠金额discount格式不正确";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        #endregion

        //        IData_GoodStorageService data_GoodStorageService = BLLContainer.Resolve<IData_GoodStorageService>();
        //        var data_GoodStorage = new Data_GoodStorage();
        //        Utils.GetModel(dicParas, ref data_GoodStorage);
        //        data_GoodStorage.StoreID = storeId;
        //        data_GoodStorage.RealTime = DateTime.Now;
        //        data_GoodStorage.UserID = Convert.ToInt32(logId);
        //        if (!data_GoodStorageService.Add(data_GoodStorage))
        //        {
        //            errMsg = "添加商品入库信息失败";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
        //    }
        //    catch (Exception e)
        //    {
        //        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
        //    }
        //}

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CheckGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string logId = userTokenKeyModel.LogId;

                string errMsg = string.Empty;
                string barCode = dicParas.ContainsKey("barCode") ? (dicParas["barCode"] + "") : string.Empty;
                string price = dicParas.ContainsKey("price") ? (dicParas["price"] + "") : string.Empty;
                string storageCount = dicParas.ContainsKey("storageCount") ? (dicParas["storageCount"] + "") : string.Empty;
                string totalPrice = dicParas.ContainsKey("totalPrice") ? (dicParas["totalPrice"] + "") : string.Empty;
                string discount = dicParas.ContainsKey("discount") ? (dicParas["discount"] + "") : string.Empty;
                string note = dicParas.ContainsKey("note") ? (dicParas["note"] + "") : string.Empty;

                #region 参数验证

                if (string.IsNullOrEmpty(barCode))
                {
                    errMsg = "商品条码barCode不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(price))
                {
                    errMsg = "入库单价price不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(storageCount))
                {
                    errMsg = "入库数量storageCount不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(totalPrice))
                {
                    errMsg = "入库总额totalPrice不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(storageCount))
                {
                    errMsg = "入库数量storageCount格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.IsDecimal(price))
                {
                    errMsg = "入库单价price格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.IsDecimal(totalPrice))
                {
                    errMsg = "入库总额totalPrice格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(discount) && !Utils.IsDecimal(discount))
                {
                    errMsg = "优惠金额discount格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #endregion

                IData_GoodStorageService data_GoodStorageService = BLLContainer.Resolve<IData_GoodStorageService>();
                var data_GoodStorage = new Data_GoodStorage();
                Utils.GetModel(dicParas, ref data_GoodStorage);
                data_GoodStorage.StoreID = storeId;
                data_GoodStorage.RealTime = DateTime.Now;
                data_GoodStorage.UserID = Convert.ToInt32(logId);
                if (!data_GoodStorageService.Add(data_GoodStorage))
                {
                    errMsg = "添加商品入库信息失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (DbEntityValidationException e)
            {
                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        

        #endregion        

        #region 商品库存盘点

        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        //public object QueryGoodInventory(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
        //        string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
        //        string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

        //        string errMsg = string.Empty;                
        //        int GoodTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("商品类别") && p.PID == 0).FirstOrDefault().ID;
        //        var linq = from a in base_DepotInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.MinusEN == 0)
        //                   join b in
        //                       (from t in data_GoodsStockService.GetModels()
        //                        group t by new { DepotID = t.DepotID, GoodID = t.GoodID } into g
        //                        let topone =
        //                        (
        //                          from item in g
        //                          orderby item.InitialTime descending
        //                          select new { item.DepotID, item.GoodID, item.InitialValue, item.InitialTime, item.InitialAvgValue, item.RemainCount }
        //                        ).First()
        //                        select topone
        //                        ) on a.ID equals b.DepotID
        //                   join c in base_GoodsInfoService.GetModels() on b.GoodID equals c.ID
        //                   join d in dict_SystemService.GetModels(p => p.PID == GoodTypeId) on (c.GoodType + "") equals d.DictValue into d1
        //                   from d in d1.DefaultIfEmpty()
        //                   join e in data_GoodInventoryService.GetModels(p => p.InventoryType == 0) on a.ID equals e.InventoryIndex into e1
        //                   from e in e1.DefaultIfEmpty()
        //                   join f in base_StoreInfoService.GetModels() on e.StoreID equals f.StoreID into f1
        //                   from f in f1.DefaultIfEmpty()
        //                   select new
        //                   {
        //                       ID = e != null ? e.ID : (int?)null,  //礼品盘点ID
        //                       Barcode = c.Barcode,                 //商品条码
        //                       GoodTypeStr = d != null ? d.DictKey : string.Empty,//商品类别
        //                       GoodName = c.GoodName,               //商品名称
        //                       StoreName = f != null ? f.StoreName : string.Empty,//门店名称
        //                       DepotName = a.DepotName,             //仓库名称
        //                       InitialTime = b.InitialTime,         //期初时间
        //                       InitialValue = b.InitialValue,       //期初值
        //                       RemainCount = b.RemainCount,           //应有库存
        //                       RemainValue = Math.Round((b.InitialAvgValue ?? 0M) * (decimal)b.RemainCount, 2, MidpointRounding.AwayFromZero),       //应有库存金额
        //                       InventoryCount = e != null ? e.InventoryCount : (int?)null //实点数
        //                   };

        //        return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
        //    }
        //    catch (Exception e)
        //    {
        //        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
        //    }
        //}

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodInventory(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "盘点流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);
                IData_GoodInventoryService data_GoodInventoryService = BLLContainer.Resolve<IData_GoodInventoryService>();
                if (!data_GoodInventoryService.Any(a => a.ID == iId))
                {
                    errMsg = "该盘点信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var result = from a in data_GoodInventoryService.GetModels(p => p.ID == iId).FirstOrDefault().AsDictionary()
                             select new
                             {
                                 name = a.Key,
                                 value = a.Value
                             };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddGoodInventory(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string logId = userTokenKeyModel.LogId;

                string errMsg = string.Empty;
                string barCode = dicParas.ContainsKey("barCode") ? (dicParas["barCode"] + "") : string.Empty;
                string inventoryType = dicParas.ContainsKey("inventoryType") ? (dicParas["inventoryType"] + "") : string.Empty;
                string inventoryIndex = dicParas.ContainsKey("inventoryIndex") ? (dicParas["inventoryIndex"] + "") : string.Empty;
                string predictCount = dicParas.ContainsKey("predictCount") ? (dicParas["predictCount"] + "") : string.Empty;
                string inventoryCount = dicParas.ContainsKey("inventoryCount") ? (dicParas["inventoryCount"] + "") : string.Empty;
                string totalPrice = dicParas.ContainsKey("totalPrice") ? (dicParas["totalPrice"] + "") : string.Empty;
                string note = dicParas.ContainsKey("note") ? (dicParas["note"] + "") : string.Empty;

                #region 参数验证

                if (string.IsNullOrEmpty(barCode))
                {
                    errMsg = "商品条码barCode不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(inventoryType))
                {
                    errMsg = "盘点类别inventoryType不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(inventoryIndex) && !Utils.isNumber(inventoryIndex))
                {
                    errMsg = "盘点位置索引inventoryIndex格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(predictCount))
                {
                    errMsg = "预估数量predictCount不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(inventoryCount))
                {
                    errMsg = "盘点数量inventoryCount不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(totalPrice))
                {
                    errMsg = "库存金额totalPrice不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(predictCount))
                {
                    errMsg = "预估数量predictCount格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(inventoryCount))
                {
                    errMsg = "盘点数量inventoryCount格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.IsDecimal(totalPrice))
                {
                    errMsg = "库存金额totalPrice格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #endregion

                IData_GoodInventoryService data_GoodInventoryService = BLLContainer.Resolve<IData_GoodInventoryService>();
                var data_GoodInventory = new Data_GoodInventory();
                Utils.GetModel(dicParas, ref data_GoodInventory);
                data_GoodInventory.StoreID = storeId;
                data_GoodInventory.InventoryTime = DateTime.Now;
                data_GoodInventory.UserID = Convert.ToInt32(logId);
                if (!data_GoodInventoryService.Add(data_GoodInventory))
                {
                    errMsg = "添加商品盘点信息失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (DbEntityValidationException e)
            {
                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #endregion
    }
}