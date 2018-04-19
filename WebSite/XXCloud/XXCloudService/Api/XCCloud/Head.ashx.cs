using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Head 的摘要说明
    /// </summary>
    public class Head : ApiBase
    {
        IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
        IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
        IData_HeadService data_HeadService = BLLContainer.Resolve<IData_HeadService>(resolveNew: true);

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryHead(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

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

                string sql = @"select a.* from Data_Head a where a.StoreID='" + storeId + "'";
                sql = sql + sqlWhere;
                IData_HeadService data_HeadService = BLLContainer.Resolve<IData_HeadService>();
                var data_Head = data_HeadService.SqlQuery(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_Head);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameHeadInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                string gameIndexId = dicParas.ContainsKey("gameIndexId") ? Convert.ToString(dicParas["gameIndexId"]) : string.Empty;
                int iGameIndexId = 0;
                int.TryParse(gameIndexId, out iGameIndexId);

                IData_HeadService data_HeadService = BLLContainer.Resolve<IData_HeadService>();
                Dictionary<int, string> gameHeadInfo = data_HeadService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.GameIndexID == iGameIndexId).Select(o => new
                {
                    ID = o.ID,
                    HeadName = o.HeadName
                }).Distinct().ToDictionary(d => d.ID, d => d.HeadName);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, gameHeadInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 设备管理列表查询
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryHeadInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                int GameTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("游戏机类型")).FirstOrDefault().ID;
                var data_GameInfo = from a in data_HeadService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                                    join b in data_GameInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.GameIndexID equals b.ID into b1
                                    from b in b1.DefaultIfEmpty()
                                    join c in dict_SystemService.GetModels(p => p.PID == GameTypeId) on b.GameType equals c.DictValue into c1
                                    from c in c1.DefaultIfEmpty()
                                    orderby a.HeadID
                                    select new
                                    {
                                        ID = a.ID,
                                        MCUID = a.MCUID,
                                        HeadName = a.HeadName,
                                        GameTypeStr = c != null ? c.DictKey : string.Empty,
                                        GameName = b != null ? b.GameName : string.Empty,
                                        SiteName = a.SiteName,
                                        HeadAddress = a.HeadAddress,
                                        Lock = a.Lock != null ? (a.Lock == 1 ? "锁定" : "解锁") : "",
                                        State = a.State != null ? (a.State == 1 ? "上线" : "下线") : ""
                                    };


                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GameInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 机台绑定(解绑)
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object BindHeadInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? Convert.ToString(dicParas["id"]) : string.Empty;
                string state = dicParas.ContainsKey("state") ? Convert.ToString(dicParas["state"]) : string.Empty;
                string gameIndexId = dicParas.ContainsKey("gameIndexId") ? Convert.ToString(dicParas["gameIndexId"]) : string.Empty;
                string siteName = dicParas.ContainsKey("siteName") ? Convert.ToString(dicParas["siteName"]) : string.Empty;
                int iId = 0, iState = 0;
                int.TryParse(id, out iId);
                int.TryParse(state, out iState);

                #region 参数验证
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "游戏机设备流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (string.IsNullOrEmpty(state))
                {
                    errMsg = "绑定参数state不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (iState == 1)
                {
                    if (string.IsNullOrEmpty(gameIndexId))
                    {
                        errMsg = "游戏机ID不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                    if (string.IsNullOrEmpty(siteName))
                    {
                        errMsg = "P位编号不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }                
                #endregion

                if (!data_HeadService.Any(p => p.ID == iId))
                {
                    errMsg = "游戏机设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_Head = data_HeadService.GetModels(p => p.ID == iId).FirstOrDefault();
                //绑定
                if (iState == 1)
                {
                    data_Head.GameIndexID = Convert.ToInt32(gameIndexId);
                    data_Head.SiteName = siteName;
                }
                //解绑
                else if (iState == 0)
                {
                    data_Head.GameIndexID = (int?)null;
                }
                else
                {
                    errMsg = "绑定参数state值不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!data_HeadService.Update(data_Head))
                {
                    errMsg = "操作失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }        
        }

        /// <summary>
        /// 锁定（解锁）设备
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object LockHeadInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? Convert.ToString(dicParas["id"]) : string.Empty;
                string state = dicParas.ContainsKey("state") ? Convert.ToString(dicParas["state"]) : string.Empty;                
                int iId = 0, iState = 0;
                int.TryParse(id, out iId);
                int.TryParse(state, out iState);

                #region 参数验证
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "游戏机设备流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (string.IsNullOrEmpty(state))
                {
                    errMsg = "锁定参数state不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                #endregion

                if (!data_HeadService.Any(p => p.ID == iId))
                {
                    errMsg = "游戏机设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_Head = data_HeadService.GetModels(p => p.ID == iId).FirstOrDefault();
                data_Head.Lock = iState;                
                if (!data_HeadService.Update(data_Head))
                {
                    errMsg = "操作失败";
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
        public object GetHeadQr(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? Convert.ToString(dicParas["id"]) : string.Empty;
                int iId = 0;
                int.TryParse(id, out iId);

                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "游戏机设备流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }  

                if (!data_HeadService.Any(p => p.ID == iId))
                {
                    errMsg = "游戏机设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var barCode = data_HeadService.GetModels(p => p.ID == iId).Select(o => o.BarCode).FirstOrDefault();
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, barCode);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            } 
        }
    }
}