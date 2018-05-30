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
                sql += " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql += " AND a.StoreID='" + storeId + "'";

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
                                    (case when ISNULL(requestoutstore.StoreName,'')='' then '总店' else requestoutstore.StoreName end) AS OutStoreName,
                                    /*调拨出库仓库*/
                                    requestoutdepot.ID AS OutDepotID,
                                	requestoutdepot.DepotName AS OutDepotName,                                    
                                	/*调拨入库门店*/
                                    requestinstore.StoreID AS InStoreID,
                                	(case when ISNULL(requestinstore.StoreName,'')='' then '总店' else requestinstore.StoreName end) AS InStoreName,
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
                sql += " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql += " AND (a.CreateStoreID='" + storeId + "' or a.RequestOutStoreID='" + storeId + "' or a.RequestInStoreID='" + storeId + "')";

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
                                INNER JOIN Dict_System d ON c.GoodType = d.ID
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
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
                        data_GoodRequest.RequestCode = RedisCacheHelper.CreateCloudSerialNo(storeId.IsNull() ? merchId.ToExtStoreID() : storeId);
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
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

                                    bool isAdd = false;
                                    if (Data_GoodRequest_ListService.I.GetCount(p => p.RequestID == requestId && p.GoodID == goodId) == 1)
                                    {
                                        var data_GoodRequest_List = Data_GoodRequest_ListService.I.GetModels(p => p.RequestID == requestId && p.GoodID == goodId).FirstOrDefault();
                                        if (data_GoodRequest_List.SendCount == 0)
                                        {
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
                                    data_GoodStock_Record.SourceID = requestId;
                                    data_GoodStock_Record.GoodCost = costPrice;
                                    data_GoodStock_Record.StockFlag = (int)StockFlag.Out;
                                    data_GoodStock_Record.StockCount = sendCount;
                                    data_GoodStock_Record.CreateTime = DateTime.Now;
                                    if (!Data_GoodStock_RecordService.I.Add(data_GoodStock_Record))
                                    {
                                        errMsg = "添加调拨出库记录失败";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    //更新当前库存
                                    var stockDepotId = data_GoodStock_Record.DepotID;
                                    var stockGoodId = data_GoodStock_Record.GoodID;
                                    var stockModel = Data_GoodsStockService.I.GetModels(p => p.DepotID == stockDepotId && p.GoodID == stockGoodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                                    stockModel.RemainCount = (stockModel.RemainCount ?? 0) - data_GoodStock_Record.StockCount;
                                    if (!Data_GoodsStockService.I.Update(stockModel))
                                    {
                                        errMsg = "更新当前库存失败";
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
                var storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                var merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
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
                                    data_GoodStock_Record.SourceID = requestId;
                                    data_GoodStock_Record.GoodCost = data_GoodRequest_List.CostPrice;
                                    data_GoodStock_Record.StockFlag = (int)StockFlag.In;
                                    data_GoodStock_Record.StockCount = storageCount;
                                    data_GoodStock_Record.CreateTime = DateTime.Now;
                                    Data_GoodStock_RecordService.I.AddModel(data_GoodStock_Record);

                                    //更新当前库存
                                    var stockDepotId = data_GoodStock_Record.DepotID;
                                    var stockGoodId = data_GoodStock_Record.GoodID;
                                    var stockModel = Data_GoodsStockService.I.GetModels(p => p.DepotID == stockDepotId && p.GoodID == stockGoodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                                    stockModel.RemainCount = (stockModel.RemainCount ?? 0) + data_GoodStock_Record.StockCount;
                                    Data_GoodsStockService.I.UpdateModel(stockModel);
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

                            if (!Data_GoodsStockService.I.SaveChanges())
                            {
                                errMsg = "更新当前库存失败";
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
                                	/*入库数量*/
                                	c.StorageCount,
                                	/*采购单价*/
                                	c.TaxPrice,
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
                                		*, ROW_NUMBER() over(partition by StorageID order by ID desc) as RowNum
                                	FROM
                                		Data_GoodStorage_Detail
                                ) c ON a.ID = c.StorageID AND c.RowNum <= 1
                                LEFT JOIN Base_UserInfo u ON a.UserID = u.UserID
                                LEFT JOIN Base_DepotInfo d ON a.DepotID = d.ID                                
                                WHERE 1 = 1";
                sql = sql + " AND a.merchId='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND a.storeId='" + storeId + "'";

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

                var GoodStorageDetail = from a in Data_GoodStorage_DetailService.N.GetModels(p => p.StorageID == id)
                                        join b in Base_GoodsInfoService.N.GetModels() on a.GoodID equals b.ID
                                        join c in Dict_SystemService.N.GetModels() on b.GoodType equals c.ID
                                        join d in Data_GoodStorageService.N.GetModels() on a.StorageID equals d.ID
                                        join e in Data_GoodsStockService.N.GetModels() on new { d.DepotID, a.GoodID } equals new { e.DepotID, e.GoodID }
                                        select new
                                        {
                                            GoodID = a.GoodID,
                                            BarCode = b.Barcode,
                                            GoodName = b.GoodName,
                                            GoodTypeStr = c.DictKey,
                                            RemainCount = e.RemainCount ?? 0,
                                            StorageCount = a.StorageCount,
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
                            model.StorageOrderID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                            model.MerchID = merchId;
                            model.StoreID = storeId;
                            model.UserID = logId;
                            model.AuthorFlag = (int)GoodOutInState.Pending;
                            model.RealTime = DateTime.Now;
                            model.CheckDate = DateTime.Now.Todate();  //应从服务获取当前营业日期
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
                                    detailModel.StorageID = id;
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
                            errMsg = "已审核的单据不能删除";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        Data_GoodStorageService.I.DeleteModel(model);

                        foreach (var detailModel in Data_GoodStorage_DetailService.I.GetModels(p=>p.StorageID == id))
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
                        model.AuthorFlag = (int)GoodOutInState.Done;
                        model.AuthorID = logId;

                        if (!Data_GoodStorageService.I.Update(model))
                        {
                            errMsg = "审核商品入库信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //审核通过入库存记录
                        var detailList = Data_GoodStorage_DetailService.I.GetModels(p => p.StorageID == id).ToList();
                        foreach (var detailModel in detailList)
                        {
                            //添加入库存异动信息
                            var data_GoodStock_Record = new Data_GoodStock_Record();
                            data_GoodStock_Record.DepotID = model.DepotID;
                            data_GoodStock_Record.GoodID = detailModel.GoodID;
                            data_GoodStock_Record.GoodCost = detailModel.TaxPrice;
                            data_GoodStock_Record.SourceType = (int)SourceType.GoodStorage;
                            data_GoodStock_Record.SourceID = id;
                            data_GoodStock_Record.StockFlag = (int)StockFlag.In;
                            data_GoodStock_Record.StockCount = detailModel.StorageCount;
                            data_GoodStock_Record.CreateTime = DateTime.Now;
                            Data_GoodStock_RecordService.I.AddModel(data_GoodStock_Record);

                            //更新当前库存
                            var depotId = model.DepotID;
                            var goodId = detailModel.GoodID;
                            var stockModel = Data_GoodsStockService.I.GetModels(p => p.DepotID == depotId && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                            stockModel.RemainCount = (stockModel.RemainCount ?? 0) + detailModel.StorageCount;
                            Data_GoodsStockService.I.UpdateModel(stockModel);                            
                        }
                        
                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加入库存异动信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GoodsStockService.I.SaveChanges())
                        {
                            errMsg = "更新当前库存失败";
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

                        //判断Flw_CheckDate营业日期为已交班或已结算状态的, 出入库不能撤销

                        var model = Data_GoodStorageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        model.AuthorFlag = (int)GoodOutInState.Cancel;
                        model.AuthorID = logId;
                        if (!Data_GoodStorageService.I.Update(model))
                        {
                            errMsg = "审核商品入库信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //添加入库存撤销记录
                        var recordList = Data_GoodStock_RecordService.I.GetModels(p => p.SourceType == (int)SourceType.GoodStorage && p.SourceID == id).ToList();
                        foreach (var record in recordList)
                        {
                            //添加出库存异动信息
                            record.StockFlag = (int)StockFlag.Out;
                            record.CreateTime = DateTime.Now;
                            Data_GoodStock_RecordService.I.AddModel(record);

                            //更新当前库存
                            var depotId = record.DepotID;
                            var goodId = record.GoodID;
                            var stockModel = Data_GoodsStockService.I.GetModels(p => p.DepotID == depotId && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                            stockModel.RemainCount = (stockModel.RemainCount ?? 0) - record.StockCount;
                            Data_GoodsStockService.I.UpdateModel(stockModel);
                        }

                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加入库存撤销记录失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GoodsStockService.I.SaveChanges())
                        {
                            errMsg = "更新当前库存失败";
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
                if (!dicParas.Get("storageOrderIndex").Validintnozero("入库单ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitCount").Validintnozero("退货总数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitCost").Validdecimal("退货杂费", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("exitTotal").Validdecimalnozero("实退总额", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("exitDetails").Validarray("退货明细", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var storageOrderIndex = dicParas.Get("storageOrderIndex").Toint();
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
                        if (!Data_GoodStorageService.I.Any(a => a.ID == storageOrderIndex))
                        {
                            errMsg = "该入库单不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GoodStorageService.I.GetModels(p => p.ID == storageOrderIndex).FirstOrDefault();
                        if (model.AuthorFlag != 1)
                        {
                            errMsg = "该入库单未审核通过不能退货";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var exitModel = new Data_GoodExitInfo();
                        exitModel.DepotID = model.DepotID;
                        exitModel.ExitCost = exitCost;
                        exitModel.ExitCount = exitCount;
                        exitModel.ExitOrderID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                        exitModel.ExitTotal = exitTotal;
                        exitModel.Note = note;
                        exitModel.MerchID = merchId;
                        exitModel.StoreID = storeId;
                        exitModel.CheckDate = DateTime.Now;
                        exitModel.UserID = logId;
                        if (!Data_GoodExitInfoService.I.Add(exitModel))
                        {
                            errMsg = "保存退货信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        id = exitModel.ID;
                        
                        //保存退货明细信息
                        if (exitDetails != null && exitDetails.Count() >= 0)
                        {
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

                                    var detailModel = new Data_GoodExit_Detail();
                                    detailModel.ExitCount = dicPara.Get("exitCount").Toint();
                                    detailModel.ExitOrderIndex = id;
                                    detailModel.ExitPrice = dicPara.Get("exitPrice").Todecimal();
                                    detailModel.GoodID = dicPara.Get("goodId").Toint();
                                    detailModel.MerchID = merchId;                                    
                                    Data_GoodExit_DetailService.I.AddModel(detailModel);
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
                        }

                        //创建出库单
                        var outModel = new Data_GoodOutOrder();
                        outModel.MerchID = merchId;
                        outModel.StoreID = storeId;
                        outModel.OrderID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                        outModel.OrderType = (int)GoodOutOrderType.Exit;
                        outModel.DepotID = model.DepotID;
                        outModel.CreateTime = DateTime.Now;
                        outModel.OPUserID = logId;
                        outModel.State = (int)GoodOutInState.Pending;
                        if (!Data_GoodOutOrderService.I.Add(outModel))
                        {
                            errMsg = "创建出货单失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //保存出货明细信息
                        foreach (IDictionary<string, object> el in exitDetails)
                        {
                            if (el != null)
                            {
                                var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                
                                var outDetailModel = new Data_GoodOutOrder_Detail();
                                outDetailModel.OutCount = dicPara.Get("exitCount").Toint();
                                outDetailModel.GoodID = dicPara.Get("goodId").Toint();
                                outDetailModel.OrderID = outModel.ID;
                                outDetailModel.MerchID = merchId;
                                outDetailModel.StoreID = storeId;
                                Data_GoodOutOrder_DetailService.I.AddModel(outDetailModel);
                            }
                            else
                            {
                                errMsg = "提交数据包含空对象";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        if (!Data_GoodOutOrder_DetailService.I.SaveChanges())
                        {
                            errMsg = "保存出货明细信息失败";
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
                                		*, ROW_NUMBER() over(partition by OrderID,GoodID order by ID) as RowNum
                                	FROM
                                		Data_GoodOutOrder_Detail                                                                                         	
                                ) b ON a.ID = b.OrderID and b.RowNum <= 1
                                LEFT JOIN Base_UserInfo u ON a.OPUserID = u.UserID
                                LEFT JOIN Base_DepotInfo c ON a.DepotID = c.ID                                
                                WHERE 1 = 1";
                sql = sql + " AND a.merchId='" + merchId + "'";
                if (!storeId.IsNull())
                    sql = sql + " AND a.storeId='" + storeId + "'";

                var data_GoodStorage = Data_GoodStorageService.I.SqlQuery<Data_GoodStorageList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GoodStorage);
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

                var GoodOutOrderDetail = from a in Data_GoodOutOrder_DetailService.N.GetModels(p => p.OrderID == id)
                                        join b in Base_GoodsInfoService.N.GetModels() on a.GoodID equals b.ID
                                        join c in Dict_SystemService.N.GetModels() on b.GoodType equals c.ID
                                        join d in Data_GoodOutOrderService.N.GetModels() on a.OrderID equals d.ID
                                        join e in Data_GoodsStockService.N.GetModels() on new { d.DepotID, a.GoodID } equals new { e.DepotID, e.GoodID }
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

                var id = dicParas.Get("id").Toint(0);
                var goodOutOrderDetail = dicParas.GetArray("goodOutOrderDetail");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_GoodOutOrderService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GoodOutOrder();
                        Utils.GetModel(dicParas, ref model);
                        if (id == 0)
                        {
                            model.OrderID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                            model.MerchID = merchId;
                            model.StoreID = storeId;
                            model.OPUserID = logId;
                            model.State = (int)GoodOutInState.Pending;
                            model.CreateTime = DateTime.Now;
                            model.CheckDate = DateTime.Now;  //应从服务获取当前营业日期
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
                                    
                                    var detailModel = new Data_GoodOutOrder_Detail();
                                    Utils.GetModel(dicPara, ref detailModel);
                                    detailModel.OrderID = id;
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
                            errMsg = "已审核的单据不能删除";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        Data_GoodOutOrderService.I.DeleteModel(model);

                        foreach (var detailModel in Data_GoodOutOrder_DetailService.I.GetModels(p => p.OrderID == id))
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
                        model.State = (int)GoodOutInState.Done;
                        model.AuthorID = logId;
                        model.AuthorTime = DateTime.Now;
                        if (!Data_GoodOutOrderService.I.Update(model))
                        {
                            errMsg = "审核商品出库信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //审核通过出库存记录
                        var detailList = Data_GoodOutOrder_DetailService.I.GetModels(p => p.OrderID == id).ToList();
                        foreach (var detailModel in detailList)
                        {
                            //添加出库存异动信息
                            var data_GoodStock_Record = new Data_GoodStock_Record();
                            data_GoodStock_Record.DepotID = model.DepotID;
                            data_GoodStock_Record.GoodID = detailModel.GoodID;
                            data_GoodStock_Record.SourceType = (int)SourceType.GoodOut;
                            data_GoodStock_Record.SourceID = id;
                            data_GoodStock_Record.GoodCost = detailModel.OutPrice;
                            data_GoodStock_Record.StockFlag = (int)StockFlag.Out;
                            data_GoodStock_Record.StockCount = detailModel.OutCount;
                            data_GoodStock_Record.CreateTime = DateTime.Now;
                            Data_GoodStock_RecordService.I.AddModel(data_GoodStock_Record);

                            //更新当前库存
                            var depotId = model.DepotID;
                            var goodId = detailModel.GoodID;
                            var stockModel = Data_GoodsStockService.I.GetModels(p => p.DepotID == depotId && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                            stockModel.RemainCount = (stockModel.RemainCount ?? 0) - detailModel.OutCount;
                            Data_GoodsStockService.I.UpdateModel(stockModel);
                        }

                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加出库存异动信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GoodsStockService.I.SaveChanges())
                        {
                            errMsg = "更新当前库存失败";
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

                        //判断Flw_CheckDate营业日期为已交班或已结算状态的, 出入库不能撤销

                        var model = Data_GoodOutOrderService.I.GetModels(p => p.ID == id).FirstOrDefault();
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
                            //添加入库存异动信息
                            record.StockFlag = (int)StockFlag.In;
                            record.CreateTime = DateTime.Now;
                            Data_GoodStock_RecordService.I.AddModel(record);

                            //更新当前库存
                            var depotId = record.DepotID;
                            var goodId = record.GoodID;
                            var stockModel = Data_GoodsStockService.I.GetModels(p => p.DepotID == depotId && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                            stockModel.RemainCount = (stockModel.RemainCount ?? 0) + record.StockCount;
                            Data_GoodsStockService.I.UpdateModel(stockModel);
                        }

                        if (!Data_GoodStock_RecordService.I.SaveChanges())
                        {
                            errMsg = "添加出库存撤销记录失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GoodsStockService.I.SaveChanges())
                        {
                            errMsg = "更新当前库存失败";
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
                                    /*可调拨数*/
                                    (ISNULL(a.RemainCount,0) - ISNULL(a.MinValue,0)) AS AvailableCount,
                                    (case when ISNULL(a.InitialTime,'')='' then '' else convert(varchar,a.InitialTime,20) end) AS InitialTime,
                                    ISNULL(a.InitialValue,0) AS InitialValue,
                                    ISNULL(a.InitialAvgValue,0) AS InitialAvgValue,
                                    ISNULL(a.RemainCount,0) AS RemainCount,
                                    a.Note
                                FROM
                                    (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by DepotID,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                ) a                                                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID                                
                                LEFT JOIN Dict_System c ON b.GoodType = c.ID
                                LEFT JOIN Base_StoreInfo d ON a.StoreID = d.StoreID
                                WHERE
                                	b.AllowStorage = 1 AND b.Status = 1 AND a.RowNum <= 1 AND a.DepotID = " + depotId;
                sql += " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql += " AND a.StoreID='" + storeId + "'";
                if (!goodId.IsNull())
                    sql += sql + " AND a.GoodID=" + goodId;
                if (!goodNameOrBarCode.IsNull())
                    sql += sql + " AND (b.GoodName like '%" + goodNameOrBarCode + "%' OR b.Barcode like '%" + goodNameOrBarCode + "%')";

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
                                    /*可调拨数*/
                                    (ISNULL(a.RemainCount,0) - ISNULL(a.MinValue,0)) AS AvailableCount,
                                    (case when ISNULL(a.InitialTime,'')='' then '' else convert(varchar,a.InitialTime,20) end) AS InitialTime,
                                    a.InitialValue,
                                    a.InitialAvgValue,
                                    a.RemainCount,
                                    a.Note
                                FROM
                                    (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by DepotID,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                ) a                                                                
                                INNER JOIN Base_GoodsInfo b ON a.GoodID = b.ID                                
                                LEFT JOIN Dict_System c ON b.GoodType = c.ID
                                LEFT JOIN Base_StoreInfo d ON a.StoreID = d.StoreID
                                WHERE
                                	b.AllowStorage = 1 AND b.Status = 1 AND a.RowNum <= 1 AND a.DepotID = " + depotId;
                sql += " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql += " AND a.StoreID='" + storeId + "'";
                if (!goodId.IsNull())
                    sql += sql + " AND a.GoodID=" + goodId;
                if (!goodNameOrBarCode.IsNull())
                    sql += sql + " AND (b.GoodName like '%" + goodNameOrBarCode + "%' OR b.Barcode like '%" + goodNameOrBarCode + "%')";

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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGoodsInventory(Dictionary<string, object> dicParas)
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
                                	c.Barcode,
                                	/*商品名称*/
                                	c.GoodName,                                	
                                	/*商品类别*/
                                	c.GoodType AS GoodType,
                                	/*商品类别[字符串]*/
                                	d.DictKey AS GoodTypeStr,
                                	/*添加类别*/
                                	a.InventoryType,                                	
                                    b.MinValue,
                                    b.MaxValue,                                    
                                    (case when ISNULL(b.InitialTime,'')='' then '' else convert(varchar,b.InitialTime,20) end) AS InitialTime,
                                    b.InitialAvgValue,
                                    b.RemainCount,
                                    u.UserName,
                                    (case when ISNULL(a.InventoryTime,'')='' then '' else convert(varchar,a.InventoryTime,20) end) AS InventoryTime,
                                    a.InventoryCount,
                                    a.PredictCount,
                                    a.TotalPrice
                                FROM
                                    Data_GoodInventory a                                
                                INNER JOIN (
                                	SELECT
                                		*, ROW_NUMBER() over(partition by DepotID,GoodID order by InitialTime desc) as RowNum
                                	FROM
                                		Data_GoodsStock
                                ) b ON a.GoodID = b.GoodID AND a.InventoryIndex = b.DepotID AND b.RowNum <= 1 
                                INNER JOIN Base_GoodsInfo c ON a.GoodID = c.ID  
                                INNER JOIN Base_UserInfo u ON a.UserID = u.UserID
                                LEFT JOIN Dict_System d ON c.GoodType = d.ID
                                WHERE
                                	c.AllowStorage = 1 AND c.Status = 1 AND ISNULL(a.AuthorID,'')='' AND a.InventoryType = 0 AND a.InventoryIndex = " + depotId;
                sql += " AND a.MerchID='" + merchId + "'";
                if (!storeId.IsNull())
                    sql += " AND a.StoreID='" + storeId + "'";

                #endregion

                var list = Data_GoodInventoryService.I.SqlQuery<Data_GoodInventoryList>(sql, parameters).ToList();
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 盘点审核
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Inherit = false, Roles = "MerchUser")]
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

                                    if (!Data_GoodInventoryService.I.Any(p => p.ID == id))
                                    {
                                        errMsg = "该盘点ID不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var model = Data_GoodInventoryService.I.GetModels(p => p.ID == id).FirstOrDefault();
                                    if (model.InventoryType != (int)GoodInventorySource.Depot)
                                    {
                                        errMsg = "该盘点信息来源不是仓库";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    
                                    var depotId = model.InventoryIndex;
                                    var goodId = model.GoodID;
                                    if(!Data_GoodsStockService.I.Any(a => a.DepotID == depotId && a.GoodID == goodId))
                                    {
                                        errMsg = "该盘点对象当前库存信息不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    //获取期初平均成本 
                                    var stockModel = Data_GoodsStockService.I.GetModels(p => p.DepotID == depotId && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
                                    var initialAvgValue = stockModel.InitialAvgValue ?? 0M;
                                    var initialValue = stockModel.InitialValue ?? 0;
                                    var initialTime = stockModel.InitialTime;

                                    //获取入库数量和金额
                                    var stockInList = Data_GoodStock_RecordService.I.GetModels(p => p.DepotID == depotId && p.GoodID == goodId && p.StockFlag == (int)StockFlag.In).AsEnumerable()
                                        .Where(w => w.CreateTime >= initialTime);
                                    var stockInCount = stockInList.Sum(s => s.StockCount) ?? 0;
                                    var stockInTotal = stockInList.Sum(s => s.StockCount * s.GoodCost) ?? 0M;

                                    //计算本期平均成本                                    
                                    var avgValue = Math.Round((initialAvgValue * initialValue + stockInTotal) / (initialValue + stockInCount), 2, MidpointRounding.AwayFromZero);
                                    stockModel.InitialTime = DateTime.Now;
                                    stockModel.InitialAvgValue = avgValue;
                                    stockModel.InitialValue = initialValue + stockInCount;
                                    stockModel.RemainCount = stockModel.InitialValue;
                                    Data_GoodsStockService.I.AddModel(stockModel);

                                    //更新盘点信息
                                    model.AuthorID = logId;
                                    model.AuthorTime = DateTime.Now;
                                    Data_GoodInventoryService.I.UpdateModel(model);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GoodInventoryService.I.SaveChanges())
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