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
                if (!storeId.IsNull())
                {
                    sql += " AND a.StoreID='" + storeId + "'";
                }
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
                    base_GoodsInfo = Base_GoodsInfoService.I.GetModels(p => p.ID == id).FirstOrDefault(),
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
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

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
                                        errMsg = "同一余额类别，回购价不能大于兑换价";
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
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

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
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);
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
                                		*, ROW_NUMBER() over(partition by EventID order by CreateTime) as RowNum
                                	FROM
                                		Data_WorkFlow_Entry                                                         
                                	WHERE
                                		State = 4 /*调拨出库*/
                                    AND EventType = 0 /*产品调拨对应表格 Data_GoodRequest*/
                                ) b ON a.ID = b.EventID and b.RowNum <= 1
                                LEFT JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by EventID order by CreateTime) as RowNum
                                	FROM
                                		Data_WorkFlow_Entry                                                         
                                	WHERE
                                		State = 7 /*调拨入库*/
                                    AND EventType = 0 /*产品调拨对应表格 Data_GoodRequest*/                                                                            
                                ) c ON a.ID = c.EventID and c.RowNum <= 1
                                LEFT JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by EventID order by CreateTime) as RowNum
                                	FROM
                                		Data_WorkFlow_Entry                                                         
                                	WHERE
                                		EventType = 0 /*产品调拨对应表格 Data_GoodRequest*/
                                ) d ON a.ID = d.EventID and b.RowNum <= 1
                                WHERE 1=1
                            ";
                sql += " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql += " AND a.CreateStoreID='" + storeId + "' or a.RequestOutStoreID='" + storeId + "' or a.RequestInStoreID='" + storeId + "')";

                #endregion

                var list = Data_GoodRequestService.I.SqlQuery<Data_GoodRequestList>(sql, parameters).ToList();
                foreach (var model in list)
                {
                    model.PermittedTriggers = new GoodReqWorkFlow(model.ID, userId).PermittedTriggers.Cast<int>();
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 查询商品库存
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodsStock(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("inOrOutDepotId").Validint("申请或发货仓库ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var inOrOutDepotId = dicParas.Get("inOrOutDepotId").Toint();
                var notGoodIds = dicParas.Get("notGoodIds");
                var goodId = dicParas.Get("goodId").Toint();

                #region Sql语句
                string sql = @"SELECT
                                    /*商品ID*/
                                	a.GoodID,
                                    /*商品名称*/
                                	c.GoodName,
                                	/*库存*/
                                	ISNULL(b.RemainCount,0) AS RemainCount,
                                    /*可调拨数*/
                                    (ISNULL(b.RemainCount,0) - ISNULL(b.MinValue,0)) AS AvailableCount
                                FROM
                                	Data_GoodsStock a                                
                                INNER JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by DepotID,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock                                                                                         	
                                ) b ON a.ID = b.ID and b.RowNum <= 1 
                                INNER JOIN Base_GoodsInfo c ON b.GoodID = c.ID 
                                WHERE c.AllowStorage = 1 AND c.Status = 1 AND a.DepotID = " + inOrOutDepotId;
                if (!goodId.IsNull())
                    sql += sql + " AND a.GoodID=" + goodId;
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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                
                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var logistType = dicParas.Get("logistType");
                var logistOrderId = dicParas.Get("logistOrderId");

                if (!Data_GoodRequestService.I.Any(p => p.ID == requestId))
                {
                    errMsg = "该调拨单信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var requstType = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).FirstOrDefault().RequstType;
                
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
                                    /*含税价格*/
                                    a.CostPrice,
                                    /*税率*/
                                    a.Tax,
                                	/*库存*/
                                	ISNULL(b.RemainCount,0) AS RemainCount,
                                    /*可调拨数*/
                                    (ISNULL(b.RemainCount,0) - ISNULL(b.MinValue,0)) AS AvailableCount,
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
                                INNER JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by DepotID,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock                                                                                         	
                                ) b ON a."
                                + (requstType == (int)RequestType.MerchSend ? "OutDepotID" : "InDeportID")
                                + @" = b.DepotID AND a.GoodID = b.GoodID and b.RowNum <= 1                                
                                INNER JOIN Base_GoodsInfo c ON b.GoodID = c.ID 
                                INNER JOIN (
                                	SELECT
                                		b.*
                                	FROM
                                		Dict_System a
                                	INNER JOIN Dict_System b ON a.ID = b.PID
                                	WHERE
                                		a.DictKey = '商品类别'
                                	AND a.PID = 0
                                ) d ON CONVERT (VARCHAR, c.GoodType) = d.DictValue
                                LEFT JOIN (
                                	SELECT
                                		b.*
                                	FROM
                                		Dict_System a
                                	INNER JOIN Dict_System b ON a.ID = b.PID
                                	WHERE
                                		a.DictKey = '快递物流'
                                	AND a.PID = 0
                                ) e ON CONVERT (VARCHAR, a.LogistType) = e.DictValue
                                WHERE c.Status = 1
                            ";
                sql += " AND a.RequestID=" + requestId;
                if (!logistType.IsNull())
                {
                    sql += " AND a.LogistType=" + logistType;
                }
                if (!logistOrderId.IsNull())
                {
                    sql += " AND a.LogistOrderId='" + logistOrderId + "'";
                }
                
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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

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
                                INNER JOIN (
                                	SELECT
                                		b.*
                                	FROM
                                		Dict_System a
                                	INNER JOIN Dict_System b ON a.ID = b.PID
                                	WHERE
                                		a.DictKey = '商品类别'
                                	AND a.PID = 0
                                ) c ON CONVERT (VARCHAR, b.GoodType) = c.DictValue                                
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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

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
                                INNER JOIN (
                                	SELECT
                                		b.*
                                	FROM
                                		Dict_System a
                                	INNER JOIN Dict_System b ON a.ID = b.PID
                                	WHERE
                                		a.DictKey = '商品类别'
                                	AND a.PID = 0
                                ) c ON CONVERT (VARCHAR, b.GoodType) = c.DictValue                                
                                WHERE b.Status = 1
                                ORDER BY a.SendTime
                            ";
                sql += " AND a.RequestID=" + requestId;

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
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);

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
                }
                if (requstType == 0 || requstType == 1 || requstType == 3)
                {
                    if (!dicParas.Get("inDepotId").Validint("入库仓库ID", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var outStoreId = dicParas.Get("outStoreId").IsNull() ? storeId : dicParas.Get("outStoreId");
                var outDepotId = dicParas.Get("outDepotId").Toint();
                var inStoreId = dicParas.Get("inStoreId").IsNull() ? storeId : dicParas.Get("inStoreId");
                var inDepotId = dicParas.Get("inDepotId").Toint();
                var requestReason = dicParas.Get("requestReason");
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
                        data_GoodRequest.RequestCode = RedisCacheHelper.CreateSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);
                        data_GoodRequest.RequestReason = requestReason;
                        data_GoodRequest.RequestInStoreID = inStoreId;
                        data_GoodRequest.RequestInDepotID = inDepotId;
                        data_GoodRequest.RequestOutStoreID = outStoreId;
                        data_GoodRequest.RequestOutDepotID = outDepotId;
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
                                    Data_GoodRequest_ListService.I.AddModel(data_GoodRequest_List);
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
                        }

                        //工作流更新
                        var wf = new GoodReqWorkFlow(requestId, userId);
                        if (!wf.Request(out errMsg))
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
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);

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
                        var wf = new GoodReqWorkFlow(requestId, userId);
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

        /// <summary>
        /// 调拨出库审核
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodSendDealVerify(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);

                var errMsg = string.Empty;
                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!dicParas.Get("state").Validint("调拨出库审核状态", out errMsg))
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
                        var wf = new GoodReqWorkFlow(requestId, userId);
                        if (!wf.SendDealVerify(state, note, out errMsg))
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

        /// <summary>
        /// 调拨入库审核
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GoodRequestDealVerify(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);

                var errMsg = string.Empty;
                var requestId = dicParas.Get("requestId").Toint(0);
                if (requestId == 0)
                {
                    errMsg = "调拨单ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!dicParas.Get("state").Validint("调拨入库审核状态", out errMsg))
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
                        var wf = new GoodReqWorkFlow(requestId, userId);
                        if (!wf.RequestDealVerify(state, note, out errMsg))
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
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);

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
                        var wf = new GoodReqWorkFlow(requestId, userId);
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

                                    var id = 0;
                                    bool isAdd = false;
                                    if (Data_GoodRequest_ListService.I.GetCount(p => p.RequestID == requestId && p.GoodID == goodId) == 1)
                                    {
                                        var data_GoodRequest_List = Data_GoodRequest_ListService.I.GetModels(p => p.RequestID == requestId && p.GoodID == goodId).FirstOrDefault();
                                        if (data_GoodRequest_List.SendCount == 0)
                                        {
                                            id = data_GoodRequest_List.ID;
                                            data_GoodRequest_List.RequestID = requestId;
                                            data_GoodRequest_List.GoodID = goodId;
                                            data_GoodRequest_List.OutDepotID = outDepotId;
                                            data_GoodRequest_List.SendCount = sendCount;
                                            data_GoodRequest_List.StorageCount = 0;
                                            data_GoodRequest_List.CostPrice = costPrice;
                                            data_GoodRequest_List.Tax = tax;
                                            data_GoodRequest_List.LogistType = logistType;
                                            data_GoodRequest_List.LogistOrderID = logistOrderId;
                                            data_GoodRequest_List.SendTime = DateTime.Now;
                                            if (!Data_GoodRequest_ListService.I.Update(data_GoodRequest_List))
                                            {
                                                errMsg = "更新调拨明细信息失败";
                                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                            }
                                        }
                                        else
                                        {
                                            isAdd = true;
                                        }
                                    }
                                    else
                                        isAdd = true;

                                    if (isAdd)
                                    {
                                        var data_GoodRequest_List = new Data_GoodRequest_List();
                                        data_GoodRequest_List.RequestID = requestId;
                                        data_GoodRequest_List.GoodID = goodId;
                                        data_GoodRequest_List.OutDepotID = outDepotId;
                                        data_GoodRequest_List.SendCount = sendCount;
                                        data_GoodRequest_List.StorageCount = 0;
                                        data_GoodRequest_List.CostPrice = costPrice;
                                        data_GoodRequest_List.Tax = tax;
                                        data_GoodRequest_List.LogistType = logistType;
                                        data_GoodRequest_List.LogistOrderID = logistOrderId;
                                        data_GoodRequest_List.SendTime = DateTime.Now;
                                        if (!Data_GoodRequest_ListService.I.Add(data_GoodRequest_List))
                                        {
                                            errMsg = "更新调拨明细信息失败";
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        }

                                        id = data_GoodRequest_List.ID;
                                    }
                                    
                                    var data_GoodRequest = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).FirstOrDefault();
                                    data_GoodRequest.RequestOutDepotID = outDepotId;
                                    if (!Data_GoodRequestService.I.Update(data_GoodRequest))
                                    {
                                        errMsg = "更新调拨信息失败";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                                                    
                                    //添加出库存异动信息
                                    var data_GoodStock_Record = new Data_GoodStock_Record();
                                    data_GoodStock_Record.DepotID = outDepotId;
                                    data_GoodStock_Record.GoodID = goodId;
                                    data_GoodStock_Record.SourceType = (int)SourceType.GoodRequest;
                                    data_GoodStock_Record.SourceID = id;
                                    data_GoodStock_Record.StockFlag = (int)StockFlag.Out;
                                    data_GoodStock_Record.StockCount = sendCount;
                                    data_GoodStock_Record.CreateTime = DateTime.Now;
                                    if (!Data_GoodStock_RecordService.I.Add(data_GoodStock_Record))
                                    {
                                        errMsg = "添加调拨出库记录失败";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
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
                var storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                var userId = userTokenKeyModel.LogId.Toint(0);

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
                        var wf = new GoodReqWorkFlow(requestId, userId);
                        if (!wf.RequestDeal(out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                        //添加调拨明细
                        if (goodRequestDetails != null && goodRequestDetails.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in goodRequestDetails)
                            {
                                if (el != null)
                                {
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

                                    var data_GoodRequest = Data_GoodRequestService.I.GetModels(p => p.ID == requestId).FirstOrDefault();
                                    data_GoodRequest.RequestInDepotID = inDepotId;
                                    Data_GoodRequestService.I.UpdateModel(data_GoodRequest);

                                    //添加入库存异动信息
                                    var data_GoodStock_Record = new Data_GoodStock_Record();
                                    data_GoodStock_Record.DepotID = inDepotId;
                                    data_GoodStock_Record.GoodID = data_GoodRequest_List.GoodID;
                                    data_GoodStock_Record.SourceType = (int)SourceType.GoodRequest;
                                    data_GoodStock_Record.SourceID = id;
                                    data_GoodStock_Record.StockFlag = (int)StockFlag.In;
                                    data_GoodStock_Record.StockCount = storageCount;
                                    data_GoodStock_Record.CreateTime = DateTime.Now;
                                    Data_GoodStock_RecordService.I.AddModel(data_GoodStock_Record);
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