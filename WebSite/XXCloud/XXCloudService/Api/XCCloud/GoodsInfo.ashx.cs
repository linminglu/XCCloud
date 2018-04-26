using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
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
                                	m.MerchName
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

                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>();
                var list = base_GoodsInfoService.SqlQuery<Base_GoodsInfoList>(sql, parameters).ToList();
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
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

                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>();
                var linq = from a in base_GoodsInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
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
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;

                var iId = id.Toint(0);
                if (iId == 0)
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "商品ID不能为空");

                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>();
                if (!base_GoodsInfoService.Any(a => a.ID == iId))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

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
                                	m.MerchName
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
                sql += " AND a.ID='" + id + "'";
                #endregion

                var result = base_GoodsInfoService.SqlQuery<Base_GoodsInfoList>(sql).FirstOrDefault();
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);

            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取商品条码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodsBarCode(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>();
                var barcode = "";
                do
                {
                    barcode = Guid.NewGuid().ToString("N");
                }
                while (base_GoodsInfoService.Any(a => a.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase)));
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
                var id = dicParas.ContainsKey("id") ? dicParas["id"].Toint(0) : 0;
                var goodType = dicParas.ContainsKey("goodType") ? (dicParas["goodType"] + "") : string.Empty;
                var goodName = dicParas.ContainsKey("goodName") ? (dicParas["goodName"] + "") : string.Empty;
                var status = dicParas.ContainsKey("status") ? dicParas["status"].Toint(0) : 0;
                var note = dicParas.ContainsKey("note") ? (dicParas["note"] + "") : string.Empty;
                var barCode = dicParas.ContainsKey("barCode") ? (dicParas["barCode"] + "") : string.Empty;

                #region 参数验证
                
                if (string.IsNullOrEmpty(goodType))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "商品类别goodsType不能为空");

                if (string.IsNullOrEmpty(barCode))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "商品条码barCode不能为空");

                #endregion


                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>();
                if (id == 0)
                {
                    if (base_GoodsInfoService.Any(a => a.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品条码已存在");
                    }

                    var base_GoodsInfo = new Base_GoodsInfo();
                    base_GoodsInfo.Barcode = barCode;
                    base_GoodsInfo.GoodType = goodType.Toint();
                    base_GoodsInfo.GoodName = goodName;
                    base_GoodsInfo.MerchID = merchId;
                    base_GoodsInfo.StoreID = storeId;
                    base_GoodsInfo.Status = status;
                    base_GoodsInfo.Note = note;
                    //总店添加的商品是默认入库的？
                    base_GoodsInfo.AllowStorage = userTokenKeyModel.LogType == (int)RoleType.MerchUser ? 1 : 0;
                    if (!base_GoodsInfoService.Add(base_GoodsInfo))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "添加商品信息失败");
                }
                else
                {
                    int iId = Convert.ToInt32(id);
                    if (!base_GoodsInfoService.Any(a => a.ID == iId))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                    var base_GoodsInfo = base_GoodsInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                    if (!string.IsNullOrEmpty(storeId) && base_GoodsInfo.StoreID != storeId)
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "禁止跨门店修改商品信息");
                    if (base_GoodsInfo.MerchID != merchId)
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "禁止修改非此商户下的商品信息");

                    base_GoodsInfo.GoodType = goodType.Toint();
                    base_GoodsInfo.GoodName = goodName;
                    base_GoodsInfo.MerchID = merchId;
                    base_GoodsInfo.StoreID = storeId;
                    base_GoodsInfo.Status = status;
                    base_GoodsInfo.Note = note;

                    if (!base_GoodsInfoService.Update(base_GoodsInfo))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "修改商品信息失败");

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
                var id = dicParas.ContainsKey("id") ? dicParas["id"].Toint(-1) : 0;                

                #region 参数验证
                //如果ID为-1的话说明请求内传了ID但是ID的值不为数字
                if (id == -1)
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "商品ID格式不正确");
                
                #endregion


                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>();
                int iId = Convert.ToInt32(id);
                if (!base_GoodsInfoService.Any(a => a.ID == iId))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var base_GoodsInfo = base_GoodsInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                base_GoodsInfo.AllowStorage = 1;
                if (!base_GoodsInfoService.Update(base_GoodsInfo))
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
                var id = dicParas.ContainsKey("id") ? dicParas["id"].Toint(0) : 0;
                if (id == 0)
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "商品ID不合法");

                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>();
                if (!base_GoodsInfoService.Any(a => a.ID == id))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var base_GoodsInfo = base_GoodsInfoService.GetModels(p => p.ID == id).FirstOrDefault();

                if (!string.IsNullOrEmpty(storeId) && base_GoodsInfo.StoreID != storeId)
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "无法删除其他门店的商品信息");
                if (base_GoodsInfo.MerchID != merchId)
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "无法删除非此商户下的商品信息");

                base_GoodsInfo.Status = 0;

                if (!base_GoodsInfoService.Update(base_GoodsInfo))
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "入库流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);
                IData_GoodStorageService data_GoodStorageService = BLLContainer.Resolve<IData_GoodStorageService>(resolveNew: true);
                if (!data_GoodStorageService.Any(a => a.ID == iId))
                {
                    errMsg = "该入库信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew:true);                
                IData_GoodStorage_DetailService data_GoodStorage_DetailService = BLLContainer.Resolve<IData_GoodStorage_DetailService>(resolveNew:true);
                IBase_GoodsInfoService base_GoodsInfoService = BLLContainer.Resolve<IBase_GoodsInfoService>(resolveNew:true);
                IData_GoodsStockService data_GoodsStockService = BLLContainer.Resolve<IData_GoodsStockService>(resolveNew:true);
                IBase_DepotInfoService base_DepotInfoService = BLLContainer.Resolve<IBase_DepotInfoService>(resolveNew:true);

                int goodTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("商品类别") && p.PID == 0).FirstOrDefault().ID;
                var GoodStorageDetails = from a in data_GoodStorage_DetailService.GetModels(p => p.StorageID == iId)
                                         join b in base_GoodsInfoService.GetModels() on a.GoodID equals b.ID
                                         join c in dict_SystemService.GetModels() on (b.GoodType + "") equals c.DictValue into c1
                                         from c in c1.DefaultIfEmpty()
                                         join d in data_GoodStorageService.GetModels() on a.StorageID equals d.ID
                                         join e in data_GoodsStockService.GetModels() on new { a.GoodID, d.DepotID } equals new { e.GoodID, e.DepotID }
                                         select new
                                         {
                                             Barcode = b.Barcode,
                                             GoodName = b.GoodName,
                                             GoodTypeStr = c != null ? c.DictKey : string.Empty,
                                             RemainCount = e.RemainCount,
                                             Price = a.Price,
                                             StorageCount = a.StorageCount
                                         };

                var result = (from a in data_GoodStorageService.GetModels(p => p.ID == iId)
                              join b in base_DepotInfoService.GetModels() on a.DepotID equals b.ID
                              select new
                              {
                                  DepotID = a.DepotID,
                                  DepotName = b.DepotName,
                                  Supplier = a.Supplier,
                                  Note = a.Note,
                                  GoodStorageDetails = GoodStorageDetails
                              }).FirstOrDefault();
                            
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodInventory(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;                
                int GoodTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("商品类别") && p.PID == 0).FirstOrDefault().ID;
                var linq = from a in base_DepotInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.InventoryEN == 1)
                           join b in
                               (from t in data_GoodsStockService.GetModels()
                                group t by new { DepotID = t.DepotID, GoodID = t.GoodID } into g
                                let topone =
                                (
                                  from item in g
                                  orderby item.InitialTime descending
                                  select new { item.DepotID, item.GoodID, item.InitialValue, item.InitialTime, item.InitialAvgValue, item.RemainCount }
                                ).First()
                                select topone
                                   ) on a.ID equals b.DepotID
                           join c in base_GoodsInfoService.GetModels() on b.GoodID equals c.ID
                           join d in dict_SystemService.GetModels(p => p.PID == GoodTypeId) on (c.GoodType + "") equals d.DictValue into d1
                           from d in d1.DefaultIfEmpty()
                           join e in data_GoodInventoryService.GetModels(p => p.InventoryType == 0) on a.ID equals e.InventoryIndex into e1
                           from e in e1.DefaultIfEmpty()
                           join f in base_StoreInfoService.GetModels() on e.StoreID equals f.StoreID into f1
                           from f in f1.DefaultIfEmpty()
                           select new
                           {
                               ID = e != null ? e.ID : (int?)null,  //礼品盘点ID
                               Barcode = c.Barcode,                 //商品条码
                               GoodTypeStr = d != null ? d.DictKey : string.Empty,//商品类别
                               GoodName = c.GoodName,               //商品名称
                               StoreName = f != null ? f.StoreName : string.Empty,//门店名称
                               DepotName = a.DepotName,             //仓库名称
                               InitialTime = b.InitialTime,         //期初时间
                               InitialValue = b.InitialValue,       //期初值
                               RemainCount = b.RemainCount,           //应有库存
                               RemainValue = Math.Round((b.InitialAvgValue ?? 0M) * (decimal)b.RemainCount, 2, MidpointRounding.AwayFromZero),       //应有库存金额
                               InventoryCount = e != null ? e.InventoryCount : (int?)null //实点数
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

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