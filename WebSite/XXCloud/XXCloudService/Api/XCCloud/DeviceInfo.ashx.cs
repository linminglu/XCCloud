﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
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
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Gift 的摘要说明
    /// </summary>
    public class DeviceInfo : ApiBase
    {    
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

                var query = Base_DeviceInfoService.I.GetModels(p => p.type == (int)DeviceType.卡头 && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
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

                var gameId = dicParas.Get("gameId").Toint();

                var query = Base_DeviceInfoService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.type != (int)DeviceType.卡头);
                if (gameId != null)
                {
                    query = query.Where(w => w.GameIndexID == gameId);
                }

                var base_DeviceInfo = (from a in query
                                           join b in Data_GameInfoService.N.GetModels() on a.GameIndexID equals b.ID into b1
                                           from b in b1.DefaultIfEmpty()
                                           join c in Dict_SystemService.N.GetModels() on b.GameType equals c.ID into c1
                                           from c in c1.DefaultIfEmpty()
                                           join d in Data_GroupAreaService.N.GetModels() on b.AreaID equals d.ID into d1
                                           from d in d1.DefaultIfEmpty()
                                           orderby a.MCUID
                                           
                                      select new Base_DeviceInfoList
                                      {
                                          ID = a.ID,
                                          MCUID = a.MCUID,
                                          DeviceName = a.DeviceName,
                                          DeviceType = a.type,
                                          GameType = b != null ? b.GameType : (int?)null,
                                          GameTypeStr = c != null ? c.DictKey : string.Empty,
                                          GameName = b != null ? b.GameName : string.Empty,
                                          AreaName = d != null ? d.AreaName : string.Empty,
                                          SiteName = a.SiteName,
                                          segment = a.segment,
                                          Address = a.Address,
                                          DeviceRunning = string.Empty,
                                          DeviceStatus = a.DeviceStatus,
                                          DeviceStatusStr = a.DeviceStatus != null ? (a.DeviceStatus == 0 ? "停用" :
                                                        a.DeviceStatus == 1 ? "正常" :
                                                        a.DeviceStatus == 2 ? "锁定" : string.Empty) : string.Empty
                                      }).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, base_DeviceInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 卡头列表查询
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryHeadDeviceInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var gameId = dicParas.Get("gameId").Toint();

                var query = Base_DeviceInfoService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.type == (int)DeviceType.卡头);
                if (gameId != null)
                {
                    query = query.Where(w => w.GameIndexID == gameId);
                }

                var base_DeviceInfo = (from a in query
                                      join b in Data_GameInfoService.N.GetModels() on a.GameIndexID equals b.ID into b1
                                      from b in b1.DefaultIfEmpty()
                                      join c in Dict_SystemService.N.GetModels() on b.GameType equals c.ID into c1
                                      from c in c1.DefaultIfEmpty()
                                      join d in Data_GroupAreaService.N.GetModels() on b.AreaID equals d.ID into d1
                                      from d in d1.DefaultIfEmpty()
                                      orderby a.MCUID
                                      select new Base_DeviceInfoList
                                      {
                                          ID = a.ID,
                                          MCUID = a.MCUID,
                                          DeviceName = a.DeviceName,
                                          DeviceType = a.type,
                                          GameType = b != null ? b.GameType : (int?)null,
                                          GameTypeStr = c != null ? c.DictKey : string.Empty,
                                          GameName = b != null ? b.GameName : string.Empty,
                                          AreaName = d != null ? d.AreaName : string.Empty,
                                          SiteName = a.SiteName,
                                          segment = a.segment,
                                          Address = a.Address,
                                          DeviceRunning = string.Empty,
                                          DeviceStatus = a.DeviceStatus,
                                          DeviceStatusStr = a.DeviceStatus != null ? (a.DeviceStatus == 0 ? "停用" :
                                                        a.DeviceStatus == 1 ? "正常" :
                                                        a.DeviceStatus == 2 ? "锁定" : string.Empty) : string.Empty
                                      }).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, base_DeviceInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取报警日志
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameAlarm(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if(!dicParas.Get("id").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                if(!Base_DeviceInfoService.I.Any(a=>a.ID == id))
                {
                    errMsg = "该设备信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var base_DeviceInfo = from a in Base_DeviceInfoService.N.GetModels(p=>p.ID == id)
                                      join b in Log_GameAlarmService.N.GetModels() on a.ID equals b.DeviceID 
                                      join c in Data_GameInfoService.N.GetModels() on a.GameIndexID equals c.ID
                                      orderby c.GameName
                                      select new
                                      {
                                          GameName = c.GameName,
                                          SiteName = a.SiteName,
                                          DeviceName = a.DeviceName,                                          
                                          segment = a.segment,
                                          Address = a.Address,
                                          AlertType = b.AlertType,
                                          HappenTime = b.HappenTime,
                                          ICCardID = b.ICCardID,
                                          LockGame = b.LockGame,
                                          LockGameStr = b.LockGame == 1 ? "是" : b.LockGame == 0 ? "否" : string.Empty,
                                          LockMember = b.LockMember,
                                          LockMemberStr = b.LockMember == 1 ? "是" : b.LockMember == 0 ? "否" : string.Empty,
                                          EndTime = b.EndTime,
                                          State = b.State,
                                          StateStr = b.State == 0 ? "活动" : b.State == 1 ? "确认" : b.State == 2 ? "解决" : string.Empty
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

                var query = Base_DeviceInfoService.I.GetModels(p => p.type == (int)DeviceType.路由器 && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                
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
                    if (siteName.Length > 20)
                    {
                        errMsg = "P位编号长度不能超过20个字符";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                #endregion

                if (!Base_DeviceInfoService.I.Any(p => p.ID == iId))
                {
                    errMsg = "设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                

                var data_DeviceInfo = Base_DeviceInfoService.I.GetModels(p => p.ID == iId).FirstOrDefault();
                if (data_DeviceInfo.type != (int)DeviceType.卡头)
                {
                    errMsg = "机台绑定须为卡头设备";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //绑定
                if (iState == 1)
                {
                    if (!Base_DeviceInfoService.I.Any(p => p.ID == iBindDeviceId))
                    {
                        errMsg = "路由器设备不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Data_GameInfoService.I.Any(p => p.ID == iGameIndexId))
                    {
                        errMsg = "游戏机信息不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    } 

                    if (Base_DeviceInfoService.I.Any(p => p.GameIndexID == iGameIndexId && p.ID != iId && p.SiteName.Equals(siteName, StringComparison.OrdinalIgnoreCase)))
                    {
                        errMsg = "游戏机P位被占用";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    data_DeviceInfo.GameIndexID = iGameIndexId;
                    data_DeviceInfo.BindDeviceID = iBindDeviceId;
                    data_DeviceInfo.SiteName = siteName;

                    //继承路由器段号
                    var routInfo = Base_DeviceInfoService.I.GetModels(p => p.ID == iBindDeviceId).FirstOrDefault();
                    data_DeviceInfo.segment = routInfo.segment;

                    //按顺序生成机头地址(01~9F)十六进制
                    for (int i = 1; i < 160; i++)
                    {
                        var iAddress = Convert.ToString(i, 16).PadLeft(2, '0').ToUpper();
                        if (!Base_DeviceInfoService.I.Any(p => p.BindDeviceID == iBindDeviceId && p.ID != iBindDeviceId && p.Address.Equals(iAddress, StringComparison.OrdinalIgnoreCase)))
                        {
                            data_DeviceInfo.Address = iAddress;
                            break;
                        }
                    }
                }
                //解绑
                else if (iState == 0)
                {
                    data_DeviceInfo.GameIndexID = (int?)null;
                    data_DeviceInfo.BindDeviceID = (int?)null;
                    data_DeviceInfo.SiteName = string.Empty;
                    data_DeviceInfo.segment = string.Empty;
                    data_DeviceInfo.Address = string.Empty;
                }
                else
                {
                    errMsg = "绑定参数state值无效";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Base_DeviceInfoService.I.Update(data_DeviceInfo))
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

                if (!Base_DeviceInfoService.I.Any(p => p.ID == iId))
                {
                    errMsg = "设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (iState != 0 && iState != 1)
                {
                    errMsg = "锁定参数state值无效";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DeviceInfo = Base_DeviceInfoService.I.GetModels(p => p.ID == iId).FirstOrDefault();
                data_DeviceInfo.DeviceStatus = iState == 1 ? (int)DeviceStatus.锁定 : (int)DeviceStatus.正常;
                if (!Base_DeviceInfoService.I.Update(data_DeviceInfo))
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

                if (!Base_DeviceInfoService.I.Any(p => p.ID == iId))
                {
                    errMsg = "设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var barCode = Base_DeviceInfoService.I.GetModels(p => p.ID == iId).Select(o => o.BarCode).FirstOrDefault();
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, barCode);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 通讯绑定(解绑)
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object BindRoutineInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? Convert.ToString(dicParas["id"]) : string.Empty;
                string state = dicParas.ContainsKey("state") ? Convert.ToString(dicParas["state"]) : string.Empty;               
                string bindDeviceId = dicParas.ContainsKey("bindDeviceId") ? Convert.ToString(dicParas["bindDeviceId"]) : string.Empty;
                string address = dicParas.ContainsKey("address") ? Convert.ToString(dicParas["address"]) : string.Empty;
                int iId = 0, iState = 0, iBindDeviceId = 0;
                int.TryParse(id, out iId);
                int.TryParse(state, out iState);
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
                    if (string.IsNullOrEmpty(bindDeviceId))
                    {
                        errMsg = "路由器bindDeviceId不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (string.IsNullOrEmpty(address))
                    {
                        errMsg = "通讯地址不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (address.Length > 2)
                    {
                        errMsg = "通讯地址长度不能超过2个字符";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    bool isValidAddress = false;
                    for (var i = 0; i < 11; i++)
                    {
                        if (address.ToUpper() == ("A" + (i < 10 ? i.ToString() : "A")))
                        {
                            isValidAddress = true;
                            break;
                        }
                    }
                    if(!isValidAddress)
                    {
                        errMsg = "通讯地址范围是A0~AA";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                #endregion

                if (!Base_DeviceInfoService.I.Any(p => p.ID == iId))
                {
                    errMsg = "设备不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DeviceInfo = Base_DeviceInfoService.I.GetModels(p => p.ID == iId).FirstOrDefault();
                if (data_DeviceInfo.type == (int)DeviceType.卡头 || data_DeviceInfo.type == (int)DeviceType.路由器)
                {
                    errMsg = "通讯绑定设备类型不能是卡头或路由器";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //绑定
                if (iState == 1)
                {
                    if (Base_DeviceInfoService.I.Any(p => p.ID != iId && p.BindDeviceID == iBindDeviceId && p.Address.Equals(address, StringComparison.OrdinalIgnoreCase)))
                    {
                        errMsg = "该地址被占用";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    data_DeviceInfo.BindDeviceID = iBindDeviceId;
                    data_DeviceInfo.Address = address.ToUpper();
                }
                //解绑
                else if (iState == 0)
                {
                    data_DeviceInfo.BindDeviceID = (int?)null;
                    data_DeviceInfo.Address = string.Empty;
                }
                else
                {
                    errMsg = "绑定参数state值无效";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Base_DeviceInfoService.I.Update(data_DeviceInfo))
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
        public object SaveCoinSalerInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if(dicParas.Get("deviceId").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor1En").Validint("1号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor2En").Validint("2号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor1Coin").Validintnozero("马达1出币比例", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor2Coin").Validintnozero("马达2出币比例", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("dubleCheck").Validint("双光眼检测", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("digitCoinEn").Validint("数字币销售模式", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var deviceId = dicParas.Get("deviceId").Toint();

                //开启EF事务
                var base_DeviceInfoService = Base_DeviceInfoService.I;
                var base_DeviceInfo_ExtService = Base_DeviceInfo_ExtService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if(!base_DeviceInfoService.Any(a=>a.ID == deviceId))
                        {
                            errMsg = "该设备信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo = base_DeviceInfoService.GetModels(p => p.ID == deviceId).FirstOrDefault();
                        if (base_DeviceInfo.type != (int)DeviceType.售币机)
                        {
                            errMsg = "该设备不是售币机类型设备";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo_Ext = base_DeviceInfo_ExtService.GetModels(p => p.DeviceID == deviceId).FirstOrDefault() ?? new Base_DeviceInfo_Ext();
                        Utils.GetModel(dicParas, ref base_DeviceInfo_Ext);
                        if (deviceId == 0)
                        {
                            if (!base_DeviceInfo_ExtService.Add(base_DeviceInfo_Ext))
                            {
                                errMsg = "售币机参数设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (base_DeviceInfo_Ext.ID == 0)
                            {
                                errMsg = "该设置信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!base_DeviceInfo_ExtService.Update(base_DeviceInfo_Ext))
                            {
                                errMsg = "售币机参数设置失败";
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCoinFetcherInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (dicParas.Get("deviceId").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor1En").Validint("1号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor2En").Validint("2号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor1Coin").Validintnozero("马达1出币比例", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor2Coin").Validintnozero("马达2出币比例", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("balanceIndex").Validintnozero("会员卡存币数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("toCard").Validdecimalnozero("会员卡提币类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("fromDevice").Validintnozero("提币机出币数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var deviceId = dicParas.Get("deviceId").Toint();

                //开启EF事务
                var base_DeviceInfoService = Base_DeviceInfoService.I;
                var base_DeviceInfo_ExtService = Base_DeviceInfo_ExtService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!base_DeviceInfoService.Any(a => a.ID == deviceId))
                        {
                            errMsg = "该设备信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo = base_DeviceInfoService.GetModels(p => p.ID == deviceId).FirstOrDefault();
                        if (base_DeviceInfo.type != (int)DeviceType.提币机)
                        {
                            errMsg = "该设备不是提币机类型设备";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo_Ext = base_DeviceInfo_ExtService.GetModels(p => p.DeviceID == deviceId).FirstOrDefault() ?? new Base_DeviceInfo_Ext();
                        Utils.GetModel(dicParas, ref base_DeviceInfo_Ext);
                        if (deviceId == 0)
                        {
                            if (!base_DeviceInfo_ExtService.Add(base_DeviceInfo_Ext))
                            {
                                errMsg = "提币机参数设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (base_DeviceInfo_Ext.ID == 0)
                            {
                                errMsg = "该设置信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!base_DeviceInfo_ExtService.Update(base_DeviceInfo_Ext))
                            {
                                errMsg = "提币机参数设置失败";
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCoinSaverInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (dicParas.Get("deviceId").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor1En").Validint("1号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor2En").Validint("2号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);               
                if (dicParas.Get("balanceIndex").Validintnozero("会员卡存币类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("toCard").Validdecimalnozero("会员卡存币数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("fromDevice").Validintnozero("会员卡余额增加数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("maxSaveCount").Validintnozero("币箱存储最大值", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var deviceId = dicParas.Get("deviceId").Toint();

                //开启EF事务
                var base_DeviceInfoService = Base_DeviceInfoService.I;
                var base_DeviceInfo_ExtService = Base_DeviceInfo_ExtService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!base_DeviceInfoService.Any(a => a.ID == deviceId))
                        {
                            errMsg = "该设备信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo = base_DeviceInfoService.GetModels(p => p.ID == deviceId).FirstOrDefault();
                        if (base_DeviceInfo.type != (int)DeviceType.存币机)
                        {
                            errMsg = "该设备不是存币机类型设备";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo_Ext = base_DeviceInfo_ExtService.GetModels(p => p.DeviceID == deviceId).FirstOrDefault() ?? new Base_DeviceInfo_Ext();
                        Utils.GetModel(dicParas, ref base_DeviceInfo_Ext);
                        if (deviceId == 0)
                        {
                            if (!base_DeviceInfo_ExtService.Add(base_DeviceInfo_Ext))
                            {
                                errMsg = "存币机参数设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (base_DeviceInfo_Ext.ID == 0)
                            {
                                errMsg = "该设置信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!base_DeviceInfo_ExtService.Update(base_DeviceInfo_Ext))
                            {
                                errMsg = "存币机参数设置失败";
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCoinPusherInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (dicParas.Get("deviceId").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor1En").Validint("1号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor2En").Validint("2号马达", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor1Coin").Validint("允许实物币投币", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("motor2Coin").Validint("允许会员卡投币", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("balanceIndex").Validintnozero("会员卡扣余额类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);               

                var deviceId = dicParas.Get("deviceId").Toint();

                //开启EF事务
                var base_DeviceInfoService = Base_DeviceInfoService.I;
                var base_DeviceInfo_ExtService = Base_DeviceInfo_ExtService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!base_DeviceInfoService.Any(a => a.ID == deviceId))
                        {
                            errMsg = "该设备信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo = base_DeviceInfoService.GetModels(p => p.ID == deviceId).FirstOrDefault();
                        if (base_DeviceInfo.type != (int)DeviceType.投币机)
                        {
                            errMsg = "该设备不是投币机类型设备";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo_Ext = base_DeviceInfo_ExtService.GetModels(p => p.DeviceID == deviceId).FirstOrDefault() ?? new Base_DeviceInfo_Ext();
                        Utils.GetModel(dicParas, ref base_DeviceInfo_Ext);
                        if (deviceId == 0)
                        {
                            if (!base_DeviceInfo_ExtService.Add(base_DeviceInfo_Ext))
                            {
                                errMsg = "投币机参数设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (base_DeviceInfo_Ext.ID == 0)
                            {
                                errMsg = "该设置信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!base_DeviceInfo_ExtService.Update(base_DeviceInfo_Ext))
                            {
                                errMsg = "投币机参数设置失败";
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveRoutineInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (dicParas.Get("deviceId").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("segment").Nonempty("段号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("segment").Length > 4)
                {
                    errMsg = "段号长度不能超过4个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var deviceId = dicParas.Get("deviceId").Toint();
                var segment = dicParas.Get("segment");

                //开启EF事务
                var base_DeviceInfoService = Base_DeviceInfoService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!base_DeviceInfoService.Any(a => a.ID == deviceId))
                        {
                            errMsg = "该设备信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_DeviceInfo = base_DeviceInfoService.GetModels(p => p.ID == deviceId).FirstOrDefault();
                        if (base_DeviceInfo.type != (int)DeviceType.路由器)
                        {
                            errMsg = "该设备不是路由器类型设备";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        base_DeviceInfo.segment = segment;
                        base_DeviceInfoService.UpdateModel(base_DeviceInfo);

                        //修改关联绑定设备的段号
                        var bindDevices = base_DeviceInfoService.GetModels(p => p.BindDeviceID == deviceId && p.ID != deviceId);
                        foreach (var bindModel in bindDevices)
                        {
                            bindModel.segment = segment;
                            base_DeviceInfoService.UpdateModel(bindModel);
                        }

                        if (!base_DeviceInfoService.SaveChanges())
                        {
                            errMsg = "设置路由器段号失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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