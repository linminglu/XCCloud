using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.DBService.BLL;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Common.Extensions;
using System.Transactions;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;
using XCCloudWebBar.Model.XCCloud;

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
                                    a.DepotID, 
                                    b.DepotName,
                                    a.State
                                FROM
                                	Data_Workstation a
                                LEFT JOIN Base_DepotInfo b ON a.DepotID = b.ID                                
                                WHERE 1=1
                            ";
                sql += " AND a.StoreID=" + storeId;

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
                var errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("工作站ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                if (!Data_WorkstationService.I.Any(p => p.ID == id))
                {
                    errMsg = "该工作站不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var list = (from a in Data_WorkstationService.N.GetModels(p => p.ID == id)
                            join b in Data_WorkStation_GoodListService.N.GetModels() on a.ID equals b.WorkStationID
                            join c in Base_GoodsInfoService.N.GetModels() on b.GoodID equals c.ID
                            join d in Dict_SystemService.N.GetModels() on c.GoodType equals d.ID into d1
                            from d in d1.DefaultIfEmpty()
                            select new
                            {
                                SaleName = c.GoodName,
                                SaleType = d != null ? d.DictKey : string.Empty,
                                Source = "单品"
                            }).Union
                           (from a in Data_WorkstationService.N.GetModels(p => p.ID == id)
                            join b in Data_Food_WorkStationService.N.GetModels() on a.ID equals b.WorkStationID
                            join c in Data_FoodInfoService.N.GetModels() on b.FoodID equals c.FoodID
                            join d in Dict_SystemService.N.GetModels() on c.FoodType equals d.ID into d1
                            from d in d1.DefaultIfEmpty()                            
                            select new
                            {
                                SaleName = c.FoodName,
                                SaleType = d != null ? d.DictKey : string.Empty,
                                Source = "套餐"
                            }
                           ).OrderBy(or => or.SaleName);

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
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
                        if (!Data_WorkstationService.I.Update(model))
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
                                    if (!dicPara.Get("depotId").Validintnozero("仓库ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("workStationId").Validintnozero("工作站ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var depotId = dicPara.Get("depotId").Toint();
                                    var workStationId = dicPara.Get("workStationId").Toint();
                                    if (!Data_WorkstationService.I.Any(p => p.ID == workStationId))
                                    {
                                        errMsg = "该工作站不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    if (!Base_DepotInfoService.I.Any(p => p.ID == depotId))
                                    {
                                        errMsg = "该仓库不存在";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var workStation = Data_WorkstationService.I.GetModels(p => p.ID == workStationId).FirstOrDefault();
                                    workStation.DepotID = depotId;
                                    Data_WorkstationService.I.UpdateModel(workStation);
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