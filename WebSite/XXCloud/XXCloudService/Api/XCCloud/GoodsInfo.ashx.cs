using System;
using System.Collections.Generic;
using System.Data;
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
using XCCloudService.WorkFlow;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser, StoreUser")]
    /// <summary>
    /// GoodsInfo 的摘要说明
    /// </summary>
    public class GoodsInfo : ApiBase
    {

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
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
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
                                LEFT JOIN Dict_System c ON a.GoodType = c.ID
                                WHERE
                                	a.Status = 1";
                sql = sql + " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND a.StoreID='" + storeId + "'";
                sql = sql + sqlWhere;
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var storeIds = dicParas.Get("storeIds");

                var query = Base_GoodsInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.Status == 1);
                if (storeIds.IsNull() || storeIds.Contains("|"))
                {
                    query = query.Where(w => (w.StoreID ?? "") == "");
                }
                else
                {
                    query = query.Where(w => ((w.StoreID ?? "") == "" || w.StoreID.Equals(storeIds, StringComparison.OrdinalIgnoreCase)));
                }

                var linq = from a in query
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

                var id = dicParas.Get("id").Toint();

                if (!Base_GoodsInfoService.I.Any(a => a.ID == id))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var goodInfoPrice = from t in
                                        (from a in Base_Goodinfo_PriceService.N.GetModels(p => p.GoodID == id)
                                         join b in Dict_BalanceTypeService.N.GetModels() on a.BalanceIndex equals b.ID
                                         select new { a = a, BalanceIndexStr = b.TypeName })
                                    group t by t.a.BalanceIndex into g
                                    select new
                                    {
                                        BalanceIndex = g.Key,
                                        BalanceIndexStr = g.FirstOrDefault().BalanceIndexStr,
                                        Count0 = g.Any(w => w.a.OperateTypei == 0) ? g.Where(w => w.a.OperateTypei == 0).FirstOrDefault().a.Count : (decimal?)null,
                                        Count1 = g.Any(w => w.a.OperateTypei == 1) ? g.Where(w => w.a.OperateTypei == 1).FirstOrDefault().a.Count : (decimal?)null
                                    };

                var linq = new
                {
                    base_GoodsInfo = (from a in Base_GoodsInfoService.N.GetModels(p => p.ID == id)
                                      join b in Base_StoreInfoService.N.GetModels() on a.StoreID equals b.StoreID into b1
                                      from b in b1.DefaultIfEmpty()
                                      select new { a = a, Source = b != null ? b.StoreName : "总店" }).FirstOrDefault().AsFlatDictionary(),
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var barcode = "";
                do
                {
                    string num5 = Utils.getNumRandomCode(5);
                    barcode = (userTokenKeyModel.LogType == (int)RoleType.MerchUser) ? merchId : storeId;
                    barcode = barcode.PadRight(15, '0') + num5;
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var errMsg = string.Empty;
                if (!dicParas.Get("goodType").Validint("商品类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("barCode").Nonempty("商品条码", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);
                var barCode = dicParas.Get("barCode");
                var goodInfoPrice = dicParas.GetArray("goodInfoPrice");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (Base_GoodsInfoService.I.Any(a => a.ID != id && a.Barcode.Equals(barCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品条码已存在");
                        }

                        var base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Base_GoodsInfo();
                        Utils.GetModel(dicParas, ref base_GoodsInfo);
                        if (id == 0)
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
                            if (base_GoodsInfo.ID == 0)
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
                            foreach (var model in Base_Goodinfo_PriceService.I.GetModels(p => p.GoodID == id))
                            {
                                Base_Goodinfo_PriceService.I.DeleteModel(model);
                            }

                            List<Base_Goodinfo_Price> priceList = new List<Base_Goodinfo_Price>();
                            foreach (IDictionary<string, object> el in goodInfoPrice)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    //if (!dicPara.Get("operateTypei").Validint("操作类别", out errMsg))
                                    //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("balanceIndex").Validint("余额类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("count0").Validdecimal("数量", out errMsg) && !dicPara.Get("count1").Validdecimal("数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    //回购价大于兑换价需要系统提示，operateTypei=0兑换1回购 
                                    //var operateTypei = dicPara.Get("operateTypei").Toint();
                                    var balanceIndex = dicPara.Get("balanceIndex").Toint();
                                    var count0 = dicPara.Get("count0").Todecimal();
                                    var count1 = dicPara.Get("count1").Todecimal();
                                    //if ((operateTypei == 1 && Base_Goodinfo_PriceService.I.Any(a => a.OperateTypei == 0 && a.BalanceIndex == balanceIndex && a.Count < count)) ||
                                    //    (operateTypei == 0 && Base_Goodinfo_PriceService.I.Any(a => a.OperateTypei == 1 && a.BalanceIndex == balanceIndex && a.Count > count)))
                                    if (count0 <= count1)
                                    {
                                        errMsg = "回购价必须小于兑换价";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    if (priceList.Any(a => a.BalanceIndex == balanceIndex))
                                    {
                                        errMsg = "相同余额类别不能重复添加";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var base_Goodinfo_Price = new Base_Goodinfo_Price();
                                    base_Goodinfo_Price.BalanceIndex = balanceIndex;
                                    base_Goodinfo_Price.OperateTypei = 0;
                                    base_Goodinfo_Price.Count = count0;
                                    base_Goodinfo_Price.GoodID = id;
                                    Base_Goodinfo_PriceService.I.AddModel(base_Goodinfo_Price);
                                    priceList.Add(base_Goodinfo_Price);

                                    base_Goodinfo_Price = new Base_Goodinfo_Price();                                    
                                    base_Goodinfo_Price.BalanceIndex = balanceIndex;
                                    base_Goodinfo_Price.OperateTypei = 1;
                                    base_Goodinfo_Price.Count = count1;
                                    base_Goodinfo_Price.GoodID = id;
                                    Base_Goodinfo_PriceService.I.AddModel(base_Goodinfo_Price);
                                    priceList.Add(base_Goodinfo_Price);
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
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var errMsg = string.Empty;
                if (!dicParas.Get("id").Validint("商品ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("allowStorage").Validint("是否允许入库", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);
                var allowStorage = dicParas.Get("allowStorage").Toint();

                if (!Base_GoodsInfoService.I.Any(a => a.ID == id))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();
                base_GoodsInfo.AllowStorage = allowStorage;
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var errMsg = string.Empty;
                if (!dicParas.Get("id").Validint("商品ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                if (!Base_GoodsInfoService.I.Any(a => a.ID == id))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "该商品信息不存在");

                var base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();

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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UploadGoodPhoto(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                Dictionary<string, string> imageInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                List<string> imageUrls = new List<string>();
                if (!Utils.UploadImageFile("/XCCloud/Good/", out imageUrls, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                imageInfo.Add("ImageURL", imageUrls.First());

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, imageInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #endregion

        #region 商品调拨管理
        /// <summary>
        /// 商品查询列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodRequest(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);


                #region Sql语句
                string sql = @"SELECT
                                    /*调拨单ID*/
                                	a.ID,
                                    /*调拨单号*/
                                	a.RequestCode,
                                	/*创建时间*/
                                	(case when ISNULL(a.CreateTime,'')='' then '' else convert(varchar,a.CreateTime,20) end) AS CreateTime,
                                	/*创建门店*/
                                	(case when ISNULL(createstore.StoreName,'')='' then '总店' else createstore.StoreName end) AS CreateStore,
                                	/*创建人*/
                                	u.LogName AS CreateUser,                                	
                                	/*调拨方式*/
                                	a.RequstType,
                                    /*调拨出库门店*/
                                    a.RequestOutStoreID AS OutStoreID,
                                    (case when ISNULL(requestoutstore.StoreName,'')='' then '总店' else requestoutstore.StoreName end) AS OutStoreName,
                                    /*调拨出库仓库*/
                                    a.RequestOutDepotID AS OutDepotID,
                                	requestoutdepot.DepotName AS OutDepotName,                                    
                                	/*调拨入库门店*/
                                    a.RequestInStoreID AS InStoreID,
                                	(case when ISNULL(requestinstore.StoreName,'')='' then '总店' else requestinstore.StoreName end) AS InStoreName,
                                	/*调拨入库仓库*/
                                    a.RequestInDepotID AS InDepotID,
                                	requestindepot.DepotName AS InDepotName,
                                    /*调拨原因*/
                                    a.RequestReason AS RequestReason,
                                    /*出库时间*/
                                	(case when ISNULL(b.CreateTime,'')='' then '' else convert(varchar,b.CreateTime,20) end) AS OutDepotTime,                                	
                                    /*入库时间*/
                                	(case when ISNULL(c.CreateTime,'')='' then '' else convert(varchar,c.CreateTime,20) end) AS InDepotTime,
                                    /*调拨状态*/
                                	d.State,
                                	/*调拨说明*/
                                	d.Note
                                FROM
                                	Data_GoodRequest a
                                LEFT JOIN Base_StoreInfo createstore ON a.CreateStoreID = createstore.StoreID
                                LEFT JOIN Base_UserInfo u ON a.CreateUserID = u.UserID                                
                                LEFT JOIN Base_StoreInfo requestoutstore ON a.RequestOutStoreID = requestoutstore.StoreID
                                LEFT JOIN Base_StoreInfo requestinstore ON a.RequestInStoreID = requestinstore.StoreID
                                LEFT JOIN Base_DepotInfo requestoutdepot ON a.RequestOutDepotID = requestoutdepot.ID
                                LEFT JOIN Base_DepotInfo requestindepot ON a.RequestInDepotID = requestindepot.ID
                                LEFT JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by EventID order by CreateTime desc) as RowNum
                                	FROM
                                		Data_WorkFlow_Entry                                                         
                                	WHERE
                                		State = 4 /*调拨出库*/
                                    AND EventType = 0 /*产品调拨对应表格 Data_GoodRequest*/
                                ) b ON a.ID = b.EventID and b.RowNum <= 1
                                LEFT JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by EventID order by CreateTime desc) as RowNum
                                	FROM
                                		Data_WorkFlow_Entry                                                         
                                	WHERE
                                		State = 7 /*调拨入库*/
                                    AND EventType = 0 /*产品调拨对应表格 Data_GoodRequest*/                                                                            
                                ) c ON a.ID = c.EventID and c.RowNum <= 1
                                LEFT JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by EventID order by CreateTime desc) as RowNum
                                	FROM
                                		Data_WorkFlow_Entry                                                         
                                	WHERE
                                		EventType = 0 /*产品调拨对应表格 Data_GoodRequest*/
                                ) d ON a.ID = d.EventID and d.RowNum <= 1
                                WHERE 1=1
                            ";
                sql = sql + " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND (a.CreateStoreID='" + storeId + "' or a.RequestOutStoreID='" + storeId + "' or a.RequestInStoreID='" + storeId + "')";
                sql = sql + sqlWhere;
                sql = sql + " ORDER BY a.CreateTime desc";
                #endregion

                var list = Data_GoodRequestService.I.SqlQuery<Data_GoodRequestList>(sql, parameters).ToList();
                foreach (var model in list)
                {
                    model.PermittedTriggers = new GoodReqWorkFlow(model.ID, userId, logType, storeId).PermittedTriggers.Cast<int>();
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        

        /// <summary>
        /// 查询调拨详情
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodRequestDetail(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                
                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                if (!Data_GoodRequestService.I.Any(p => p.ID == requestId))
                {
                    errMsg = "该调拨单信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
  
                #region Sql语句
                string sql = @"SELECT
                                    /*调拨明细ID*/
                                    a.ID,
                                    /*商品ID*/
                                	a.GoodID,
                                    /*商品条码*/
                                	c.Barcode,
                                    /*商品名称*/
                                	c.GoodName,
                                    /*商品类别*/
                                    d.DictKey AS GoodTypeStr,
                                    /*申请数量*/
                                    a.RequestCount,
                                    /*调拨数量*/
                                    a.SendCount,
                                    /*入库数量*/
                                    a.StorageCount,
                                    /*含税价格*/
                                    a.CostPrice,
                                    /*税率*/
                                    a.Tax,
                                    /*出库仓库*/
                                    a.OutDepotID,
                                    /*出库仓库*/
                                    f.DepotName AS OutDepotName,
                                	/*库存*/
                                	ISNULL(b.RemainCount,0) AS RemainCount,
                                    ISNULL(b.MinValue,0) AS MinValue,                                    
                                    /*快递物流*/
                                    a.LogistType,
                                    /*快递物流[名称]*/
                                    e.DictKey AS LogistTypeStr,
                                    /*物流单号*/
                                    a.LogistOrderID,
                                    /*发货时间*/
                                    (case when ISNULL(a.SendTime,'')='' then '' else convert(varchar,a.SendTime,20) end) AS SendTime
                                FROM
                                	Data_GoodRequest_List a
                                LEFT JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by StockIndex,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                    WHERE 
                                        StockType = 0
                                ) b ON a.OutDepotID = b.StockIndex AND a.GoodID = b.GoodID and b.RowNum <= 1
                                INNER JOIN Base_GoodsInfo c ON a.GoodID = c.ID 
                                INNER JOIN Dict_System d ON c.GoodType = d.ID
                                LEFT JOIN (
                                	SELECT
                                		b.*
                                	FROM
                                		Dict_System a
                                	INNER JOIN Dict_System b ON a.ID = b.PID
                                	WHERE
                                		a.DictKey = '物流公司'
                                	AND a.PID = 0
                                ) e ON CONVERT (VARCHAR, a.LogistType) = e.DictValue
                                LEFT JOIN Base_DepotInfo f ON a.OutDepotID = f.ID 
                                WHERE c.Status = 1
                            ";
                sql += " AND a.RequestID=" + requestId;
              
                #endregion

                var list = Data_GoodRequest_ListService.I.SqlQuery<Data_GoodRequest_ListList>(sql);

                sql = @"SELECT
                                    /*调拨单ID*/
                                	a.ID,
                                    /*调拨单号*/
                                	a.RequestCode,
                                	/*创建时间*/
                                	(case when ISNULL(a.CreateTime,'')='' then '' else convert(varchar,a.CreateTime,20) end) AS CreateTime,
                                	/*创建门店*/
                                	createstore.StoreName AS CreateStore,
                                	/*创建人*/
                                	u.LogName AS CreateUser,                                	
                                	/*调拨方式*/
                                	a.RequstType,
                                    /*调拨出库门店*/
                                    requestoutstore.StoreID AS OutStoreID,
                                    requestoutstore.StoreName AS OutStoreName,
                                    /*调拨出库仓库*/
                                    requestoutdepot.ID AS OutDepotID,
                                	requestoutdepot.DepotName AS OutDepotName,                                    
                                	/*调拨入库门店*/
                                    requestinstore.StoreID AS InStoreID,
                                	requestinstore.StoreName AS InStoreName,
                                	/*调拨入库仓库*/
                                    requestindepot.ID AS InDepotID,
                                	requestindepot.DepotName AS InDepotName,
                                    /*调拨原因*/
                                    a.RequestReason AS RequestReason
                                FROM
                                	Data_GoodRequest a
                                LEFT JOIN Base_StoreInfo createstore ON a.CreateStoreID = createstore.StoreID
                                LEFT JOIN Base_UserInfo u ON a.CreateUserID = u.UserID                                
                                LEFT JOIN Base_StoreInfo requestoutstore ON a.RequestOutStoreID = requestoutstore.StoreID
                                LEFT JOIN Base_StoreInfo requestinstore ON a.RequestInStoreID = requestinstore.StoreID
                                LEFT JOIN Base_DepotInfo requestoutdepot ON a.RequestOutDepotID = requestoutdepot.ID
                                LEFT JOIN Base_DepotInfo requestindepot ON a.RequestInDepotID = requestindepot.ID 
                                WHERE a.ID = " + requestId;
                var data_GoodRequest = Data_GoodRequestService.I.SqlQuery<Data_GoodRequestList>(sql).FirstOrDefault();

                var linq = new {
                    data_GoodRequest = data_GoodRequest,
                    GoodRequestDetails = list
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取调拨出库信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodSendDealInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;

                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }              

                if (!Data_GoodRequestService.I.Any(p => p.ID == requestId))
                {
                    errMsg = "该调拨单信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #region Sql语句
                string sql = @"SELECT
                                    /*调拨单ID*/
                                    a.RequestID,
                                    /*商品ID*/
                                	a.GoodID,
                                    /*商品条码*/
                                	b.Barcode,
                                    /*商品名称*/
                                	b.GoodName,
                                    /*商品类别*/
                                    c.DictKey AS GoodTypeStr,
                                    /*申请数量*/
                                    a.RequestCount,
                                    /*已调拨数量*/
                                    a.FinishCount
                                FROM (
                                    SELECT RequestID, GoodID, RequestCount, ISNULL(SUM(SendCount),0) AS FinishCount
                                    FROM
                                	    Data_GoodRequest_List
                                    GROUP BY RequestID, GoodID, RequestCount
                                ) a                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID 
                                INNER JOIN Dict_System c ON b.GoodType = c.ID                              
                                WHERE b.Status = 1
                            ";
                sql += " AND a.RequestID=" + requestId;
                
                #endregion

                var list = Data_GoodRequest_ListService.I.SqlQuery<Data_GoodSendDealList>(sql);

                sql = @"SELECT
                                    /*调拨单ID*/
                                	a.ID,
                                    /*调拨单号*/
                                	a.RequestCode,
                                	/*创建时间*/
                                	(case when ISNULL(a.CreateTime,'')='' then '' else convert(varchar,a.CreateTime,20) end) AS CreateTime,
                                	/*创建门店*/
                                	createstore.StoreName AS CreateStore,
                                	/*创建人*/
                                	u.LogName AS CreateUser,                                	
                                	/*调拨方式*/
                                	a.RequstType,
                                    /*调拨出库门店*/
                                    requestoutstore.StoreID AS OutStoreID,
                                    requestoutstore.StoreName AS OutStoreName,
                                    /*调拨出库仓库*/
                                    requestoutdepot.ID AS OutDepotID,
                                	requestoutdepot.DepotName AS OutDepotName,                                    
                                	/*调拨入库门店*/
                                    requestinstore.StoreID AS InStoreID,
                                	requestinstore.StoreName AS InStoreName,
                                	/*调拨入库仓库*/
                                    requestindepot.ID AS InDepotID,
                                	requestindepot.DepotName AS InDepotName,
                                    /*调拨原因*/
                                    a.RequestReason AS RequestReason
                                FROM
                                	Data_GoodRequest a
                                LEFT JOIN Base_StoreInfo createstore ON a.CreateStoreID = createstore.StoreID
                                LEFT JOIN Base_UserInfo u ON a.CreateUserID = u.UserID                                
                                LEFT JOIN Base_StoreInfo requestoutstore ON a.RequestOutStoreID = requestoutstore.StoreID
                                LEFT JOIN Base_StoreInfo requestinstore ON a.RequestInStoreID = requestinstore.StoreID
                                LEFT JOIN Base_DepotInfo requestoutdepot ON a.RequestOutDepotID = requestoutdepot.ID
                                LEFT JOIN Base_DepotInfo requestindepot ON a.RequestInDepotID = requestindepot.ID 
                                WHERE a.ID = " + requestId;
                var data_GoodRequest = Data_GoodRequestService.I.SqlQuery<Data_GoodRequestList>(sql).FirstOrDefault();

                var linq = new
                {
                    data_GoodRequest = data_GoodRequest,
                    GoodRequestDetails = list
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取调拨入库信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodRequestDealInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;

                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Data_GoodRequestService.I.Any(p => p.ID == requestId))
                {
                    errMsg = "该调拨单信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #region Sql语句
                string sql = @"SELECT
                                    /*调拨明细ID*/
                                    a.ID,
                                    /*调拨单ID*/
                                    a.RequestID,
                                    /*商品ID*/
                                	a.GoodID,
                                    /*商品条码*/
                                	b.Barcode,
                                    /*商品名称*/
                                	b.GoodName,
                                    /*商品类别*/
                                    c.DictKey AS GoodTypeStr,
                                    /*申请数量*/
                                    a.RequestCount,
                                    /*实发数量*/
                                    a.SendCount,
                                    /*含税单价*/
                                    a.CostPrice,
                                    /*税率*/
                                    a.Tax
                                FROM (
                                    SELECT
                                		*
                                	FROM
                                		Data_GoodRequest_List 
                                    WHERE StorageCount = 0 /*未入库记录*/                                
                                ) a                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID
                                INNER JOIN Dict_System c ON b.GoodType = c.ID                              
                                WHERE b.Status = 1                                
                            ";
                sql += " AND a.RequestID=" + requestId;
                sql += " ORDER BY a.SendTime";

                #endregion

                var list = Data_GoodRequest_ListService.I.SqlQuery<Data_GoodRequestDealList>(sql);

                sql = @"SELECT
                                    /*调拨单ID*/
                                	a.ID,
                                    /*调拨单号*/
                                	a.RequestCode,
                                	/*创建时间*/
                                	(case when ISNULL(a.CreateTime,'')='' then '' else convert(varchar,a.CreateTime,20) end) AS CreateTime,
                                	/*创建门店*/
                                	createstore.StoreName AS CreateStore,
                                	/*创建人*/
                                	u.LogName AS CreateUser,                                	
                                	/*调拨方式*/
                                	a.RequstType,
                                    /*调拨出库门店*/
                                    requestoutstore.StoreID AS OutStoreID,
                                    requestoutstore.StoreName AS OutStoreName,
                                    /*调拨出库仓库*/
                                    requestoutdepot.ID AS OutDepotID,
                                	requestoutdepot.DepotName AS OutDepotName,                                    
                                	/*调拨入库门店*/
                                    requestinstore.StoreID AS InStoreID,
                                	requestinstore.StoreName AS InStoreName,
                                	/*调拨入库仓库*/
                                    requestindepot.ID AS InDepotID,
                                	requestindepot.DepotName AS InDepotName,
                                    /*调拨原因*/
                                    a.RequestReason AS RequestReason
                                FROM
                                	Data_GoodRequest a
                                LEFT JOIN Base_StoreInfo createstore ON a.CreateStoreID = createstore.StoreID
                                LEFT JOIN Base_UserInfo u ON a.CreateUserID = u.UserID                                
                                LEFT JOIN Base_StoreInfo requestoutstore ON a.RequestOutStoreID = requestoutstore.StoreID
                                LEFT JOIN Base_StoreInfo requestinstore ON a.RequestInStoreID = requestinstore.StoreID
                                LEFT JOIN Base_DepotInfo requestoutdepot ON a.RequestOutDepotID = requestoutdepot.ID
                                LEFT JOIN Base_DepotInfo requestindepot ON a.RequestInDepotID = requestindepot.ID 
                                WHERE a.ID = " + requestId;
                var data_GoodRequest = Data_GoodRequestService.I.SqlQuery<Data_GoodRequestList>(sql).FirstOrDefault();

                var linq = new
                {
                    data_GoodRequest = data_GoodRequest,
                    GoodRequestDetails = list
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 调拨申请或创建调拨单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodRequest(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                if (!dicParas.Get("requstType").Validint("调拨方式", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("goodRequestDetails").Validarray("调拨明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var requstType = dicParas.Get("requstType").Toint();
                if (requstType == 0 || requstType == 3)
                {
                    if (!dicParas.Get("outStoreId").Nonempty("出库门店ID", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (requstType == 2)
                {       
                    if (!dicParas.Get("inStoreId").Nonempty("调配门店ID", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicParas.Get("outDepotId").Validint("出库仓库ID", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicParas.Get("logistType").Validint("物流类别", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicParas.Get("logistOrderId").Nonempty("物流单号", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (requstType == 0 || requstType == 1 || requstType == 3)
                {
                    if (!dicParas.Get("inDepotId").Validint("入库仓库ID", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var outStoreId = dicParas.Get("outStoreId");
                var outDepotId = dicParas.Get("outDepotId").Toint();
                var inStoreId = (requstType == 0 || requstType == 1 || requstType == 3) ? storeId : dicParas.Get("inStoreId");  //门店向总部申请，入库门店默认为当前门店
                var inDepotId = dicParas.Get("inDepotId").Toint();
                var requestReason = dicParas.Get("requestReason");
                var logistType = dicParas.Get("logistType").Toint();
                var logistOrderId = dicParas.Get("logistOrderId");
                var goodRequestDetails = dicParas.GetArray("goodRequestDetails");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {                        
                        var data_GoodRequest = new Data_GoodRequest();
                        data_GoodRequest.CreateStoreID = storeId;
                        data_GoodRequest.MerchID = merchId;
                        data_GoodRequest.CreateUserID = userTokenKeyModel.LogId.Toint();
                        data_GoodRequest.CreateTime = DateTime.Now;
                        data_GoodRequest.RequstType = requstType;
                        data_GoodRequest.RequestCode = RedisCacheHelper.CreateCloudSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);
                        data_GoodRequest.RequestReason = requestReason;
                        data_GoodRequest.RequestInStoreID = inStoreId;
                        data_GoodRequest.RequestInDepotID = inDepotId;
                        data_GoodRequest.RequestOutStoreID = outStoreId;
                        data_GoodRequest.RequestOutDepotID = outDepotId;
                        data_GoodRequest.CheckDate = getCurrCheckDate(merchId, storeId);

                        if (!Data_GoodRequestService.I.Add(data_GoodRequest))
                        {
                            errMsg = "创建调拨单失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var requestId = data_GoodRequest.ID;                        

                        //添加调拨明细
                        if (goodRequestDetails != null && goodRequestDetails.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodRequestDetails)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("goodId").Validintnozero("商品ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    
                                    if (requstType == 2)
                                    {
                                        if (!dicPara.Get("sendCount").Validintnozero("实发数量", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        if (!dicPara.Get("costPrice").Validdecimalnozero("含税单价", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        if (!dicPara.Get("tax").Validdecimalnozero("税率", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                                                                                
                                    }
                                    else
                                    {
                                        if (!dicPara.Get("requestCount").Validintnozero("申请数量", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                                        
                                    }

                                    var goodId = dicPara.Get("goodId").Toint();
                                    var requestCount = dicPara.Get("requestCount").Toint(0);
                                    var sendCount = dicPara.Get("sendCount").Toint(0);
                                    var costPrice = dicPara.Get("costPrice").Todecimal(0);
                                    var tax = dicPara.Get("tax").Todecimal(0);

                                    if (requstType == 2)
                                    {
                                        //期初平均成本大于0才能出库
                                        if (!Data_GoodsStockService.I.Any(a => a.StockType == (int)StockType.Depot && a.StockIndex == outDepotId && a.GoodID == goodId && a.InitialAvgValue > 0))
                                        {
                                            errMsg = "该商品库存不符合出库条件, 期初平均成本须大于0";
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        }
                                    }

                                    var data_GoodRequest_List = new Data_GoodRequest_List();
                                    data_GoodRequest_List.RequestID = requestId;
                                    data_GoodRequest_List.GoodID = goodId;
                                    data_GoodRequest_List.InStoreID = inStoreId;
                                    data_GoodRequest_List.InDeportID = inDepotId;
                                    data_GoodRequest_List.OutStoreID = outStoreId;
                                    data_GoodRequest_List.OutDepotID = outDepotId;
                                    data_GoodRequest_List.RequestCount = requestCount;
                                    data_GoodRequest_List.SendCount = sendCount;
                                    data_GoodRequest_List.StorageCount = 0;
                                    data_GoodRequest_List.CostPrice = costPrice;
                                    data_GoodRequest_List.Tax = tax;
                                    data_GoodRequest_List.MerchID = merchId;
                                    data_GoodRequest_List.LogistType = logistType;
                                    data_GoodRequest_List.LogistOrderID = logistOrderId;
                                    data_GoodRequest_List.SendTime = DateTime.Now;
                                    Data_GoodRequest_ListService.I.AddModel(data_GoodRequest_List);

                                    if (requstType == 2)
                                    {
                                        //更新当前库存
                                        if (!XCCloudBLLExt.UpdateGoodsStock(outDepotId, goodId, (int)SourceType.GoodRequest, requestId, costPrice, (int)StockFlag.Out, sendCount, merchId, storeId, out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodRequest_ListService.I.SaveChanges())
                            {
                                errMsg = "添加调拨明细失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStock_RecordService.I.SaveChanges())
                            {
                                errMsg = "添加出库存异动信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //工作流更新
                        var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
                        if (requstType == 2)
                        {
                            if (!wf.SendDeal(out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        else
                        {
                            if (!wf.Request(out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        /// <summary>
        /// 调拨申请审核
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodRequestVerify(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                
                if (!dicParas.Get("state").Validint("调拨申请审核状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var state = dicParas.Get("state").Toint(0);
                var note = dicParas.Get("note");                

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
                        {
                            errMsg = "该调拨单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //工作流更新
                        var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
                        if (!wf.RequestVerify(state, note, out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        ///// <summary>
        ///// 调拨出库审核
        ///// </summary>
        ///// <param name="dicParas"></param>
        ///// <returns></returns>
        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        //public object GoodSendDealVerify(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
        //        var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
        //        var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
        //        var userId = userTokenKeyModel.LogId.Toint(0);
        //        var logType = userTokenKeyModel.LogType;

        //        var errMsg = string.Empty;
        //        var requestId = dicParas.Get("requestId").Toint(0);
        //        if (requestId == 0)
        //        {
        //            errMsg = "调拨单ID不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }
        //        if (!dicParas.Get("state").Validint("调拨出库审核状态", out errMsg))
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

        //        var state = dicParas.Get("state").Toint(0);
        //        var note = dicParas.Get("note");

        //        //开启EF事务
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            try
        //            {
        //                if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
        //                {
        //                    errMsg = "该调拨单不存在";
        //                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                }

        //                //工作流更新
        //                var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
        //                if (!wf.SendDealVerify(state, note, out errMsg))
        //                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

        //                var data_GoodRequest_List = Data_GoodRequest_ListService.I;
        //                foreach (var requestListModel in data_GoodRequest_List.GetModels(p => p.RequestID == requestId))
        //                {
        //                    //更新当前库存
        //                    if (!XCCloudBLLExt.UpdateGoodsStock(requestListModel.OutDepotID, requestListModel.GoodID, (int)SourceType.GoodRequest, requestId, requestListModel.CostPrice, (int)StockFlag.Out, requestListModel.SendCount, merchId, storeId, out errMsg))
        //                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                }

        //                if (!Data_GoodStock_RecordService.I.SaveChanges())
        //                {
        //                    errMsg = "添加出库存异动信息失败";
        //                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                }

        //                ts.Complete();
        //            }
        //            catch (DbEntityValidationException e)
        //            {
        //                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
        //            }
        //            catch (Exception e)
        //            {
        //                errMsg = e.Message;
        //                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //            }
        //        }

        //        return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
        //    }
        //    catch (Exception e)
        //    {
        //        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
        //    }
        //}

        ///// <summary>
        ///// 调拨入库审核
        ///// </summary>
        ///// <param name="dicParas"></param>
        ///// <returns></returns>
        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        //public object GoodRequestDealVerify(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
        //        var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
        //        var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
        //        var userId = userTokenKeyModel.LogId.Toint(0);
        //        var logType = userTokenKeyModel.LogType;

        //        var errMsg = string.Empty;
        //        var requestId = dicParas.Get("requestId").Toint(0);
        //        if (requestId == 0)
        //        {
        //            errMsg = "调拨单ID不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }
        //        if (!dicParas.Get("state").Validint("调拨入库审核状态", out errMsg))
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

        //        var state = dicParas.Get("state").Toint(0);
        //        var note = dicParas.Get("note");

        //        //开启EF事务
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            try
        //            {
        //                if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
        //                {
        //                    errMsg = "该调拨单不存在";
        //                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                }

        //                //工作流更新
        //                var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
        //                if (!wf.RequestDealVerify(state, note, out errMsg))
        //                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

        //                var data_GoodRequest_List = Data_GoodRequest_ListService.I;
        //                foreach (var requestListModel in data_GoodRequest_List.GetModels(p => p.RequestID == requestId))
        //                {
        //                    //更新当前库存
        //                    if (!XCCloudBLLExt.UpdateGoodsStock(requestListModel.InDeportID, requestListModel.GoodID, (int)SourceType.GoodRequest, requestId, requestListModel.CostPrice, (int)StockFlag.In, requestListModel.StorageCount, merchId, storeId, out errMsg))
        //                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                }

        //                if (!Data_GoodStock_RecordService.I.SaveChanges())
        //                {
        //                    errMsg = "添加入库存异动信息失败";
        //                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                }

        //                ts.Complete();
        //            }
        //            catch (DbEntityValidationException e)
        //            {
        //                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
        //            }
        //            catch (Exception e)
        //            {
        //                errMsg = e.Message;
        //                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //            }
        //        }

        //        return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
        //    }
        //    catch (Exception e)
        //    {
        //        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
        //    }
        //}

        /// <summary>
        /// 调拨出库
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodSendDeal(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!dicParas.Get("logistType").Validint("物流类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("logistOrderId").Nonempty("物流单号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("goodRequestDetails").Validarray("调拨明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var logistType = dicParas.Get("logistType").Toint();
                var logistOrderId = dicParas.Get("logistOrderId");
                var goodRequestDetails = dicParas.GetArray("goodRequestDetails");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
                        {
                            errMsg = "该调拨单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //工作流更新
                        var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
                        if (!wf.SendDeal(out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                        

                        //添加调拨明细
                        if (goodRequestDetails != null && goodRequestDetails.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodRequestDetails)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("outDepotId").Validintnozero("出库仓库ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("goodId").Validintnozero("商品ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                                    
                                    if (!dicPara.Get("sendCount").Validintnozero("实发数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("costPrice").Validdecimalnozero("含税单价", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("tax").Validdecimalnozero("税率", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var goodId = dicPara.Get("goodId").Toint();
                                    var outDepotId = dicPara.Get("outDepotId").Toint();
                                    var sendCount = dicPara.Get("sendCount").Toint();
                                    var costPrice = dicPara.Get("costPrice").Todecimal();
                                    var tax = dicPara.Get("tax").Todecimal();

                                    if (!Data_GoodRequest_ListService.I.Any(a => a.RequestID == requestId && a.GoodID == goodId))
                                    {
                                        errMsg = "该调拨明细信息不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    //期初平均成本大于0才能出库
                                    if (!Data_GoodsStockService.I.Any(a => a.StockType == (int)StockType.Depot && a.StockIndex == outDepotId && a.GoodID == goodId && a.InitialAvgValue > 0))
                                    {
                                        errMsg = "该商品库存不符合出库条件, 期初平均成本须大于0";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    //更新调拨明细信息
                                    var data_GoodRequest_List = Data_GoodRequest_ListService.I.GetModels(a => a.RequestID == requestId && a.GoodID == goodId).OrderByDescending(or => or.ID).FirstOrDefault();
                                    data_GoodRequest_List.OutDepotID = outDepotId;                                    
                                    data_GoodRequest_List.StorageCount = 0;
                                    data_GoodRequest_List.CostPrice = costPrice;
                                    data_GoodRequest_List.Tax = tax;
                                    data_GoodRequest_List.LogistType = logistType;
                                    data_GoodRequest_List.LogistOrderID = logistOrderId;
                                    data_GoodRequest_List.SendTime = DateTime.Now;
                                    if (data_GoodRequest_List.SendCount == 0)
                                    {
                                        data_GoodRequest_List.SendCount = sendCount;
                                        Data_GoodRequest_ListService.I.UpdateModel(data_GoodRequest_List);                                        
                                    }
                                    else
                                    {
                                        data_GoodRequest_List.SendCount = sendCount;
                                        data_GoodRequest_List.MerchID = merchId;
                                        Data_GoodRequest_ListService.I.AddModel(data_GoodRequest_List);                                          
                                    }                                    
                                    
                                    //更新调拨单信息
                                    var data_GoodRequest = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).FirstOrDefault();
                                    data_GoodRequest.RequestOutDepotID = outDepotId;
                                    Data_GoodRequestService.I.UpdateModel(data_GoodRequest);
                                                                                                        
                                    //如何不需要审核
                                    if (!wf.CanFire(Trigger.SendDealVerify))
                                    {
                                        //更新当前库存
                                        if (!XCCloudBLLExt.UpdateGoodsStock(outDepotId, goodId, (int)SourceType.GoodRequest, requestId, costPrice, (int)StockFlag.Out, sendCount, merchId, storeId, out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }                                                                        
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodRequest_ListService.I.SaveChanges())
                            {
                                errMsg = "更新调拨明细失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodRequestService.I.SaveChanges())
                            {
                                errMsg = "更新调拨单失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStock_RecordService.I.SaveChanges())
                            {
                                errMsg = "添加出库存异动信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        /// <summary>
        /// 调拨入库
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodRequestDeal(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                if (!dicParas.Get("requestId").Validintnozero("调拨单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);               
                if (!dicParas.Get("inDepotId").Validintnozero("入库仓库ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("goodRequestDetails").Validarray("调拨明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var requestId = dicParas.Get("requestId").Toint(0);
                var inDepotId = dicParas.Get("inDepotId").Toint();
                var goodRequestDetails = dicParas.GetArray("goodRequestDetails");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
                        {
                            errMsg = "该调拨单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //工作流更新
                        var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
                        if (!wf.RequestDeal(out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                        //添加调拨明细
                        if (goodRequestDetails != null && goodRequestDetails.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodRequestDetails)
                            {
                                if (el != null)
                                {
                                    //更新调拨明细
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("id").Validintnozero("调拨明细ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                                                               
                                    if (!dicPara.Get("storageCount").Validintnozero("入库数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var id = dicPara.Get("id").Toint();
                                    var storageCount = dicPara.Get("storageCount").Toint();
                                    var data_GoodRequest_List = Data_GoodRequest_ListService.I.GetModels(p => p.ID == id).FirstOrDefault();
                                    data_GoodRequest_List.StorageCount = storageCount;
                                    data_GoodRequest_List.InDeportID = inDepotId;
                                    Data_GoodRequest_ListService.I.UpdateModel(data_GoodRequest_List);

                                    //更新调拨单
                                    var data_GoodRequest = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).FirstOrDefault();
                                    data_GoodRequest.RequestInDepotID = inDepotId;
                                    Data_GoodRequestService.I.UpdateModel(data_GoodRequest);                                    

                                    //如何不需要审核
                                    if (!wf.CanFire(Trigger.RequestDealVerify))
                                    {
                                        //更新当前库存
                                        if (!XCCloudBLLExt.UpdateGoodsStock(inDepotId, data_GoodRequest_List.GoodID, (int)SourceType.GoodRequest, requestId, data_GoodRequest_List.CostPrice, (int)StockFlag.In, storageCount, merchId, storeId, out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                                        
                                    }                                    
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodRequest_ListService.I.SaveChanges())
                            {
                                errMsg = "更新调拨明细失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodRequestService.I.SaveChanges())
                            {
                                errMsg = "更新调拨单失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStock_RecordService.I.SaveChanges())
                            {
                                errMsg = "添加入库存异动信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }                        

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        /// <summary>
        /// 流程关闭
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodRequestClose(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                if (!dicParas.Get("requestId").Validintnozero("调拨单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var requestId = dicParas.Get("requestId").Toint(0);

                if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
                {
                    errMsg = "该调拨单不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //工作流更新
                var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
                if (!wf.Close(out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 流程撤销
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodRequestCancel(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                if (!dicParas.Get("requestId").Validintnozero("调拨单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var requestId = dicParas.Get("requestId").Toint(0);

                if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
                {
                    errMsg = "该调拨单不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //工作流更新
                var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
                if (!wf.Cancel(out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取调拨退货信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodRequestExitInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;

                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Data_GoodRequestService.I.Any(p => p.ID == requestId))
                {
                    errMsg = "该调拨单信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var requestCode = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).Select(o => o.RequestCode).FirstOrDefault();

                #region Sql语句
                string sql = @"SELECT
                                    /*调拨明细ID*/
                                    a.ID,
                                    /*调拨单ID*/
                                    a.RequestID,
                                    /*商品ID*/
                                	a.GoodID,
                                    /*商品条码*/
                                	b.Barcode,
                                    /*商品名称*/
                                	b.GoodName,
                                    /*商品类别*/
                                    c.DictKey AS GoodTypeStr,
                                    /*申请数量*/
                                    a.RequestCount,
                                    /*实发数量*/
                                    a.SendCount,
                                    /*入库数量*/
                                    a.StorageCount,
                                    /*已退货数量*/
                                    ISNULL(d.ExitedCount,0) AS ExitedCount,
                                    /*含税单价*/
                                    a.CostPrice,
                                    /*税率*/
                                    a.Tax
                                FROM (
                                    SELECT
                                		*
                                	FROM
                                		Data_GoodRequest_List 
                                    WHERE StorageCount > 0 /*已入库记录*/                                
                                ) a                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID
                                INNER JOIN Dict_System c ON b.GoodType = c.ID 
                                LEFT JOIN (
                                    SELECT 
                                        b.GoodID, SUM(ISNULL(b.ExitCount,0)) AS ExitedCount
                                    FROM 
                                        Data_GoodExitInfo a
                                    INNER JOIN Data_GoodExit_Detail b ON a.ExitOrderID = b.ExitOrderID 
                                    WHERE a.SourceType = 1 AND a.SourceOrderID = " + requestCode +
                                @"  GROUP BY b.GoodID
                                ) d ON a.GoodID = d.GoodID                             
                                WHERE b.Status = 1                                
                            ";
                sql += " AND a.RequestID=" + requestId;
                sql += " ORDER BY a.SendTime";

                #endregion

                var list = Data_GoodRequest_ListService.I.SqlQuery<Data_GoodRequestExitList>(sql);

                sql = @"SELECT
                                    /*调拨单ID*/
                                	a.ID,
                                    /*调拨单号*/
                                	a.RequestCode,
                                	/*创建时间*/
                                	(case when ISNULL(a.CreateTime,'')='' then '' else convert(varchar,a.CreateTime,20) end) AS CreateTime,
                                	/*创建门店*/
                                	createstore.StoreName AS CreateStore,
                                	/*创建人*/
                                	u.LogName AS CreateUser,                                	
                                	/*调拨方式*/
                                	a.RequstType,
                                    /*调拨出库门店*/
                                    requestoutstore.StoreID AS OutStoreID,
                                    requestoutstore.StoreName AS OutStoreName,
                                    /*调拨出库仓库*/
                                    requestoutdepot.ID AS OutDepotID,
                                	requestoutdepot.DepotName AS OutDepotName,                                    
                                	/*调拨入库门店*/
                                    requestinstore.StoreID AS InStoreID,
                                	requestinstore.StoreName AS InStoreName,
                                	/*调拨入库仓库*/
                                    requestindepot.ID AS InDepotID,
                                	requestindepot.DepotName AS InDepotName,
                                    /*调拨原因*/
                                    a.RequestReason AS RequestReason
                                FROM
                                	Data_GoodRequest a
                                LEFT JOIN Base_StoreInfo createstore ON a.CreateStoreID = createstore.StoreID
                                LEFT JOIN Base_UserInfo u ON a.CreateUserID = u.UserID                                
                                LEFT JOIN Base_StoreInfo requestoutstore ON a.RequestOutStoreID = requestoutstore.StoreID
                                LEFT JOIN Base_StoreInfo requestinstore ON a.RequestInStoreID = requestinstore.StoreID
                                LEFT JOIN Base_DepotInfo requestoutdepot ON a.RequestOutDepotID = requestoutdepot.ID
                                LEFT JOIN Base_DepotInfo requestindepot ON a.RequestInDepotID = requestindepot.ID 
                                WHERE a.ID = " + requestId;
                var data_GoodRequest = Data_GoodRequestService.I.SqlQuery<Data_GoodRequestList>(sql).FirstOrDefault();

                var linq = new
                {
                    data_GoodRequest = data_GoodRequest,
                    GoodRequestDetails = list
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 调拨退货
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodRequestExit(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                if (!dicParas.Get("requestId").Validintnozero("调拨单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("inDepotId").Validintnozero("入库仓库ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitCount").Validintnozero("退货总数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitCost").Validdecimal("退货杂费", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitTotal").Validdecimalnozero("实退总额", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("logistType").Validint("物流类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("logistOrderId").Nonempty("物流单号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                
                if (!dicParas.GetArray("goodRequestDetails").Validarray("调拨明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var requestId = dicParas.Get("requestId").Toint();
                var inDepotId = dicParas.Get("inDepotId").Toint();
                var exitCount = dicParas.Get("exitCount").Toint();
                var exitCost = dicParas.Get("exitCost").Todecimal();
                var exitTotal = dicParas.Get("exitTotal").Todecimal();
                var logistType = dicParas.Get("logistType").Toint();
                var logistOrderId = dicParas.Get("logistOrderId");
                var note = dicParas.Get("note");
                var goodRequestDetails = dicParas.GetArray("goodRequestDetails");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
                        {
                            errMsg = "该调拨单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var requestCode = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).Select(o => o.RequestCode).FirstOrDefault();

                        //工作流更新
                        var wf = new GoodReqWorkFlow(requestId ?? 0, userId, logType, storeId);
                        if (!wf.RequestExit(out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                        //添加退货单
                        var exitModel = new Data_GoodExitInfo();
                        exitModel.DepotID = inDepotId;
                        exitModel.ExitCost = exitCost;
                        exitModel.ExitCount = exitCount;
                        exitModel.ExitOrderID = RedisCacheHelper.CreateCloudSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);
                        exitModel.ExitTotal = exitTotal;
                        exitModel.Note = note;
                        exitModel.MerchID = merchId;
                        exitModel.StoreID = storeId;
                        exitModel.ExitTime = DateTime.Now;
                        exitModel.CheckDate = getCurrCheckDate(merchId, storeId);
                        exitModel.UserID = userId;
                        exitModel.SourceType = (int)GoodExitSourceType.GoodRequest;
                        exitModel.SourceOrderID = requestCode;
                        exitModel.LogistType = logistType;
                        exitModel.LogistOrderID = logistOrderId;
                        if (!Data_GoodExitInfoService.I.Add(exitModel))
                        {
                            errMsg = "保存退货信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //添加退货明细
                        if (goodRequestDetails != null && goodRequestDetails.Count() >= 0)
                        {
                            //获取已退货信息
                            var exitedList = from a in Data_GoodExitInfoService.N.GetModels(p => p.SourceType == (int)GoodExitSourceType.GoodRequest && p.SourceOrderID.Equals(requestCode, StringComparison.OrdinalIgnoreCase))
                                             join b in Data_GoodExit_DetailService.N.GetModels() on a.ExitOrderID equals b.ExitOrderID
                                             group b by b.GoodID into g
                                             select new { GoodID = g.Key, ExitedCount = g.Sum(s => s.ExitCount) };
                            //获取入库信息
                            var storageList = Data_GoodRequest_ListService.I.GetModels(p => p.RequestID == requestId && p.StorageCount > 0).Select(o => new { GoodID = o.GoodID, StorageCount = o.StorageCount });

                            foreach (IDictionary<string, object> el in goodRequestDetails)
                            {
                                if (el != null)
                                {
                                    //获取调拨明细
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("id").Validintnozero("调拨明细ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("exitCount").Validintnozero("退货数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var id = dicPara.Get("id").Toint();
                                    var goodExitCount = dicPara.Get("exitCount").Toint();                                    
                                    var data_GoodRequest_List = Data_GoodRequest_ListService.I.GetModels(p => p.ID == id).FirstOrDefault();
                                    var goodId = data_GoodRequest_List.GoodID;
                                    var exitedCount = exitedList.Where(w => w.GoodID == goodId).Select(o => o.ExitedCount).FirstOrDefault() ?? 0;
                                    var storageCount = storageList.Where(w => w.GoodID == goodId).Select(o => o.StorageCount).FirstOrDefault() ?? 0;
                                    var leftCount = storageCount - exitedCount;
                                    if (leftCount < goodExitCount)
                                    {
                                        errMsg = "退货数量不能超过入库数量";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var detailModel = new Data_GoodExit_Detail();
                                    detailModel.ExitCount = goodExitCount;
                                    detailModel.ExitOrderID = exitModel.ExitOrderID;
                                    detailModel.DepotID = exitModel.DepotID;
                                    detailModel.ExitPrice = data_GoodRequest_List.CostPrice;
                                    detailModel.GoodID = goodId;
                                    detailModel.MerchID = merchId;
                                    Data_GoodExit_DetailService.I.AddModel(detailModel);

                                    //更新当前库存
                                    if (!XCCloudBLLExt.UpdateGoodsStock(detailModel.DepotID, detailModel.GoodID, (int)SourceType.GoodExit, exitModel.ID, detailModel.ExitPrice, (int)StockFlag.Out, detailModel.ExitCount, merchId, storeId, out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodExit_DetailService.I.SaveChanges())
                            {
                                errMsg = "保存退货明细信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStock_RecordService.I.SaveChanges())
                            {
                                errMsg = "添加出库存异动信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        /// <summary>
        /// 删除调拨单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelGoodRequest(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
                var logType = userTokenKeyModel.LogType;

                var errMsg = string.Empty;
                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodRequestService.I.Any(a => a.ID == requestId))
                        {
                            errMsg = "该调拨单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (Data_GoodStock_RecordService.I.Any(a => a.SourceType == (int)SourceType.GoodRequest && a.SourceID == requestId))
                        {
                            errMsg = "该调拨单存在出入库信息不能删除";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        //工作流更新
                        var wf = new GoodReqWorkFlow(requestId, userId, logType, storeId);
                        if (!wf.Delete(out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                        //删除调拨明细
                        var requestModel = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).FirstOrDefault();
                        Data_GoodRequestService.I.DeleteModel(requestModel);

                        foreach (var detailModel in Data_GoodRequest_ListService.I.GetModels(p => p.RequestID == requestId))
                        {
                            Data_GoodRequest_ListService.I.DeleteModel(detailModel);
                        }

                        if (!Data_GoodRequest_ListService.I.SaveChanges())
                        {
                            errMsg = "更新调拨明细失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GoodRequestService.I.SaveChanges())
                        {
                            errMsg = "更新调拨单失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                                                
                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DownloadGoodRequestTemplate(Dictionary<string, object> dicParas)
        {
            try
            {
                string templateUrl = @"/XCCloud/Report/DownloadTemplate.aspx?templateName=GoodRequestTemplate";

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, HttpUtility.UrlEncode(templateUrl));
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #endregion

        #region 商品入库管理

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodSupplierDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var linq = from a in Data_SupplierListService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                           select new
                           {
                               MerchID = a.MerchID,
                               StoreID = a.StoreID,
                               Supplier = a.Supplier
                           };
                if (!storeId.IsNull())
                    linq = linq.Where(w => w.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));

                var Suppliers = linq.Select(o => new { Supplier = o.Supplier }).Distinct().OrderBy(or => or.Supplier);

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, Suppliers);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

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

                string sql = @"SELECT
                                	a.ID,
                                    a.StorageOrderID,
                                    a.DepotID,
                                	/*入库门店*/
                                	(case when IsNull(b.StoreName,'')='' then '总店' else b.StoreName end) AS StoreName,
                                	/*入库时间*/
                                    (case when IsNull(a.RealTime,'')='' then '' else convert(varchar,a.RealTime,20) end) AS RealTime,
                                	/*采购渠道*/
                                	a.Supplier,                                	
                                	/*入库金额*/
                                	c.TotalPrice,
                                	/*入库人*/
                                	u.LogName,
                                	/*入库仓库*/
                                	d.DepotName,
                                	/*状态*/
                                	a.AuthorFlag
                                FROM
                                	Data_GoodStorage a
                                LEFT JOIN Base_StoreInfo b ON a.StoreID = b.StoreID
                                LEFT JOIN (
                                	SELECT
                                		StorageOrderID, SUM(ISNULL(TotalPrice,0)) AS TotalPrice
                                	FROM
                                		Data_GoodStorage_Detail
                                    GROUP BY StorageOrderID
                                ) c ON a.StorageOrderID = c.StorageOrderID
                                LEFT JOIN Base_UserInfo u ON a.UserID = u.UserID
                                LEFT JOIN Base_DepotInfo d ON a.DepotID = d.ID                                
                                WHERE 1 = 1";
                sql = sql + " AND a.merchId='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND a.storeId='" + storeId + "'";
                sql = sql + sqlWhere;

                var data_GoodStorage = Data_GoodStorageService.I.SqlQuery<Data_GoodStorageList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GoodStorage);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Grants = "商品入库")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if(!dicParas.Get("id").Validintnozero("入库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();
                var data_GoodStorage = Data_GoodStorageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if(data_GoodStorage == null)
                {
                    errMsg = "该入库单信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var storageOrderId = data_GoodStorage.StorageOrderID;
                var GoodStorageDetail = from a in Data_GoodStorage_DetailService.N.GetModels(p => p.StorageOrderID.Equals(storageOrderId, StringComparison.OrdinalIgnoreCase))
                                        join b in Base_GoodsInfoService.N.GetModels() on a.GoodID equals b.ID
                                        join c in Dict_SystemService.N.GetModels() on b.GoodType equals c.ID
                                        join d in Data_GoodStorageService.N.GetModels(p => p.ID == id) on a.StorageOrderID equals d.StorageOrderID
                                        join e in
                                            (
                                                from a in Data_GoodsStockService.N.GetModels(p => p.StockType == (int)StockType.Depot)
                                                group a by new { a.StockIndex, a.GoodID } into g
                                                select new
                                                {
                                                    DepotID = g.Key.StockIndex,
                                                    GoodID = g.Key.GoodID,
                                                    RemainCount = g.OrderByDescending(or => or.InitialTime).FirstOrDefault().RemainCount ?? 0
                                                }
                                            ) on new { d.DepotID, a.GoodID } equals new { e.DepotID, e.GoodID }
                                        join f in
                                            (
                                                from a in Data_GoodExitInfoService.N.GetModels(p => p.SourceType == (int)GoodExitSourceType.GoodStorage && p.SourceOrderID.Equals(storageOrderId, StringComparison.OrdinalIgnoreCase))
                                                join b in Data_GoodExit_DetailService.N.GetModels() on a.ExitOrderID equals b.ExitOrderID
                                                group b by b.GoodID into g
                                                select new
                                                {
                                                    GoodID = g.Key,
                                                    ExitedCount = g.Sum(s => s.ExitCount ?? 0)
                                                }
                                            ) on a.GoodID equals f.GoodID into f1
                                        from f in f1.DefaultIfEmpty()
                                        select new
                                        {
                                            GoodID = a.GoodID,
                                            BarCode = b.Barcode,
                                            GoodName = b.GoodName,
                                            GoodTypeStr = c.DictKey,
                                            RemainCount = e.RemainCount,
                                            StorageCount = a.StorageCount,
                                            ExitedCount = f != null ? f.ExitedCount : 0,
                                            Price = a.Price,
                                            Tax = a.Tax,
                                            TaxPrice = a.TaxPrice
                                        };

                var linq = new
                {
                    data_GoodStorage = Data_GoodStorageService.I.GetModels(p => p.ID == id).FirstOrDefault(),
                    GoodStorageDetail = GoodStorageDetail
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }       

        [Authorize(Grants = "商品入库")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.Get("depotId").Validintnozero("入库仓库ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("payable").Validdecimal("应付金额", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("payment").Validdecimal("实付金额", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("discount").Validdecimal("优惠金额", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("supplier").Nonempty("供应商", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if(!dicParas.GetArray("goodStorageDetail").Validarray("入库明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);
                var goodStorageDetail = dicParas.GetArray("goodStorageDetail");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_GoodStorageService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GoodStorage();
                        Utils.GetModel(dicParas, ref model);                        
                        if (id == 0)
                        {
                            model.StorageOrderID = RedisCacheHelper.CreateCloudSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);
                            model.MerchID = merchId;
                            model.StoreID = storeId;
                            model.UserID = logId;
                            model.AuthorFlag = (int)GoodOutInState.Pending;
                            model.RealTime = DateTime.Now;
                            model.CheckDate = getCurrCheckDate(merchId, storeId);
                            if (!Data_GoodStorageService.I.Add(model))
                            {
                                errMsg = "保存商品入库信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该入库单不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (model.AuthorFlag != (int)GoodOutInState.Pending)
                            {
                                errMsg = "已审核的入库单不能修改";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStorageService.I.Update(model))
                            {
                                errMsg = "保存商品入库信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = model.ID;
                        
                        //添加入库明细
                        if (goodStorageDetail != null && goodStorageDetail.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodStorageDetail)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if(!dicPara.Get("goodId").Validintnozero("商品ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("storageCount").Validintnozero("入库数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("price").Validdecimal("不含税单价", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("taxPrice").Validdecimal("含税单价", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("tax").Validdecimal("税率", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var taxPrice = dicPara.Get("taxPrice").Todecimal(0);
                                    var storageCount = dicPara.Get("storageCount").Toint(0);
                                    var detailModel = new Data_GoodStorage_Detail();
                                    Utils.GetModel(dicPara, ref detailModel);
                                    detailModel.StorageOrderID = model.StorageOrderID;
                                    detailModel.DepotID = model.DepotID;
                                    detailModel.MerchID = merchId;   
                                    detailModel.TotalPrice = Math.Round(taxPrice * storageCount, 2, MidpointRounding.AwayFromZero);
                                    Data_GoodStorage_DetailService.I.AddModel(detailModel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodStorage_DetailService.I.SaveChanges())
                            {
                                errMsg = "保存商品入库明细信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        [Authorize(Grants = "商品入库")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("入库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodStorageService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该入库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GoodStorageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        if (model.AuthorFlag != (int)GoodOutInState.Pending)
                        {
                            errMsg = "已审核的入库信息不能删除";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        Data_GoodStorageService.I.DeleteModel(model);

                        var storageOrderId = model.StorageOrderID;
                        foreach (var detailModel in Data_GoodStorage_DetailService.I.GetModels(p => p.StorageOrderID == storageOrderId))
                        {
                            Data_GoodStorage_DetailService.I.DeleteModel(detailModel);
                        }

                        if (!Data_GoodStorageService.I.SaveChanges())
                        {
                            errMsg = "删除入库单失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        [Authorize(Inherit = false, Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CheckGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("入库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodStorageService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该入库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GoodStorageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        if (model.AuthorFlag == (int)GoodOutInState.Done)
                        {
                            errMsg = "已入库审核的记录不能重复审核";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        model.AuthorFlag = (int)GoodOutInState.Done;
                        model.AuthorID = logId;
                        model.RealTime = DateTime.Now;
                        if (!Data_GoodStorageService.I.Update(model))
                        {
                            errMsg = "审核商品入库信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //审核通过入库存记录
                        var storageOrderId = model.StorageOrderID;
                        var detailList = Data_GoodStorage_DetailService.I.GetModels(p => p.StorageOrderID == storageOrderId).ToList();
                        foreach (var detailModel in detailList)
                        {
                            //更新当前库存
                            if (!XCCloudBLLExt.UpdateGoodsStock(model.DepotID, detailModel.GoodID, (int)SourceType.GoodStorage, id, detailModel.TaxPrice, (int)StockFlag.In, detailModel.StorageCount, merchId, storeId, out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                             
                        }
                        
                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加入库存异动信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        /// <summary>
        /// 撤销审核
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Inherit = false, Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CanGoodStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("入库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodStorageService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该入库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        var model = Data_GoodStorageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        var checkDate = model.CheckDate;
                        var modelMerchId = model.MerchID;
                        var modelStoreId = model.StoreID;

                        //判断Store_CheckDate营业日期为已结算状态的, 出入库不能撤销
                        if (Store_CheckDateService.I.Any(a => a.MerchID.Equals(modelMerchId, StringComparison.OrdinalIgnoreCase) && a.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && a.CheckDate == checkDate && a.AuthorID > 0))
                        {
                            errMsg = "营业日期为已结算状态的, 出入库不能撤销";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (model.AuthorFlag != (int)GoodOutInState.Done)
                        {
                            errMsg = "未审核的入库单不能撤销";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        model.AuthorFlag = (int)GoodOutInState.Cancel;
                        model.AuthorID = logId;
                        if (!Data_GoodStorageService.I.Update(model))
                        {
                            errMsg = "撤销商品入库信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //添加入库存撤销记录
                        var recordList = Data_GoodStock_RecordService.I.GetModels(p => p.SourceType == (int)SourceType.GoodStorage && p.SourceID == id).ToList();
                        foreach (var record in recordList)
                        {
                            //更新当前库存
                            if (!XCCloudBLLExt.UpdateGoodsStock(record.DepotID, record.GoodID, (int)SourceType.GoodStorage, id, record.GoodCost, (int)StockFlag.Out, record.StockCount, merchId, storeId, out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                            
                        }

                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加入库存撤销记录失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        [Authorize(Grants = "商品入库")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGoodExit(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.Get("sourceOrderId").Nonempty("入库单号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitCount").Validintnozero("退货总数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitCost").Validdecimal("退货杂费", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitTotal").Validdecimalnozero("实退总额", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("exitDetails").Validarray("退货明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("logistType").Validint("物流类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("logistOrderId").Nonempty("物流单号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var logistType = dicParas.Get("logistType").Toint();
                var logistOrderId = dicParas.Get("logistOrderId");
                var sourceOrderId = dicParas.Get("sourceOrderId");
                var exitCount = dicParas.Get("exitCount").Toint();
                var exitCost = dicParas.Get("exitCost").Todecimal();
                var exitTotal = dicParas.Get("exitTotal").Todecimal();
                var exitDetails = dicParas.GetArray("exitDetails");
                var note = dicParas.Get("note");
                var id = 0;

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodStorageService.I.Any(a => a.StorageOrderID == sourceOrderId))
                        {
                            errMsg = "该入库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GoodStorageService.I.GetModels(p => p.StorageOrderID == sourceOrderId).FirstOrDefault();
                        if (model.AuthorFlag != (int)GoodOutInState.Done)
                        {
                            errMsg = "未审核或已撤销的入库单不能退货";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }                       

                        var exitModel = new Data_GoodExitInfo();
                        exitModel.DepotID = model.DepotID;
                        exitModel.ExitCost = exitCost;
                        exitModel.ExitCount = exitCount;
                        exitModel.ExitOrderID = RedisCacheHelper.CreateCloudSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);
                        exitModel.ExitTotal = exitTotal;
                        exitModel.Note = note;
                        exitModel.MerchID = merchId;
                        exitModel.StoreID = storeId;
                        exitModel.ExitTime = DateTime.Now;
                        exitModel.CheckDate = getCurrCheckDate(merchId, storeId);
                        exitModel.UserID = logId;
                        exitModel.SourceType = (int)GoodExitSourceType.GoodStorage;
                        exitModel.SourceOrderID = sourceOrderId;
                        exitModel.LogistType = logistType;
                        exitModel.LogistOrderID = logistOrderId;
                        if (!Data_GoodExitInfoService.I.Add(exitModel))
                        {
                            errMsg = "保存退货信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        id = exitModel.ID;
                        
                        //保存退货明细信息
                        if (exitDetails != null && exitDetails.Count() >= 0)
                        {
                            //获取已退货信息
                            var exitedList = from a in Data_GoodExitInfoService.N.GetModels(p => p.SourceType == (int)GoodExitSourceType.GoodStorage && p.SourceOrderID.Equals(sourceOrderId, StringComparison.OrdinalIgnoreCase))
                                             join b in Data_GoodExit_DetailService.N.GetModels() on a.ExitOrderID equals b.ExitOrderID                                             
                                             group b by b.GoodID into g
                                             select new { GoodID = g.Key, ExitedCount = g.Sum(s => s.ExitCount) };
                            //获取入库信息
                            var storageList = Data_GoodStorage_DetailService.I.GetModels(p => p.StorageOrderID.Equals(sourceOrderId, StringComparison.OrdinalIgnoreCase)).Select(o => new { GoodID = o.GoodID, StorageCount = o.StorageCount });

                            foreach (IDictionary<string, object> el in exitDetails)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("goodId").Validintnozero("商品ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("exitCount").Validintnozero("退货数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("exitPrice").Validdecimalnozero("退货单价", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var goodId = dicPara.Get("goodId").Toint();
                                    var goodExitCount = dicPara.Get("exitCount").Toint() ?? 0;
                                    var exitPrice = dicPara.Get("exitPrice").Todecimal();
                                    var exitedCount = exitedList.Where(w => w.GoodID == goodId).Select(o => o.ExitedCount).FirstOrDefault() ?? 0;
                                    var storageCount = storageList.Where(w => w.GoodID == goodId).Select(o => o.StorageCount).FirstOrDefault() ?? 0;
                                    var leftCount = storageCount - exitedCount;
                                    if (leftCount < goodExitCount)
                                    {
                                        errMsg = "退货数量不能超过入库数量";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var detailModel = new Data_GoodExit_Detail();
                                    detailModel.ExitCount = goodExitCount;
                                    detailModel.ExitOrderID = exitModel.ExitOrderID;
                                    detailModel.DepotID = exitModel.DepotID;
                                    detailModel.ExitPrice = exitPrice;
                                    detailModel.GoodID = goodId;                                    
                                    detailModel.MerchID = merchId;                                    
                                    Data_GoodExit_DetailService.I.AddModel(detailModel);

                                    //更新当前库存
                                    if (!XCCloudBLLExt.UpdateGoodsStock(detailModel.DepotID, detailModel.GoodID, (int)SourceType.GoodExit, id, detailModel.ExitPrice, (int)StockFlag.Out, detailModel.ExitCount, merchId, storeId, out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                                                                       
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodExit_DetailService.I.SaveChanges())
                            {
                                errMsg = "保存退货明细信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStock_RecordService.I.SaveChanges())
                            {
                                errMsg = "添加出库存异动信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }                        

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        #endregion  
      
        #region 商品库存出库
        
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodOutOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

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

                string sql = @"SELECT
                                	a.ID,
                                    /*出库单号*/
                                    a.OrderID,
                                    /*出库类别*/
                                    a.OrderType,                                	
                                	/*出库时间*/
                                    (case when IsNull(a.CreateTime,'')='' then '' else convert(varchar,a.CreateTime,20) end) AS CreateTime,
                                	/*出库数量*/
                                	b.OutCount,
                                	/*出库金额*/
                                	b.OutTotal,                                	
                                	/*出库人*/
                                	u.LogName,
                                	/*出库仓库*/
                                	c.DepotName,
                                	/*状态*/
                                	a.State,
                                    /*营业日期*/
                                    (case when IsNull(a.CheckDate,'')='' then '' else convert(varchar,a.CheckDate,23) end) AS CheckDate
                                FROM
                                	Data_GoodOutOrder a
                                LEFT JOIN (
                                	SELECT
                                		OrderID, SUM(ISNULL(OutCount,0)) AS OutCount, SUM(ISNULL(OutTotal,0)) AS OutTotal
                                	FROM
                                		Data_GoodOutOrder_Detail   
                                    GROUP BY OrderID                                                                       	
                                ) b ON a.OrderID = b.OrderID
                                LEFT JOIN Base_UserInfo u ON a.OPUserID = u.UserID
                                LEFT JOIN Base_DepotInfo c ON a.DepotID = c.ID                                
                                WHERE 1 = 1";
                sql = sql + " AND a.merchId='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND a.storeId='" + storeId + "'";
                sql = sql + sqlWhere;
                sql = sql + " ORDER BY a.ID DESC";

                //查询退货信息
                string sql2 = @"SELECT
                                	a.ID,
                                    /*退货单号*/
                                    a.ExitOrderID AS OrderID,
                                    /*出库类别*/
                                    3 AS OrderType,
                                	/*退货时间*/
                                    (case when IsNull(a.ExitTime,'')='' then '' else convert(varchar,a.ExitTime,20) end) AS CreateTime,
                                	/*退货数量*/
                                	a.ExitCount AS OutCount,
                                	/*实退总额*/
                                	a.ExitTotal AS OutTotal,                                	
                                	/*操作人*/
                                	u.LogName,
                                	/*出库仓库*/
                                	c.DepotName,
                                	/*状态*/
                                	1 AS State,
                                    /*营业日期*/
                                    (case when IsNull(a.CheckDate,'')='' then '' else convert(varchar,a.CheckDate,23) end) AS CheckDate
                                FROM
                                	Data_GoodExitInfo a                                
                                LEFT JOIN Base_UserInfo u ON a.UserID = u.UserID
                                LEFT JOIN Base_DepotInfo c ON a.DepotID = c.ID                                
                                WHERE 1 = 1";
                sql2 = sql2 + " AND a.merchId='" + merchId + "'";
                if (!storeId.IsNull())
                    sql2 = sql2 + " AND a.storeId='" + storeId + "'";
                sql2 = sql2 + " ORDER BY a.ID DESC";

                var data_GoodOutOrder = Data_GoodOutOrderService.I.SqlQuery<Data_GoodOutOrderList>(sql, parameters)
                    .Union(Data_GoodExitInfoService.I.SqlQuery<Data_GoodOutOrderList>(sql2, parameters)).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GoodOutOrder);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Grants = "商品出库")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodOutOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("出库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var GoodOutOrderDetail = from a in Data_GoodOutOrder_DetailService.N.GetModels()
                                         join b in Base_GoodsInfoService.N.GetModels() on a.GoodID equals b.ID
                                         join c in Dict_SystemService.N.GetModels() on b.GoodType equals c.ID
                                         join d in Data_GoodOutOrderService.N.GetModels(p => p.ID == id) on a.OrderID equals d.OrderID
                                         join e in Data_GoodsStockService.N.GetModels(p => p.StockType == (int)StockType.Depot) on new { DepotID = d.DepotID, a.GoodID } equals new { DepotID = e.StockIndex, e.GoodID }
                                         select new
                                         {
                                             BarCode = b.Barcode,
                                             GoodName = b.GoodName,
                                             GoodTypeStr = c.DictKey,
                                             RemainCount = e.RemainCount ?? 0,
                                             OutCount = a.OutCount,
                                             InitialAvgValue = e.InitialAvgValue ?? 0,
                                             OutTotal = a.OutTotal
                                         };

                var linq = new
                {
                    data_GoodOutOrder = Data_GoodOutOrderService.I.GetModels(p => p.ID == id).FirstOrDefault(),
                    GoodOutOrderDetail = GoodOutOrderDetail
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Grants = "商品出库")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGoodOutOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.Get("depotId").Validintnozero("出库仓库ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("orderType").Validint("出库类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("goodOutOrderDetail").Validarray("出库明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("logistType").Validint("物流类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("logistOrderId").Nonempty("物流单号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("orderType").Toint() == (int)GoodOutOrderType.Transfer) //转仓出库
                {
                    if (!dicParas.Get("inDepotId").Validintnozero("入库仓库ID", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                    if (dicParas.Get("inDepotId").Toint() == dicParas.Get("depotId").Toint())
                    {
                        errMsg = "转仓时出库仓库不能与入库仓库相同";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                    
                var id = dicParas.Get("id").Toint(0);
                var goodOutOrderDetail = dicParas.GetArray("goodOutOrderDetail");
                var depotId = dicParas.Get("depotId").Toint();
                var orderType = dicParas.Get("orderType").Toint();
                var inDepotId = dicParas.Get("inDepotId").Toint();
                var storageOrderId = string.Empty; //转仓出库时创建入库单号
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var checkDate = getCurrCheckDate(merchId, storeId);
                        var model = Data_GoodOutOrderService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GoodOutOrder();
                        Utils.GetModel(dicParas, ref model);
                        if (id == 0)
                        {
                            model.OrderID = RedisCacheHelper.CreateCloudSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);                            
                            model.MerchID = merchId;
                            model.StoreID = storeId;
                            model.OPUserID = logId;
                            model.State = (int)GoodOutInState.Pending; 
                            if (orderType == (int)GoodOutOrderType.Transfer)
                            {
                                storageOrderId = RedisCacheHelper.CreateCloudSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);
                                model.State = (int)GoodOutInState.Done; //转仓出库无需审核
                                model.Note = "转仓出库 入库单号：" + storageOrderId + Environment.NewLine + model.Note;
                            }

                            if (orderType == (int)GoodOutOrderType.Discard || orderType == (int)GoodOutOrderType.Exit)
                            {
                                model.State = (int)GoodOutInState.Done; //废品或退货出库无需审核
                            }
                            
                            model.CreateTime = DateTime.Now;
                            model.CheckDate = checkDate;
                            if (!Data_GoodOutOrderService.I.Add(model))
                            {
                                errMsg = "保存商品出库信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该出库单不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (model.State != (int)GoodOutInState.Pending)
                            {
                                errMsg = "已审核的出库单不能修改";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodOutOrderService.I.Update(model))
                            {
                                errMsg = "保存商品出库信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = model.ID;

                        //添加出库明细
                        if (goodOutOrderDetail != null && goodOutOrderDetail.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodOutOrderDetail)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("goodId").Validintnozero("商品ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("outCount").Validintnozero("出库数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("outPrice").Validdecimal("出库成本金额", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("outTotal").Validdecimal("出库金额", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var goodId = dicPara.Get("goodId").Toint();

                                    //期初平均成本大于0才能出库
                                    if (!Data_GoodsStockService.I.Any(a => a.StockType == (int)StockType.Depot && a.StockIndex == depotId && a.GoodID == goodId && a.InitialAvgValue > 0))
                                    {
                                        errMsg = "该商品库存不符合出库条件, 期初平均成本须大于0";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var detailModel = new Data_GoodOutOrder_Detail();
                                    Utils.GetModel(dicPara, ref detailModel);
                                    detailModel.OrderID = model.OrderID;
                                    detailModel.DepotID = model.DepotID;
                                    detailModel.MerchID = merchId;
                                    detailModel.StoreID = storeId;
                                    Data_GoodOutOrder_DetailService.I.AddModel(detailModel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodOutOrder_DetailService.I.SaveChanges())
                            {
                                errMsg = "保存商品出库明细信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //转仓出库修改库存信息
                        if (orderType == (int)GoodOutOrderType.Transfer) //转仓出库
                        {
                            //更新出库存记录
                            var orderId = model.OrderID;
                            var detailList = Data_GoodOutOrder_DetailService.I.GetModels(p => p.OrderID == orderId).ToList();
                            foreach (var detailModel in detailList)
                            {
                                //更新当前库存
                                if (!XCCloudBLLExt.UpdateGoodsStock(model.DepotID, detailModel.GoodID, (int)SourceType.GoodOut, id, detailModel.OutPrice, (int)StockFlag.Out, detailModel.OutCount, merchId, storeId, out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStock_RecordService.I.SaveChanges())
                            {
                                errMsg = "添加出库存异动信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //创建入库单
                            var storageModel = new Data_GoodStorage();
                            storageModel.DepotID = inDepotId;
                            storageModel.StorageOrderID = storageOrderId;
                            storageModel.MerchID = merchId;
                            storageModel.StoreID = storeId;
                            storageModel.UserID = logId;                            
                            storageModel.AuthorFlag = (int)GoodOutInState.Done;
                            storageModel.CheckDate = checkDate;
                            storageModel.RealTime = DateTime.Now;
                            storageModel.Payable = 0;
                            storageModel.Payment = 0;
                            storageModel.Discount = 0;
                            storageModel.Note = "转仓入库 出库单号：" + model.OrderID;
                            if (!Data_GoodStorageService.I.Add(storageModel))
                            {
                                errMsg = "保存商品入库信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //更新入库存记录
                            foreach (var detailModel in detailList)
                            {
                                var storageDetailModel = new Data_GoodStorage_Detail();
                                storageDetailModel.StorageOrderID = storageOrderId;
                                storageDetailModel.DepotID = inDepotId;
                                storageDetailModel.MerchID = merchId;
                                storageDetailModel.Tax = 0;
                                storageDetailModel.GoodID = detailModel.GoodID;                                
                                storageDetailModel.TaxPrice = detailModel.OutPrice;
                                storageDetailModel.Price = detailModel.OutPrice;
                                storageDetailModel.StorageCount = detailModel.OutCount;
                                storageDetailModel.TotalPrice = detailModel.OutTotal;
                                Data_GoodStorage_DetailService.I.AddModel(storageDetailModel);

                                //更新当前库存
                                if (!XCCloudBLLExt.UpdateGoodsStock(inDepotId, detailModel.GoodID, (int)SourceType.GoodStorage, storageModel.ID, detailModel.OutPrice, (int)StockFlag.In, detailModel.OutCount, merchId, storeId, out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStorage_DetailService.I.SaveChanges())
                            {
                                errMsg = "保存商品入库明细信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GoodStock_RecordService.I.SaveChanges())
                            {
                                errMsg = "添加入库存异动信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        [Authorize(Grants = "商品出库")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelGoodOutOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("出库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodOutOrderService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该出库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GoodOutOrderService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        if (model.State != (int)GoodOutInState.Pending)
                        {
                            errMsg = "已审核的出库信息不能删除";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        Data_GoodOutOrderService.I.DeleteModel(model);

                        var orderId = model.OrderID;
                        foreach (var detailModel in Data_GoodOutOrder_DetailService.I.GetModels(p => p.OrderID == orderId))
                        {
                            Data_GoodOutOrder_DetailService.I.DeleteModel(detailModel);
                        }

                        if (!Data_GoodOutOrderService.I.SaveChanges())
                        {
                            errMsg = "删除入库单失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        [Authorize(Inherit = false, Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CheckGoodOutOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("出库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodOutOrderService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该出库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GoodOutOrderService.I.GetModels(p => p.ID == id).FirstOrDefault();

                        //废品、转仓、退货出库无需审核
                        if (model.OrderType == (int)GoodOutOrderType.Discard || model.OrderType == (int)GoodOutOrderType.Transfer || model.OrderType == (int)GoodOutOrderType.Exit)
                        {
                            errMsg = "该类型出库单无需审核";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (model.State == (int)GoodOutInState.Done)
                        {
                            errMsg = "已出库审核的记录不能重复审核";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        model.State = (int)GoodOutInState.Done;
                        model.AuthorID = logId;
                        model.AuthorTime = DateTime.Now;
                        if (!Data_GoodOutOrderService.I.Update(model))
                        {
                            errMsg = "审核商品出库信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //更新出库存记录
                        var orderId = model.OrderID;
                        var detailList = Data_GoodOutOrder_DetailService.I.GetModels(p => p.OrderID == orderId).ToList();
                        foreach (var detailModel in detailList)
                        {
                            //更新当前库存
                            if (!XCCloudBLLExt.UpdateGoodsStock(model.DepotID, detailModel.GoodID, (int)SourceType.GoodOut, id, detailModel.OutPrice, (int)StockFlag.Out, detailModel.OutCount, merchId, storeId, out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                               
                        }

                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加出库存异动信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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
        
        /// <summary>
        /// 撤销出库
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Inherit = false, Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CanGoodOutOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("出库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GoodOutOrderService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该出库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                       
                        var model = Data_GoodOutOrderService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        var checkDate = model.CheckDate;
                        var modelMerchId = model.MerchID;
                        var modelStoreId = model.StoreID;

                        //废品、转仓、退货出库无需审核, 不能撤销
                        if (model.OrderType == (int)GoodOutOrderType.Discard || model.OrderType == (int)GoodOutOrderType.Transfer || model.OrderType == (int)GoodOutOrderType.Exit)
                        {
                            errMsg = "该类型出库单不能撤销";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //判断Store_CheckDate营业日期为已结算状态的, 出入库不能撤销
                        if (Store_CheckDateService.I.Any(a => a.MerchID.Equals(modelMerchId, StringComparison.OrdinalIgnoreCase) && a.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && a.CheckDate == checkDate && a.AuthorID > 0))
                        {
                            errMsg = "营业日期为已结算状态的, 出入库不能撤销";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        if (model.State != (int)GoodOutInState.Done)
                        {
                            errMsg = "未审核的出库单不能撤销";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                       
                        model.State = (int)GoodOutInState.Cancel;
                        model.CancelUserID = logId;
                        model.CancelTime = DateTime.Now;
                        if (!Data_GoodOutOrderService.I.Update(model))
                        {
                            errMsg = "审核商品出库信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //添加出库存撤销记录
                        var recordList = Data_GoodStock_RecordService.I.GetModels(p => p.SourceType == (int)SourceType.GoodOut && p.SourceID == id).ToList();
                        foreach (var record in recordList)
                        {
                            //更新当前库存
                            if (!XCCloudBLLExt.UpdateGoodsStock(record.DepotID, record.GoodID, (int)SourceType.GoodOut, id, record.GoodCost, (int)StockFlag.In, record.StockCount, merchId, storeId, out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                            
                        }

                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加出库存撤销记录失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        #endregion

        #region 商品库存管理

        /// <summary>
        /// 商品库存列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodsStock(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("depotId").Validintnozero("仓库ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var depotId = dicParas.Get("depotId").Toint();
                var notGoodIds = dicParas.Get("notGoodIds");
                var goodId = dicParas.Get("goodId").Toint();
                var goodNameOrBarCode = dicParas.Get("goodNameOrBarCode");
               
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);


                #region Sql语句
                string sql = @"SELECT
                                	a.ID,
                                    a.GoodID,
                                	/*商品条码*/
                                	b.Barcode,
                                	/*商品名称*/
                                	b.GoodName,                                	
                                	/*商品类别*/
                                	b.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	c.DictKey AS GoodTypeStr,
                                	/*门店ID*/
                                	a.StoreID,
                                	/*门店名称*/
                                	d.StoreName,                                	
                                    ISNULL(a.MinValue,0) AS MinValue,
                                    ISNULL(a.MaxValue,0) AS MaxValue,
                                    (case when ISNULL(a.InitialTime,'')='' then '' else convert(varchar,a.InitialTime,20) end) AS InitialTime,
                                    ISNULL(a.InitialValue,0) AS InitialValue,
                                    ISNULL(a.InitialAvgValue,0) AS InitialAvgValue,
                                    ISNULL(a.RemainCount,0) AS RemainCount,
                                    a.Note
                                FROM
                                    (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by StockIndex,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                    WHERE StockType = 0
                                ) a                                                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID                                
                                LEFT JOIN Dict_System c ON b.GoodType = c.ID
                                LEFT JOIN Base_StoreInfo d ON a.StoreID = d.StoreID
                                WHERE
                                	b.Status = 1 AND a.RowNum <= 1 AND a.StockIndex = " + depotId;
                sql = sql + " AND a.MerchID='" + merchId + "' AND b.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND a.StoreID='" + storeId + "'";
                if (!goodId.IsNull())
                    sql = sql + " AND a.GoodID=" + goodId;
                if (!goodNameOrBarCode.IsNull())
                    sql = sql + " AND (b.GoodName like '%" + goodNameOrBarCode + "%' OR b.Barcode like '%" + goodNameOrBarCode + "%')";
                sql = sql + sqlWhere;
                #endregion

                var list = Data_GoodsStockService.I.SqlQuery<Data_GoodsStockList>(sql, parameters).ToList();
                if (!notGoodIds.IsNull())
                {
                    list = list.Where(p => !notGoodIds.Contains(p.GoodID + "")).ToList();
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 商品库存列表（供商品调拨使用）
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodsStock2(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                string errMsg = string.Empty;
                var depotId = dicParas.Get("depotId").Toint();
                var storeId = dicParas.Get("storeId");
                var merchId = dicParas.Get("merchId");

                if (depotId.IsNull() && storeId.IsNull() && merchId.IsNull())
                {
                    errMsg = "仓库ID、商户ID或门店ID不能同时为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var notGoodIds = dicParas.Get("notGoodIds");
                var goodId = dicParas.Get("goodId").Toint();
                var goodNameOrBarCode = dicParas.Get("goodNameOrBarCode");
                
                #region Sql语句
                string sql = @"SELECT
                                	a.ID,
                                    a.GoodID,
                                    a.StockIndex AS DepotID,
                                	/*商品条码*/
                                	b.Barcode,
                                	/*商品名称*/
                                	b.GoodName,                                	
                                	/*商品类别*/
                                	b.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	c.DictKey AS GoodTypeStr,
                                	/*门店ID*/
                                	a.StoreID,
                                	/*门店名称*/
                                	d.StoreName,                                	
                                    ISNULL(a.MinValue,0) AS MinValue,
                                    ISNULL(a.MaxValue,0) AS MaxValue,
                                    (case when ISNULL(a.InitialTime,'')='' then '' else convert(varchar,a.InitialTime,20) end) AS InitialTime,
                                    ISNULL(a.InitialValue,0) AS InitialValue,
                                    ISNULL(a.InitialAvgValue,0) AS InitialAvgValue,
                                    ISNULL(a.RemainCount,0) AS RemainCount,
                                    a.Note
                                FROM
                                    (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by StockIndex,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                    WHERE
                                        StockType = 0 AND InitialAvgValue > 0                                                                 /*未盘点或者期初成本金额为0的库存商品不能出库*/
                                ) a                                                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID                                
                                LEFT JOIN Dict_System c ON b.GoodType = c.ID
                                LEFT JOIN Base_StoreInfo d ON a.StoreID = d.StoreID
                                WHERE
                                	b.Status = 1 AND ISNULL(b.StoreID,'')='' AND a.RowNum <= 1 ";    //不能调拨专属商品
                if (!depotId.IsNull())
                    sql += " AND a.StockIndex=" + depotId;
                if (!merchId.IsNull())
                    sql += " AND a.MerchID='" + merchId + "' AND b.MerchID='" + merchId + "' AND ISNULL(a.StoreID,'')=''";  //门店向总店申请或总店向门店派货，仅查总店库存
                if (!storeId.IsNull())
                    sql += " AND a.StoreID='" + storeId + "'";
                if (!goodId.IsNull())
                    sql += sql + " AND a.GoodID=" + goodId;
                if (!goodNameOrBarCode.IsNull())
                    sql += sql + " AND (b.GoodName like '%" + goodNameOrBarCode + "%' OR b.Barcode like '%" + goodNameOrBarCode + "%')";

                #endregion

                var list = Data_GoodsStockService.I.SqlQuery<Data_GoodsStockList>(sql).ToList();
                if (!notGoodIds.IsNull())
                {
                    list = list.Where(p => !notGoodIds.Contains(p.GoodID + "")).ToList();
                }

                list = list.GroupBy(g => g.GoodID).Select(o => new Data_GoodsStockList
                {
                    GoodID = o.Key,
                    AvailableCount = o.Sum(s => s.AvailableCount),
                    RemainCount = o.Sum(s => s.RemainCount),
                    Barcode = o.FirstOrDefault().Barcode,
                    GoodType = o.FirstOrDefault().GoodType,
                    GoodTypeStr = o.FirstOrDefault().GoodTypeStr,
                    GoodName = o.FirstOrDefault().GoodName,
                    StoreName = o.FirstOrDefault().StoreName,
                    Source = o.FirstOrDefault().Source
                }).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 商品库存列表（供商品出库使用）
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodsStock3(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                string errMsg = string.Empty;
                if (!dicParas.Get("depotId").Validintnozero("仓库ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var depotId = dicParas.Get("depotId").Toint();                
                var notGoodIds = dicParas.Get("notGoodIds");
                var goodId = dicParas.Get("goodId").Toint();
                var goodNameOrBarCode = dicParas.Get("goodNameOrBarCode");

                #region Sql语句
                string sql = @"SELECT
                                	a.ID,
                                    a.GoodID,
                                    a.StockIndex AS DepotID,
                                	/*商品条码*/
                                	b.Barcode,
                                	/*商品名称*/
                                	b.GoodName,                                	
                                	/*商品类别*/
                                	b.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	c.DictKey AS GoodTypeStr,
                                	/*门店ID*/
                                	a.StoreID,
                                	/*门店名称*/
                                	d.StoreName,                                	
                                    ISNULL(a.MinValue,0) AS MinValue,
                                    ISNULL(a.MaxValue,0) AS MaxValue,
                                    (case when ISNULL(a.InitialTime,'')='' then '' else convert(varchar,a.InitialTime,20) end) AS InitialTime,
                                    ISNULL(a.InitialValue,0) AS InitialValue,
                                    ISNULL(a.InitialAvgValue,0) AS InitialAvgValue,
                                    ISNULL(a.RemainCount,0) AS RemainCount,
                                    a.Note
                                FROM
                                    (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by StockIndex,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                    WHERE
                                        StockType = 0 AND InitialAvgValue > 0                                                                 /*未盘点或者期初成本金额为0的库存商品不能出库*/
                                ) a                                                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID                                
                                LEFT JOIN Dict_System c ON b.GoodType = c.ID
                                LEFT JOIN Base_StoreInfo d ON a.StoreID = d.StoreID
                                WHERE
                                	b.Status = 1 AND a.RowNum <= 1 AND a.StockIndex = " + depotId;                
                if (!goodId.IsNull())
                    sql += sql + " AND a.GoodID=" + goodId;
                if (!goodNameOrBarCode.IsNull())
                    sql += sql + " AND (b.GoodName like '%" + goodNameOrBarCode + "%' OR b.Barcode like '%" + goodNameOrBarCode + "%')";

                #endregion

                var list = Data_GoodsStockService.I.SqlQuery<Data_GoodsStockList>(sql).ToList();
                if (!notGoodIds.IsNull())
                {
                    list = list.Where(p => !notGoodIds.Contains(p.GoodID + "")).ToList();
                }                

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object ExportGoodsUnderWarning(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("depotId").Validintnozero("库存ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var depotId = dicParas.Get("depotId").Toint();
                var notGoodIds = dicParas.Get("notGoodIds");
                var goodId = dicParas.Get("goodId").Toint();
                var goodNameOrBarCode = dicParas.Get("goodNameOrBarCode");
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
                                	b.Barcode,
                                	/*商品名称*/
                                	b.GoodName,                                	
                                	/*商品类别*/
                                	b.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	c.DictKey AS GoodTypeStr,
                                	/*门店ID*/
                                	a.StoreID,
                                	/*门店名称*/
                                	d.StoreName,                                	
                                    a.MinValue,
                                    a.MaxValue,                                    
                                    (case when ISNULL(a.InitialTime,'')='' then '' else convert(varchar,a.InitialTime,20) end) AS InitialTime,
                                    a.InitialValue,
                                    a.InitialAvgValue,
                                    a.RemainCount,
                                    a.Note
                                FROM
                                    (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by StockIndex,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                    WHERE 
                                        StockType = 0
                                ) a                                                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID                                
                                LEFT JOIN Dict_System c ON b.GoodType = c.ID
                                LEFT JOIN Base_StoreInfo d ON a.StoreID = d.StoreID
                                WHERE
                                	b.Status = 1 AND a.RowNum <= 1 AND a.StockIndex = " + depotId;
                sql = sql + " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND a.StoreID='" + storeId + "'";
                if (!goodId.IsNull())
                    sql = sql + " AND a.GoodID=" + goodId;
                if (!goodNameOrBarCode.IsNull())
                    sql = sql + " AND (b.GoodName like '%" + goodNameOrBarCode + "%' OR b.Barcode like '%" + goodNameOrBarCode + "%')";
                sql = sql + sqlWhere;
                #endregion

                var list = Data_GoodsStockService.I.SqlQuery<Data_GoodsStockList>(sql, parameters).Where(w => w.AvailableCount <= 0).ToList();
                if (!notGoodIds.IsNull())
                {
                    list = list.Where(p => !notGoodIds.Contains(p.GoodID + "")).ToList();
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("序号");
                dt.Columns.Add("商品条码");
                dt.Columns.Add("商品类别");
                dt.Columns.Add("商品名称");
                dt.Columns.Add("添加来源");
                dt.Columns.Add("库存下限");
                dt.Columns.Add("库存上限");
                dt.Columns.Add("期初时间");
                dt.Columns.Add("期初库存");
                dt.Columns.Add("期初平均成本（含税）");
                dt.Columns.Add("期初库存金额（含税）");
                dt.Columns.Add("当前库存");
                dt.Columns.Add("当前库存金额（含税）");
                foreach (var item in list)
                {
                    var dr = dt.NewRow();
                    dr["序号"] = item.ID;
                    dr["商品条码"] = item.Barcode;
                    dr["商品类别"] = item.GoodTypeStr;
                    dr["商品名称"] = item.GoodName;
                    dr["添加来源"] = item.Source;
                    dr["库存下限"] = item.MinValue + "";
                    dr["库存上限"] = item.MaxValue + "";
                    dr["期初时间"] = item.InitialTime;
                    dr["期初库存"] = item.InitialValue + "";
                    dr["期初平均成本（含税）"] = item.InitialAvgValue + "";
                    dr["期初库存金额（含税）"] = item.InitialTotal + "";
                    dr["当前库存"] = item.RemainCount + "";
                    dr["当前库存金额（含税）"] = item.RemainTotal + "";
                    dt.Rows.Add(dr);
                }

                string filePath = Utils.ExportToExcel(dt);
                string guid = Guid.NewGuid().ToString();
                CacheHelper.Insert(guid, filePath, 5 * 60, onRemoveCallback);
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, guid);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 过期文件移除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="reason"></param>
        private void onRemoveCallback(string key, object value, System.Web.Caching.CacheItemRemovedReason reason)
        {
            if (value != null)
            {
                string filePath = value.ToString();
                if (!Utils.DeleteFile(filePath))
                    LogHelper.SaveLog("错误:过期文件删除失败，文件路径：" + filePath);
            }
        }
     
        #endregion

        #region 商品库存盘点
        
        /// <summary>
        /// 获取库存索引字典
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object GetStockIndexDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string storeId = userTokenModel.StoreId;
                string merchId = storeId.Substring(0, 6);
                var logId = userTokenModel.UserId;

                string errMsg = string.Empty;
                if (!dicParas.Get("stockType").Validint("库存类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var stockType = dicParas.Get("stockType").Toint();

                switch (stockType)
                {
                    case (int)StockType.Depot:
                        {
                            var linq = from a in Base_DepotInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "").Equals(storeId, StringComparison.OrdinalIgnoreCase))
                                       select new
                                       {
                                           StockIndex = a.ID,
                                           StockName = a.DepotName,
                                           Source = 1
                                       };

                            var err = string.Empty;
                            if (CheckUserGrant("总部盘点", logId, out err))
                            {
                                linq = linq.Union(from a in Base_DepotInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "") == "")
                                                  select new
                                                  {
                                                      StockIndex = a.ID,
                                                      StockName = a.DepotName,
                                                      Source = 0
                                                  });
                            }

                            return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
                        }
                    case (int)StockType.WorkStation:
                        {
                            var linq = from a in Data_WorkstationService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "").Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.StationType == 0)                                      
                                       select new
                                       {
                                           StockIndex = a.ID,
                                           StockName = a.WorkStation
                                       };

                            return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
                        }
                    case (int)StockType.SelfService:
                        {
                            var linq = from a in Data_WorkstationService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "").Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.StationType == 1)
                                       select new
                                       {
                                           StockIndex = a.ID,
                                           StockName = a.WorkStation
                                       };

                            return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
                        }
                    case (int)StockType.GameInfo:
                        {
                            var linq = from a in Data_GameInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreID ?? "").Equals(storeId, StringComparison.OrdinalIgnoreCase))
                                       select new
                                       {
                                           StockIndex = a.ID,
                                           StockName = a.GameName
                                       };

                            return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
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
        /// 获取盘点信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object GetGoodsInventory(Dictionary<string, object> dicParas)
        {
            try
            {
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string storeId = userTokenModel.StoreId;

                string errMsg = string.Empty;
                if (!dicParas.Get("stockType").Validint("库存类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("stockIndex").Validintnozero("库存索引", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("barCode").Nonempty("商品条码", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var stockType = dicParas.Get("stockType").Toint();
                var stockIndex = dicParas.Get("stockIndex").Toint();
                var barCode = dicParas.Get("barCode");

                #region Sql语句
                string sql = @"SELECT
                                    a.GoodID,
                                	ISNULL(b.ID, 0) AS ID,
                                	/*商品条码*/
                                	c.Barcode,
                                	/*商品名称*/
                                	c.GoodName,   
                                    /*商品来源*/
                                    ISNULL(s.StoreName, '总店') AS Source,                             	
                                	/*商品类别*/
                                	c.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	d.DictKey AS GoodTypeStr,                             	
                                    a.MinValue,
                                    a.MaxValue,                                            
                                    (case when ISNULL(a.InitialTime,'')='' then '' else convert(varchar,a.InitialTime,20) end) AS InitialTime,
                                    ISNULL(a.InitialValue,0) AS InitialValue,
                                    ISNULL(a.InitialAvgValue,0) AS InitialAvgValue,
                                    ISNULL(a.RemainCount,0) AS RemainCount,
                                    u.LogName AS UserName,
                                    (case when ISNULL(b.InventoryTime,'')='' then '' else convert(varchar,b.InventoryTime,20) end) AS InventoryTime,
                                    b.InventoryCount
                                FROM (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by StockType,StockIndex,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                ) a                                
                                LEFT JOIN Data_GoodInventory b ON a.GoodID = b.GoodID AND a.StockType = b.InventoryType AND a.StockIndex = b.InventoryIndex
                                INNER JOIN Base_GoodsInfo c ON a.GoodID = c.ID  
                                LEFT JOIN Dict_System d ON c.GoodType = d.ID
                                LEFT JOIN Base_UserInfo u ON b.UserID = u.UserID
                                LEFT JOIN Base_StoreInfo s ON c.StoreID = s.StoreID
                                WHERE
                                	c.Status = 1 AND a.RowNum <= 1 AND ISNULL(b.AuthorID,'')='' AND a.StockIndex = " + stockIndex + " AND a.StockType = " + stockType + " AND c.BarCode = '" + barCode + "'";    //查询未盘点审核的记录

                #endregion

                var data_GoodInventory = Data_GoodInventoryService.I.SqlQuery<Data_GoodInventoryList>(sql).FirstOrDefault();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GoodInventory);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Grants = "库存盘点")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object SaveGoodsInventory(Dictionary<string, object> dicParas)
        {
            try
            {
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string storeId = userTokenModel.StoreId;
                string merchId = storeId.Substring(0, 6);
                var logId = userTokenModel.UserId;

                string errMsg = string.Empty;
                if (!dicParas.GetArray("goodsInventoryList").Validarray("盘点列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var goodsInventoryList = dicParas.GetArray("goodsInventoryList");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {            
                        var data_GoodInventoryService = Data_GoodInventoryService.I;
                        var data_GoodsStockService = Data_GoodsStockService.I;
                        var base_DepotInfoService = Base_DepotInfoService.I;
 
                        if (goodsInventoryList != null && goodsInventoryList.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodsInventoryList)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("goodId").Validintnozero("商品ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("stockType").Validint("库存类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("stockIndex").Validintnozero("库存索引", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("inventoryCount").Validint("盘点数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    
                                    var goodId = dicPara.Get("goodId").Toint();
                                    var stockType = dicPara.Get("stockType").Toint();
                                    var stockIndex = dicPara.Get("stockIndex").Toint();
                                    var inventoryCount = dicPara.Get("inventoryCount").Toint();

                                    if (!data_GoodsStockService.Any(p => p.StockType == stockType && p.StockIndex == stockIndex && p.GoodID == goodId))
                                    {
                                        errMsg = "该商品库存信息不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    if (data_GoodInventoryService.Any(p => p.InventoryType == stockType && p.InventoryIndex == stockIndex && p.GoodID == goodId && (p.AuthorID ?? 0) == 0))
                                    {
                                        errMsg = "该商品存在未审核的盘点记录, 不能盘点";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    if (stockType == (int)StockType.Depot && !base_DepotInfoService.Any(p => p.ID == stockIndex))
                                    {
                                        errMsg = "该商品仓库信息不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var data_GoodsStock = data_GoodsStockService.GetModels(p => p.StockType == stockType && p.StockIndex == stockIndex && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();

                                    var base_DepotInfo = stockType == (int)StockType.Depot ? base_DepotInfoService.GetModels(p => p.ID == stockIndex).FirstOrDefault() : null;

                                    var data_GoodInventory = new Data_GoodInventory();                                    
                                    data_GoodInventory.PredictCount = data_GoodsStock.RemainCount;
                                    data_GoodInventory.TotalPrice = Math.Round((data_GoodsStock.RemainCount * data_GoodsStock.InitialAvgValue) ?? 0, 2, MidpointRounding.AwayFromZero);
                                    data_GoodInventory.InventoryCount = inventoryCount;
                                    data_GoodInventory.UserID = logId;
                                    data_GoodInventory.InventoryTime = DateTime.Now;
                                    data_GoodInventory.InventoryType = stockType;
                                    data_GoodInventory.InventoryIndex = stockIndex;
                                    data_GoodInventory.GoodID = goodId;
                                    data_GoodInventory.MerchID = merchId;
                                    data_GoodInventory.StoreID = base_DepotInfo != null ? base_DepotInfo.StoreID : storeId;
                                    data_GoodInventoryService.AddModel(data_GoodInventory);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!data_GoodInventoryService.SaveChanges())
                            {
                                errMsg = "保存商品盘点信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        /// <summary>
        /// 查询盘点审核
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodsInventory(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("stockType").Validint("库存类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("stockIndex").Validintnozero("库存索引", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var stockType = dicParas.Get("stockType").Toint();
                var stockIndex = dicParas.Get("stockIndex").Toint();
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);


                #region Sql语句
                string sql = @"SELECT
                                    a.GoodID,
                                	a.ID,
                                	/*商品条码*/
                                	c.Barcode,
                                	/*商品名称*/
                                	c.GoodName,   
                                    /*商品来源*/
                                    ISNULL(s.StoreName, '总店') AS Source,                             	
                                	/*商品类别*/
                                	c.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	d.DictKey AS GoodTypeStr,                             	
                                    b.MinValue,
                                    b.MaxValue,                                            
                                    (case when ISNULL(b.InitialTime,'')='' then '' else convert(varchar,b.InitialTime,20) end) AS InitialTime,
                                    ISNULL(b.InitialValue,0) AS InitialValue,
                                    ISNULL(b.InitialAvgValue,0) AS InitialAvgValue,
                                    ISNULL(b.RemainCount,0) AS RemainCount,
                                    u.LogName AS UserName,
                                    (case when ISNULL(a.InventoryTime,'')='' then '' else convert(varchar,a.InventoryTime,20) end) AS InventoryTime,
                                    a.InventoryCount
                                FROM Data_GoodInventory a                                
                                LEFT JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by StockType,StockIndex,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                ) b ON a.GoodID = b.GoodID AND a.InventoryType = b.StockType AND a.InventoryIndex = b.StockIndex 
                                INNER JOIN Base_GoodsInfo c ON a.GoodID = c.ID  
                                LEFT JOIN Dict_System d ON c.GoodType = d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID = u.UserID
                                LEFT JOIN Base_StoreInfo s ON c.StoreID = s.StoreID
                                WHERE
                                	c.Status = 1 AND b.RowNum <= 1 AND ISNULL(a.AuthorID,'')='' AND a.InventoryIndex = " + stockIndex + " AND a.InventoryType = " + stockType;    //查询未盘点审核的记录
                sql = sql + sqlWhere;

                #endregion

                var list = Data_GoodInventoryService.I.SqlQuery<Data_GoodInventoryList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Grants = "盘点审核")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CheckGoodsInventory(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                var logId = userTokenKeyModel.LogId.Toint();

                string errMsg = string.Empty;
                if (!dicParas.GetArray("goodsInventoryIDs").Validarray("盘点ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var goodsInventoryIDs = dicParas.GetArray("goodsInventoryIDs");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var data_GoodInventoryService = Data_GoodInventoryService.I;
                        var data_GoodsStockService = Data_GoodsStockService.I;

                        if (goodsInventoryIDs != null && goodsInventoryIDs.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodsInventoryIDs)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("id").Validintnozero("盘点ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var id = dicPara.Get("id").Toint();

                                    if (!data_GoodInventoryService.Any(p => p.ID == id))
                                    {
                                        errMsg = "该盘点信息不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var model = data_GoodInventoryService.GetModels(p => p.ID == id).FirstOrDefault();
                                    if (!model.AuthorID.IsNull())
                                    {
                                        errMsg = "已盘点审核的记录不能重复审核";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    
                                    var stockIndex = model.InventoryIndex;
                                    var stockType = model.InventoryType;
                                    var goodId = model.GoodID;
                                    if (!data_GoodsStockService.Any(a => a.StockType == stockType && a.StockIndex == stockIndex && a.GoodID == goodId))
                                    {
                                        errMsg = "该商品库存信息不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }                                    

                                    //获取期初平均成本 
                                    var stockModel = data_GoodsStockService.GetModels(p => p.StockType == stockType && p.StockIndex == stockIndex && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                                    var initialAvgValue = stockModel.InitialAvgValue ?? 0M;
                                    var initialValue = stockModel.InitialValue ?? 0;
                                    var initialTime = stockModel.InitialTime;

                                    switch (stockType)
                                    {
                                        case (int)StockType.Depot:
                                            {
                                                //获取入库数量和金额
                                                var stockInList = from a in Data_GoodStorageService.N.GetModels(p => p.AuthorFlag == (int)GoodOutInState.Done).AsEnumerable().Where(w => w.RealTime >= initialTime)
                                                                  join b in Data_GoodStorage_DetailService.N.GetModels(p => p.DepotID == stockIndex && p.GoodID == goodId) on a.StorageOrderID equals b.StorageOrderID
                                                                  select b;
                                                var stockInCount = stockInList.Sum(s => s.StorageCount) ?? 0;
                                                var stockInTotal = stockInList.Sum(s => s.TotalPrice) ?? 0M;

                                                //获取出库数量和金额
                                                var stockOutList = from a in Data_GoodOutOrderService.N.GetModels(p => p.State == (int)GoodOutInState.Done).AsEnumerable().Where(w => w.AuthorTime >= initialTime)
                                                                   join b in Data_GoodOutOrder_DetailService.N.GetModels(p => p.DepotID == stockIndex && p.GoodID == goodId) on a.OrderID equals b.OrderID
                                                                   select b;
                                                var stockOutCount = stockOutList.Sum(s => s.OutCount) ?? 0;
                                                var stockOutTotal = stockOutList.Sum(s => s.OutTotal) ?? 0M;

                                                //获取退货数量和金额
                                                var stockExitList = from a in Data_GoodExitInfoService.N.GetModels().AsEnumerable().Where(w => w.ExitTime >= initialTime)
                                                                    join b in Data_GoodExit_DetailService.N.GetModels(p => p.DepotID == stockIndex && p.GoodID == goodId) on a.ExitOrderID equals b.ExitOrderID
                                                                    select b;
                                                var stockExitCount = stockExitList.Sum(s => s.ExitCount) ?? 0;
                                                var stockExitTotal = stockExitList.Sum(s => s.ExitCount * s.ExitPrice) ?? 0M;

                                                //获取调拨入库数量和金额
                                                var stockRequestInList = from a in Data_GoodStock_RecordService.N.GetModels(p => p.SourceType == (int)SourceType.GoodRequest && p.DepotID == stockIndex && p.GoodID == goodId && p.StockFlag == (int)StockFlag.In).AsEnumerable().Where(w => w.CreateTime >= initialTime)
                                                                         select a;
                                                var stockRequestInCount = stockRequestInList.Sum(s => s.StockCount) ?? 0;
                                                var stockRequestInTotal = stockRequestInList.Sum(s => s.StockCount * s.GoodCost) ?? 0M;

                                                //获取调拨出库数量和金额
                                                var stockRequestOutList = from a in Data_GoodStock_RecordService.N.GetModels(p => p.SourceType == (int)SourceType.GoodRequest && p.DepotID == stockIndex && p.GoodID == goodId && p.StockFlag == (int)StockFlag.Out).AsEnumerable().Where(w => w.CreateTime >= initialTime)
                                                                          select a;
                                                var stockRequestOutCount = stockRequestOutList.Sum(s => s.StockCount) ?? 0;
                                                var stockRequestOutTotal = stockRequestOutList.Sum(s => s.StockCount * s.GoodCost) ?? 0M;

                                                //计算本期平均成本 
                                                var count = initialValue + stockInCount + stockRequestInCount - stockOutCount - stockExitCount - stockRequestOutCount;
                                                var total = initialAvgValue * initialValue + stockInTotal + stockRequestInTotal - stockOutTotal - stockExitTotal - stockRequestOutTotal;
                                                var avgValue = count > 0 ? Math.Round(total / count, 2, MidpointRounding.AwayFromZero) : 0;
                                                stockModel.InitialTime = DateTime.Now;
                                                stockModel.InitialAvgValue = avgValue;
                                                stockModel.InitialValue = stockModel.RemainCount;
                                                data_GoodsStockService.AddModel(stockModel);
                                                break;
                                            }
                                    }
                                   
                                    //更新盘点信息
                                    model.AuthorID = logId;
                                    model.AuthorTime = DateTime.Now;
                                    data_GoodInventoryService.UpdateModel(model);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!data_GoodsStockService.SaveChanges())
                            {
                                errMsg = "更新库存信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!data_GoodInventoryService.SaveChanges())
                            {
                                errMsg = "盘点审核失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }                                                

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
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

        #endregion
    }
}