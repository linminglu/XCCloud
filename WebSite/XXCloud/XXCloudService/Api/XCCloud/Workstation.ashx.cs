using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Common.Extensions;
using System.Transactions;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Workstation 的摘要说明
    /// </summary>
    public class Workstation : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddWorkStation(Dictionary<string, object> dicParas)
        {
            string dogId = dicParas.Get("dogId").ToString();
            string workStation = dicParas.Get("workStation").ToString();

            XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
            string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
            string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

            IBase_StoreDogListService storeDogService = BLLContainer.Resolve<IBase_StoreDogListService>();
            var storeDogModel = storeDogService.GetModels(p => p.DogID.Equals(dogId)).ToList<Base_StoreDogList>()[0];
            if (storeDogModel == null)
            {
                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "加密狗不存在");
            }

            IData_WorkstationService workstationService = BLLContainer.Resolve<IData_WorkstationService>();
            var wsCount = workstationService.GetModels(p => p.WorkStation.Equals(workStation)).Count();
            if (wsCount > 0)
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            else
            {
                Data_Workstation wsModel = new Data_Workstation();
                wsModel.MerchID = storeDogModel.MerchID;
                wsModel.StoreID = storeDogModel.StoreID;
                wsModel.WorkStation = workStation;
                wsModel.DepotID = 0;
                wsModel.MacAddress = workStation;
                wsModel.DiskID = workStation;
                wsModel.State = 0;
                workstationService.AddModel(wsModel);
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }   
        }


        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryWorkstation(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

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
                                    a.WorkStation,
                                    a.DepotID, 
                                    b.DepotName,
                                    a.State
                                FROM
                                	Data_Workstation a
                                LEFT JOIN Base_DepotInfo b ON a.DepotID = b.ID                                
                                WHERE 1=1
                            ";
                sql = sql + " AND a.StoreID=" + storeId;
                sql = sql + sqlWhere;

                var list = Data_WorkstationService.I.SqlQuery<Data_WorkstationList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetWorkstationDic(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? Convert.ToString(dicParas["storeId"]) : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "storeId参数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var workstationList = Data_WorkstationService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .OrderBy(or => or.WorkStation)
                    .Select(o => new
                    {
                        ID = o.ID,
                        WorkStation = o.WorkStation
                    });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, workstationList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取售卖信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetWorkstation(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("工作站ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();
                var saleName = dicParas.Get("saleName");
                var saleType = dicParas.Get("saleType").Toint();

                if (!Data_WorkstationService.I.Any(p => p.ID == id))
                {
                    errMsg = "该工作站不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var list = (from a in Base_GoodsInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && ((p.StoreID ?? "") == "" || p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) && p.Status == 1)
                            join b in Data_WorkStation_GoodListService.N.GetModels(p => p.WorkStationID == id) on a.ID equals b.GoodID into b1
                            from b in b1.DefaultIfEmpty()
                            select new
                            {
                                SaleID = a.ID,
                                SaleName = a.GoodName,
                                SaleType = 0,
                                SaleTypeStr = "单品",
                                ForeAuthorize = "",
                                Price = a.Price,
                                AllowSale = b != null ? 1 : 0
                            }).Union
                           (from a in Data_FoodInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.FoodState == 1)
                            join b in Data_Food_StoreListService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.ID equals b.FoodID
                            join c in Data_Food_WorkStationService.N.GetModels(p => p.WorkStationID == id) on a.ID equals c.FoodID into c1
                            from c in c1.DefaultIfEmpty()
                            select new
                            {
                                SaleID = a.ID,
                                SaleName = a.FoodName,
                                SaleType = 1,
                                SaleTypeStr = "套餐",
                                ForeAuthorize = a.ForeAuthorize == 1 ? "是" : a.ForeAuthorize == 0 ? "否" : string.Empty,
                                Price = a.ClientPrice,
                                AllowSale = c != null ? 1 : 0
                            }
                           );
                if (!saleName.IsNull())
                    list = list.Where(w => w.SaleName.Contains(saleName));
                if (!saleType.IsNull())
                    list = list.Where(w => w.SaleType == saleType);

                list = list.OrderBy(or => or.SaleType);

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 绑定套餐或单品
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object BindFoodWorkStation(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("工作站ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.GetArray("saleInfos").Validarray("售卖信息列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();
                var saleInfos = dicParas.GetArray("saleInfos");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (saleInfos != null && saleInfos.Count() > 0)
                        {
                            foreach (IDictionary<string, object> el in saleInfos)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("saleId").Validintnozero("售卖项目ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("saleType").Validint("售卖项目类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var saleId = dicPara.Get("saleId").Toint();
                                    var saleType = dicPara.Get("saleType").Toint();
                                    if (saleType == 0)
                                    {
                                        if (!Base_GoodsInfoService.I.Any(p => p.ID == saleId && p.Status == 1))
                                        {
                                            errMsg = "该单品不存在或已停用";
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        }

                                        //先删除，后添加
                                        foreach (var model in Data_WorkStation_GoodListService.I.GetModels(p => p.GoodID == saleId && p.WorkStationID == id))
                                        {
                                            Data_WorkStation_GoodListService.I.DeleteModel(model, true, merchId, merchSecret);
                                        }

                                        var data_WorkStation_GoodListModel = new Data_WorkStation_GoodList();
                                        data_WorkStation_GoodListModel.GoodID = saleId;
                                        data_WorkStation_GoodListModel.WorkStationID = id;
                                        Data_WorkStation_GoodListService.I.AddModel(data_WorkStation_GoodListModel, true, merchId, merchSecret);

                                        if (!Data_WorkStation_GoodListService.I.SaveChanges())
                                        {
                                            errMsg = "绑定单品失败";
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        }
                                    }
                                    else if (saleType == 1)
                                    {
                                        if (!Data_FoodInfoService.I.Any(p => p.ID == saleId && p.FoodState == 1))
                                        {
                                            errMsg = "该套餐不存在或已无效";
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        }

                                        //先删除，后添加
                                        foreach (var model in Data_Food_WorkStationService.I.GetModels(p => p.FoodID == saleId && p.WorkStationID == id))
                                        {
                                            Data_Food_WorkStationService.I.DeleteModel(model, true, merchId, merchSecret);
                                        }

                                        var data_Food_WorkStationModel = new Data_Food_WorkStation();
                                        data_Food_WorkStationModel.FoodID = saleId;
                                        data_Food_WorkStationModel.WorkStationID = id;
                                        Data_Food_WorkStationService.I.AddModel(data_Food_WorkStationModel, true, merchId, merchSecret);

                                        if (!Data_Food_WorkStationService.I.SaveChanges())
                                        {
                                            errMsg = "绑定套餐失败";
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        }
                                    }
                                    else
                                    {
                                        errMsg = "该售卖项目类别不正确";
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
        public object SaveWorkstation(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("工作站ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("state").Validint("工作站状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();
                var state = dicParas.Get("state").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_WorkstationService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该工作站不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_WorkstationService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        model.State = state;
                        if (!Data_WorkstationService.I.Update(model, true, merchId, merchSecret))
                        {
                            errMsg = "修改工作站失败";
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
        /// 批量绑定仓库
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object BindDepotBatch(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                if (!dicParas.GetArray("depotWorkstationList").Validarray("仓库工作站列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var depotWorkstationList = dicParas.GetArray("depotWorkstationList");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (depotWorkstationList != null && depotWorkstationList.Count() > 0)
                        {
                            foreach (IDictionary<string, object> el in depotWorkstationList)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    //if (!dicPara.Get("depotId").Validintnozero("仓库ID", out errMsg))
                                    //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("workStationId").Validintnozero("工作站ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var depotId = dicPara.Get("depotId").Toint(0);
                                    var workStationId = dicPara.Get("workStationId").Toint();
                                    if (!Data_WorkstationService.I.Any(p => p.ID == workStationId))
                                    {
                                        errMsg = "该工作站不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    //if (!Base_DepotInfoService.I.Any(p => p.ID == depotId))
                                    //{
                                    //    errMsg = "该仓库不存在";
                                    //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    //}

                                    var workStation = Data_WorkstationService.I.GetModels(p => p.ID == workStationId).FirstOrDefault();
                                    workStation.DepotID = depotId;
                                    Data_WorkstationService.I.UpdateModel(workStation, true, merchId, merchSecret);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_WorkstationService.I.SaveChanges())
                            {
                                errMsg = "批量绑定仓库失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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