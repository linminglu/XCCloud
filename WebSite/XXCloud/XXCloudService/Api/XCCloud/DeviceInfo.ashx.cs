using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
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
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Gift 的摘要说明
    /// </summary>
    public class DeviceInfo : ApiBase
    {
        IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
        IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
        IBase_DeviceInfoService base_DeviceInfoService = BLLContainer.Resolve<IBase_DeviceInfoService>(resolveNew: true);        

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDeviceHeadDict(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                string gameIndexId = dicParas.ContainsKey("gameIndexId") ? Convert.ToString(dicParas["gameIndexId"]) : string.Empty;
                int iGameIndexId = 0;
                int.TryParse(gameIndexId, out iGameIndexId);

                var query = base_DeviceInfoService.GetModels(p => p.type == (int)DeviceType.卡头 && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(storeId))
                {
                    query = query.Where(w => w.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(gameIndexId))
                {
                    query = query.Where(w => w.GameIndexID == iGameIndexId);
                }

                Dictionary<int, string> gameHeadInfo = query.Select(o => new
                {
                    ID = o.ID,
                    DeviceName = o.DeviceName
                }).Distinct().ToDictionary(d => d.ID, d => d.DeviceName);

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
        public object QueryDeviceInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                int DeviceTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("设备类型")).FirstOrDefault().ID;
                var query = base_DeviceInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                
                var base_DeviceInfo = from a in query
                                      join b in data_GameInfoService.GetModels() on a.GameIndexID equals b.ID into b1
                                      from b in b1.DefaultIfEmpty()
                                      join c in dict_SystemService.GetModels(p => p.PID == DeviceTypeId) on (a.type + "") equals c.DictValue into c1
                                      from c in c1.DefaultIfEmpty()
                                      orderby a.MCUID
                                      select new
                                      {
                                          ID = a.ID,
                                          MCUID = a.MCUID,
                                          DeviceName = a.DeviceName,
                                          DeviceType = a.type,
                                          DeviceTypeStr = c != null ? c.DictKey : string.Empty,
                                          GameIndexID = b.ID,
                                          GameName = b != null ? b.GameName : string.Empty,
                                          SiteName = a.SiteName,
                                          segment = a.segment,
                                          Address = a.Address,
                                          DeviceLock = a.DeviceLock,
                                          DeviceLockStr = a.DeviceLock != null ?
                                                        (a.DeviceLock == 1 ? "锁定" :
                                                        a.DeviceLock == 0 ? "解锁" : string.Empty) : string.Empty,
                                          DeviceStatus = a.DeviceStatus,
                                          DeviceStatusStr = a.DeviceStatus != null ?
                                                        (a.DeviceStatus == 1 ? "正常" :
                                                        a.DeviceStatus == 2 ? "检修" :
                                                        a.DeviceStatus == 0 ? "停用" : string.Empty) : string.Empty
                                      };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, base_DeviceInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取路由器设备
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetRouteDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var query = base_DeviceInfoService.GetModels(p => p.type == (int)DeviceType.路由器 && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                
                var routeDevices = from a in query
                                      select new
                                      {
                                          ID = a.ID,
                                          DeviceName = a.DeviceName
                                      };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, routeDevices);
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
        public object BindDeviceInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? Convert.ToString(dicParas["id"]) : string.Empty;
                string state = dicParas.ContainsKey("state") ? Convert.ToString(dicParas["state"]) : string.Empty;
                string gameIndexId = dicParas.ContainsKey("gameIndexId") ? Convert.ToString(dicParas["gameIndexId"]) : string.Empty;
                string bindDeviceId = dicParas.ContainsKey("bindDeviceId") ? Convert.ToString(dicParas["bindDeviceId"]) : string.Empty;
                string siteName = dicParas.ContainsKey("siteName") ? Convert.ToString(dicParas["siteName"]) : string.Empty;
                int iId = 0, iState = 0, iGameIndexId = 0, iBindDeviceId = 0;
                int.TryParse(id, out iId);
                int.TryParse(state, out iState);
                int.TryParse(gameIndexId, out iGameIndexId);
                int.TryParse(bindDeviceId, out iBindDeviceId);

                #region 参数验证
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "设备流水号不能为空";
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
                        errMsg = "游戏机gameIndexId不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                    if (string.IsNullOrEmpty(bindDeviceId))
                    {
                        errMsg = "路由器bindDeviceId不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                    if (string.IsNullOrEmpty(siteName))
                    {
                        errMsg = "P位编号不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                #endregion

                if (!base_DeviceInfoService.Any(p => p.ID == iId))
                {
                    errMsg = "设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (base_DeviceInfoService.Any(p => p.GameIndexID == iGameIndexId && p.ID != iId && p.SiteName.Equals(siteName, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "游戏机P位被占用";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DeviceInfo = base_DeviceInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                //绑定
                if (iState == 1)
                {
                    data_DeviceInfo.GameIndexID = iGameIndexId;
                    data_DeviceInfo.BindDeviceID = iBindDeviceId;
                    data_DeviceInfo.SiteName = siteName;
                }
                //解绑
                else if (iState == 0)
                {
                    data_DeviceInfo.GameIndexID = (int?)null;
                    data_DeviceInfo.BindDeviceID = (int?)null;
                    data_DeviceInfo.SiteName = string.Empty;
                }
                else
                {
                    errMsg = "绑定参数state值无效";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!base_DeviceInfoService.Update(data_DeviceInfo))
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
        public object LockDeviceInfo(Dictionary<string, object> dicParas)
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
                    errMsg = "设备流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (string.IsNullOrEmpty(state))
                {
                    errMsg = "锁定参数state不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                #endregion

                if (!base_DeviceInfoService.Any(p => p.ID == iId))
                {
                    errMsg = "设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (iState != 0 && iState != 1)
                {
                    errMsg = "锁定参数state值无效";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DeviceInfo = base_DeviceInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                data_DeviceInfo.DeviceLock = iState;
                if (!base_DeviceInfoService.Update(data_DeviceInfo))
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
        /// 获取二维码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDeviceQr(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? Convert.ToString(dicParas["id"]) : string.Empty;
                int iId = 0;
                int.TryParse(id, out iId);

                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "设备流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!base_DeviceInfoService.Any(p => p.ID == iId))
                {
                    errMsg = "设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var barCode = base_DeviceInfoService.GetModels(p => p.ID == iId).Select(o => o.BarCode).FirstOrDefault();
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, barCode);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryReloadGifts(Dictionary<string, object> dicParas)
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

                string sql = @"select a.ID, (case when a.RealTime is null or a.RealTime='' then '' else convert(varchar,a.RealTime,23) end) as RealTime, c.HeadName, d.DictKey as ReloadTypeName, b.LogName, a.ReloadCount, a.Note from Data_Reload a " +
                    " left join Base_UserInfo b on a.UserID=b.UserID " +
                    " left join Data_Head c on a.DeviceID=c.ID " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='安装类别' and a.PID=0) d on convert(varchar, a.ReloadType)=d.DictValue " +
                    " where b.StoreID='" + storeId + "' and deviceType=2 and ReloadType=3 " + 
                    " order by c.HeadName, a.RealTime desc";
                sql = sql + sqlWhere;
                IData_ReloadService data_ReloadService = BLLContainer.Resolve<IData_ReloadService>();
                var data_Reload = data_ReloadService.SqlQuery<Data_ReloadModelList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_Reload);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        //public object ReloadDevice(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
        //        string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
        //        string userId = userTokenKeyModel.LogId;
        //        int iUserId = 0;
        //        int.TryParse(userId, out iUserId);

        //        string errMsg = string.Empty;
        //        string goodId = dicParas.ContainsKey("goodId") ? Convert.ToString(dicParas["goodId"]) : string.Empty;
        //        string deviceId = dicParas.ContainsKey("deviceId") ? Convert.ToString(dicParas["deviceId"]) : string.Empty;
        //        string deviceType = dicParas.ContainsKey("deviceType") ? Convert.ToString(dicParas["deviceType"]) : string.Empty;
        //        string reloadType = dicParas.ContainsKey("reloadType") ? Convert.ToString(dicParas["reloadType"]) : string.Empty;
        //        string reloadCount = dicParas.ContainsKey("reloadCount") ? Convert.ToString(dicParas["reloadCount"]) : string.Empty;
        //        string note = dicParas.ContainsKey("note") ? Convert.ToString(dicParas["note"]) : string.Empty;
        //        int iDeviceId = 0, iGoodId = 0;
        //        int.TryParse(deviceId, out iDeviceId);
        //        int.TryParse(goodId, out iGoodId);

        //        #region 参数验证

        //        if (string.IsNullOrEmpty(goodId))
        //        {
        //            errMsg = "商品索引goodId不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        //if (string.IsNullOrEmpty(deviceId))
        //        //{
        //        //    errMsg = "设备索引deviceId不能为空";
        //        //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        //}

        //        if (string.IsNullOrEmpty(deviceType))
        //        {
        //            errMsg = "设备类型deviceType不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (string.IsNullOrEmpty(reloadType))
        //        {
        //            errMsg = "安装类别reloadType不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (string.IsNullOrEmpty(reloadCount))
        //        {
        //            errMsg = "安装数量reloadCount不能为空";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        if (!Utils.isNumber(reloadCount))
        //        {
        //            errMsg = "安装数量reloadCount格式不正确";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }

        //        int iReloadCount = Convert.ToInt32(reloadCount);
        //        if (iReloadCount < 0)
        //        {
        //            errMsg = "安装数量reloadCount不能为负数";
        //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //        }                

        //        #endregion

        //        IBase_StorageInfoService base_StorageInfoService = BLLContainer.Resolve<IBase_StorageInfoService>();
        //        IData_Storage_RecordService data_Storage_RecordService = BLLContainer.Resolve<IData_Storage_RecordService>();
        //        IData_ReloadService data_ReloadService = BLLContainer.Resolve<IData_ReloadService>();
        //        IBase_DeviceInfoService base_DeviceInfoService = BLLContainer.Resolve<IBase_DeviceInfoService>(resolveNew: true);
        //        IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
        //        int? iDepotId = (from a in base_DeviceInfoService.GetModels(p => p.ID == iDeviceId)
        //                       join b in data_GameInfoService.GetModels() on a.GameIndexID equals b.ID
        //                       select b.DepotID).FirstOrDefault();

        //        //开启EF事务
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            try
        //            {
        //                var data_Reload = new Data_Reload();
        //                data_Reload.StoreID = storeId;
        //                data_Reload.GoodID = Convert.ToInt32(goodId);
        //                data_Reload.DeviceID = !string.IsNullOrEmpty(deviceId) ? Convert.ToInt32(deviceId) : (int?)null;
        //                data_Reload.DeviceType = Convert.ToInt32(deviceType);
        //                data_Reload.ReloadType = Convert.ToInt32(reloadType);
        //                data_Reload.ReloadCount = iReloadCount;
        //                data_Reload.Note = note;
        //                data_Reload.RealTime = DateTime.Now;
        //                data_Reload.UserID = iUserId;
        //                if (!data_ReloadService.Add(data_Reload))
        //                {
        //                    errMsg = "添加设备安装信息失败";
        //                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                }

        //                //更新库存
        //                if (base_StorageInfoService.Any(a => a.DepotID == iDepotId && a.GoodID == iGoodId))
        //                {
        //                    var base_StorageInfo = base_StorageInfoService.GetModels(p => p.DepotID == iDepotId && p.GoodID == iGoodId).FirstOrDefault();
        //                    base_StorageInfo.Surplus -= iReloadCount;
        //                    if (!base_StorageInfoService.Update(base_StorageInfo))
        //                    {
        //                        errMsg = "更新库存信息失败";
        //                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                    }

        //                    //添加库存记录
        //                    var data_Storage_Record = new Data_Storage_Record();
        //                    data_Storage_Record.StorageCount = iReloadCount;
        //                    data_Storage_Record.StorageFlag = (int)StockFlag.Out;
        //                    data_Storage_Record.StorageID = base_StorageInfo.ID;
        //                    data_Storage_Record.CreateTime = DateTime.Now;
        //                    if (!data_Storage_RecordService.Add(data_Storage_Record))
        //                    {
        //                        errMsg = "添加库存记录失败";
        //                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
        //                    }
        //                }                                                

        //                ts.Complete();
        //            }
        //            catch (Exception ex)
        //            {
        //                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
        //            }
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
        public object CheckDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? Convert.ToString(dicParas["id"]) : string.Empty;
                string realCount = dicParas.ContainsKey("realCount") ? Convert.ToString(dicParas["realCount"]) : string.Empty;

                #region 参数验证

                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "安装索引id不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(realCount))
                {
                    errMsg = "实点数realCount不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(realCount))
                {
                    errMsg = "实点数realCount格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iRealCount = Convert.ToInt32(realCount);
                if (iRealCount < 0)
                {
                    errMsg = "实点数realCount不能为负数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }  

                #endregion
                

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
        
    }
}