using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.DAL;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Project 的摘要说明
    /// </summary>
    public class Project : ApiBase
    {
        #region 游乐项目维护

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryProjectInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;
                var notProjectIds = dicParas.Get("notProjectIds");                

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;
                
                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                

                #region Sql语句
                string sql = @"SELECT
                                    a.*,
                                    b.AreaName,
                                    c.DictKey AS ProjectTypeStr
                                FROM
                                	Data_ProjectInfo a
                                LEFT JOIN Data_GroupArea b ON a.AreaType = b.ID  
                                LEFT JOIN Dict_System c ON a.ProjectType = c.ID                                
                                WHERE a.State=1 ";
                sql += " AND a.StoreID=" + storeId;

                #endregion

                var list = Data_ProjectInfoService.I.SqlQuery<Data_ProjectInfoList>(sql, parameters).ToList();
                if (!notProjectIds.IsNull())
                    list = list.Where(p => !notProjectIds.Contains(p.ID.ToString())).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        
        
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if(!dicParas.Get("id").Validintnozero("项目ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var data_ProjectInfo = Data_ProjectInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (data_ProjectInfo == null)
                {
                    errMsg = "该项目不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var gameIndex = data_ProjectInfo.GameIndex;
                var data_GameInfo = Data_GameInfoService.I.GetModels(p => p.ID == gameIndex).FirstOrDefault() ?? new Data_GameInfo();
                var model = new
                {
                    data_ProjectInfo = data_ProjectInfo,
                    PushBalanceIndex1 = data_GameInfo.PushBalanceIndex1,
                    PushCoin1 = data_GameInfo.PushCoin1,
                    PushBalanceIndex2 = data_GameInfo.PushBalanceIndex2,
                    PushCoin2 = data_GameInfo.PushCoin2,
                    ReadCat = data_GameInfo.ReadCat,
                    PushLevel = data_GameInfo.PushLevel
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectTimeInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("游乐项目ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var linq = new
                {
                    data_ProjectTime = Data_Project_TimeInfoService.I.GetModels(p => p.ProjectTimeID == id).FirstOrDefault() ?? new Data_Project_TimeInfo { ProjectTimeID = id },
                    ProjectBandPrices = Data_Project_BandPriceService.I.GetModels(p => p.ProjectID == id)
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq.AsFlatDictionary());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }  

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveProjectInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("chargeType").Validint("扣费类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("projectType").Validintnozero("项目类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("areaType").Validintnozero("区域ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);
                var chargeType = dicParas.Get("chargeType").Toint();                
                      
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_ProjectInfoService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_ProjectInfo();
                        model.MerchID = merchId;
                        model.StoreID = storeId;
                        Utils.GetModel(dicParas, ref model);

                        //保存设备配置信息
                        if (chargeType == (int)ProjectInfoChargeType.Count)
                        {
                            if (!dicParas.Get("pushBalanceIndex1").Validintnozero("按钮一投币类别", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            if (!dicParas.Get("pushCoin1").Validintnozero("按钮一投币量", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            if (!dicParas.Get("readCat").Validint("刷卡即扣", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            if (!dicParas.Get("pushLevel").Validint("投币信号电平", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            if (!dicParas.Get("guestPrice").Validdecimalnozero("散客支付价格", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            var gameIndex = model.GameIndex ?? 0;
                            var data_GameInfoService = Data_GameInfoService.I;
                            var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == gameIndex).FirstOrDefault() ?? new Data_GameInfo();

                            //获取参数默认值
                            IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
                            int GameInfoId = dict_SystemService.GetModels(p => p.DictKey.Equals("游戏机档案维护")).FirstOrDefault().ID;
                            var dict = dict_SystemService.GetModels(p => p.PID == GameInfoId && p.Enabled == 1).Select(o => new
                            {
                                DictKey = o.DictKey,
                                DictValue = o.DictValue
                            }).Distinct().ToDictionary(d => d.DictKey, d => (object)d.DictValue);
                            
                            Utils.GetModel(dict, ref data_GameInfo);
                            data_GameInfo.GameID = string.Empty;
                            data_GameInfo.GameName = model.ProjectName;
                            data_GameInfo.GameType = model.ProjectType;
                            data_GameInfo.AreaID = model.AreaType;
                            data_GameInfo.PushBalanceIndex1 = dicParas.Get("pushBalanceIndex1").Toint();
                            data_GameInfo.PushCoin1 = dicParas.Get("pushCoin1").Toint();                            
                            data_GameInfo.ReadCat = dicParas.Get("readCat").Toint();
                            data_GameInfo.PushLevel = dicParas.Get("pushLevel").Toint();
                            data_GameInfo.PushBalanceIndex2 = dicParas.Get("pushBalanceIndex2").Toint(0);
                            data_GameInfo.PushCoin2 = dicParas.Get("pushCoin2").Toint(0);
                            if (gameIndex == 0)
                            {
                                data_GameInfo.State = 1;
                                data_GameInfo.MerchID = merchId;
                                data_GameInfo.StoreID = storeId;
                                if (!data_GameInfoService.Add(data_GameInfo))
                                {
                                    errMsg = "保存设备配置信息失败";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }

                                model.GameIndex = data_GameInfo.ID;
                            }
                            else
                            {
                                if (data_GameInfo.ID == 0)
                                {
                                    errMsg = "该设备配置信息不存在";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }

                                if (!data_GameInfoService.Update(data_GameInfo))
                                {
                                    errMsg = "保存设备配置信息失败";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }                            
                        }   
                                                
                        if (id == 0)
                        {
                            model.State = 1;
                            if (!Data_ProjectInfoService.I.Add(model))
                            {
                                errMsg = "保存游乐项目失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该游乐项目不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_ProjectInfoService.I.Update(model))
                            {
                                errMsg = "保存游乐项目失败";
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
        public object SaveProjectTimeInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("projectTimeId").Validintnozero("游乐项目ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                              
                if (!dicParas.GetArray("projectBandPrices").Validarray("计时项目波段设定", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("chargeType").Toint() == (int)ProjectTimeChargeType.Weixin && dicParas.GetObject("cycleType").Toint() != (int)CycleType.Out)
                {
                    errMsg = "微信票码验证进闸时, 计费方式须为出闸一次性扣费";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var projectTimeId = dicParas.Get("projectTimeId").Toint();
                var projectBandPrices = dicParas.GetArray("projectBandPrices");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_ProjectInfoService.I.GetModels(p => p.ID == projectTimeId).FirstOrDefault();
                        if (model == null)
                        {
                            errMsg = "该游乐项目信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (model.ChargeType != (int)ProjectInfoChargeType.Time)
                        {
                            errMsg = "该游乐项目信息不是计时项目";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var projectTimeInfoModel = Data_Project_TimeInfoService.I.GetModels(p => p.ProjectTimeID == projectTimeId).FirstOrDefault() ?? new Data_Project_TimeInfo();
                        projectTimeInfoModel.StoreID = storeId;
                        projectTimeInfoModel.MerchID = merchId;
                        Utils.GetModel(dicParas, ref projectTimeInfoModel);
                        if (projectTimeInfoModel.ID == 0)
                        {
                            if (!Data_Project_TimeInfoService.I.Add(projectTimeInfoModel))
                            {
                                errMsg = "保存游乐项目计时规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!Data_Project_TimeInfoService.I.Update(projectTimeInfoModel))
                            {
                                errMsg = "保存游乐项目计时规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //保存计时项目波段设定
                        if (projectBandPrices != null && projectBandPrices.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var bandPriceModel in Data_Project_BandPriceService.I.GetModels(p => p.ProjectID == projectTimeId))
                            {
                                Data_Project_BandPriceService.I.DeleteModel(bandPriceModel);
                            }

                            foreach (IDictionary<string, object> el in projectBandPrices)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("useTimeCount").Validint("总用时时间", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("cycleTime").Validint("周期时间", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("count").Validint("扣费数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var bandPriceModel = new Data_Project_BandPrice();
                                    bandPriceModel.UseTimeCount = dicPara.Get("useTimeCount").Toint();
                                    bandPriceModel.UseType = dicPara.Get("useType").Toint();
                                    bandPriceModel.CycleTime = dicPara.Get("cycleTime").Toint();
                                    bandPriceModel.Count = dicPara.Get("count").Toint();
                                    bandPriceModel.StoreID = storeId;
                                    bandPriceModel.ProjectID = projectTimeId;
                                    bandPriceModel.MerchID = merchId;
                                    bandPriceModel.StoreID = storeId;
                                    Data_Project_BandPriceService.I.AddModel(bandPriceModel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_Project_BandPriceService.I.SaveChanges())
                            {
                                errMsg = "保存计时项目波段设定失败";
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
        public object DelProjectInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("项目ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_ProjectInfoService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该游乐项目不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_ProjectInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        model.State = 0;
                        Data_ProjectInfoService.I.UpdateModel(model);

                        if (model.ChargeType == (int)ProjectInfoChargeType.Count)
                        {
                            //删除游戏机信息
                            var gameIndex = model.GameIndex;
                            if (!Data_GameInfoService.I.Any(a => a.ID == gameIndex))
                            {
                                errMsg = "该计次项目关联的游戏机信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var gameInfo = Data_GameInfoService.I.GetModels(p => p.ID == gameIndex).FirstOrDefault();
                            gameInfo.State = 0;
                            Data_GameInfoService.I.UpdateModel(gameInfo);
                        }
                        
                        //解除设备绑定信息
                        foreach (var bindModel in Data_Project_BindDeviceService.I.GetModels(p => p.ProjectID == id))
                        {
                            Data_Project_BindDeviceService.I.DeleteModel(bindModel);
                            
                            var deviceId = bindModel.DeviceID;
                            if (!Base_DeviceInfoService.I.Any(a => a.ID == deviceId))
                            {
                                errMsg = "该绑定的设备信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var deviceInfo = Base_DeviceInfoService.I.GetModels(p => p.ID == deviceId).FirstOrDefault();
                            deviceInfo.GameIndexID = (int?)null;
                            deviceInfo.BindDeviceID = (int?)null;
                            deviceInfo.SiteName = string.Empty;
                            deviceInfo.segment = string.Empty;
                            deviceInfo.Address = string.Empty;
                            Base_DeviceInfoService.I.UpdateModel(deviceInfo);                            
                        }

                        if (!Base_DeviceInfoService.I.SaveChanges())
                        {
                            errMsg = "解除设备绑定失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_Project_BindDeviceService.I.SaveChanges())
                        {
                            errMsg = "删除绑定设备失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GameInfoService.I.SaveChanges())
                        {
                            errMsg = "删除关联游戏机失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_ProjectInfoService.I.SaveChanges())
                        {
                            errMsg = "删除游乐项目失败";
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

        [Authorize(Roles = "MerchUser", Inherit = true)]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var storeIds = dicParas.Get("storeIds");

                var query = Data_ProjectInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
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
                               ProjectName = a.ProjectName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectGames(Dictionary<string, object> dicParas)
        {
            try
            {
                var gameTypeId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("游戏机类型", StringComparison.OrdinalIgnoreCase) && p.PID == 0).FirstOrDefault().ID;
                var projectGameId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("游乐项目", StringComparison.OrdinalIgnoreCase) && p.PID == gameTypeId).FirstOrDefault().ID;
                var linq = Dict_SystemService.I.GetModels(p => p.PID == projectGameId).Select(o => new
                {
                    ID = o.ID,
                    Name = o.DictKey
                });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #endregion

        #region 绑定设备维护

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetUnBindDeviceDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var bindDeviceIds = Data_Project_BindDeviceService.I.GetModels().Select(o => o.DeviceID);
                var linq = from a in Base_DeviceInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)
                               && (p.GameIndexID ?? 0) == 0 && p.DeviceStatus == 1 && (p.type == (int)DeviceType.卡头 || p.type == (int)DeviceType.闸机 || p.type == (int)DeviceType.自助机)).ToList()
                           where !bindDeviceIds.Contains(a.ID)
                           orderby a.DeviceName
                           select new
                           {
                               ID = a.ID,
                               DeviceName = a.DeviceName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryBindDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;
                if(!dicParas.Get("projectId").Validintnozero("游乐项目ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var projectId = dicParas.Get("projectId").Toint();

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                #region Sql语句
                string sql = @"SELECT
                                    a.*,
                                    b.DeviceName,
                                    c.DictKey AS DeviceTypeStr
                                FROM
                                	Data_Project_BindDevice a
                                LEFT JOIN Base_DeviceInfo b ON a.DeviceID = b.ID  
                                INNER JOIN Dict_System c ON convert(varchar, b.type)=c.DictValue 
                                INNER JOIN Dict_System d ON c.PID=d.ID and d.DictKey='设备类型'                             
                                WHERE 1=1
                            ";
                sql += " AND a.ProjectID=" + projectId;

                #endregion

                var list = Data_FreeGiveRuleService.I.SqlQuery<Data_Project_BindDeviceList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetBindDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("绑定流水号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var bindDevice = Data_Project_BindDeviceService.I.GetModels(p => p.ID == id).FirstOrDefault();
                var model = new
                {
                    bindDevice = bindDevice,
                    bindDeviceId = Base_DeviceInfoService.I.GetModels(p => p.ID == bindDevice.DeviceID).Select(o => o.BindDeviceID).FirstOrDefault()
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken, SysIdAndVersionNo = false)]
        public object SaveBindDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("projectId").Validintnozero("游乐项目ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("deviceId").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("bindDeviceId").Validintnozero("路由器ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("workType").Validint("工作方式", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);
                var projectId = dicParas.Get("projectId").Toint();
                var deviceId = dicParas.Get("deviceId").Toint();
                var bindDeviceId = dicParas.Get("bindDeviceId").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var data_ProjectInfoService = Data_ProjectInfoService.I;
                        var data_Project_BindDeviceService = Data_Project_BindDeviceService.I;
                        var data_GameInfoService = Data_GameInfoService.I;
                        var base_DeviceInfoService = Base_DeviceInfoService.I;

                        //检查游乐项目信息
                        if (!data_ProjectInfoService.Any(a => a.ID == projectId))
                        {
                            errMsg = "该游乐项目不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var projectInfo = data_ProjectInfoService.GetModels(p => p.ID == projectId).FirstOrDefault();
                        var gameIndex = projectInfo.GameIndex;
                        if(gameIndex > 0 && !data_GameInfoService.Any(a=>a.ID == gameIndex))
                        {
                            errMsg = "该游乐项目绑定的游戏机信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //检查绑定设备信息
                        if (!base_DeviceInfoService.Any(a => a.ID == deviceId))
                        {
                            errMsg = "该设备信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var deviceInfo = base_DeviceInfoService.GetModels(p => p.ID == deviceId).FirstOrDefault();
                        if (deviceInfo.type != (int)DeviceType.卡头 && deviceInfo.type != (int)DeviceType.闸机 && deviceInfo.type != (int)DeviceType.自助机)
                        {
                            errMsg = "绑定的设备类型不正确";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (deviceInfo.GameIndexID > 0 || deviceInfo.BindDeviceID > 0)
                        {
                            errMsg = "该设备已被绑定";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //检查路由器信息
                        if (!base_DeviceInfoService.Any(a => a.ID == bindDeviceId))
                        {
                            errMsg = "该路由器信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //继承路由器段号
                        var routInfo = base_DeviceInfoService.GetModels(p => p.ID == bindDeviceId).FirstOrDefault();
                        deviceInfo.segment = routInfo.segment;

                        //按顺序生成机头地址(01~9F)十六进制
                        var flag = false;
                        for (int i = 1; i < 160; i++)
                        {
                            var address = Convert.ToString(i, 16).PadLeft(2, '0').ToUpper();
                            if (!base_DeviceInfoService.Any(p => p.BindDeviceID == bindDeviceId && p.ID != deviceId && p.ID != bindDeviceId && p.Address.Equals(address, StringComparison.OrdinalIgnoreCase)))
                            {
                                deviceInfo.Address = address;
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                        {
                            errMsg = "没有可用的机头地址";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //更新绑定设备信息
                        deviceInfo.GameIndexID = gameIndex;
                        deviceInfo.BindDeviceID = bindDeviceId;
                        base_DeviceInfoService.UpdateModel(deviceInfo);

                        //保存设备绑定信息
                        var model = data_Project_BindDeviceService.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_Project_BindDevice();
                        Utils.GetModel(dicParas, ref model);
                        model.MerchID = merchId;
                        if (id == 0)
                        {                            
                            data_Project_BindDeviceService.AddModel(model);
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该绑定流水号不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            data_Project_BindDeviceService.UpdateModel(model);
                        }

                        if (!base_DeviceInfoService.SaveChanges())
                        {
                            errMsg = "更新绑定设备信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!data_Project_BindDeviceService.SaveChanges())
                        {
                            errMsg = "保存设备绑定信息失败";
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
        public object DelBindDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("绑定流水号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_Project_BindDeviceService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该绑定流水号不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }                        

                        var model = Data_Project_BindDeviceService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        Data_Project_BindDeviceService.I.DeleteModel(model);

                        //解除设备绑定信息
                        var deviceId = model.DeviceID;
                        if (!Base_DeviceInfoService.I.Any(a => a.ID == deviceId))
                        {
                            errMsg = "该绑定的设备信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var deviceInfo = Base_DeviceInfoService.I.GetModels(p => p.ID == deviceId).FirstOrDefault();
                        deviceInfo.GameIndexID = (int?)null;
                        deviceInfo.BindDeviceID = (int?)null;
                        deviceInfo.SiteName = string.Empty;
                        deviceInfo.segment = string.Empty;
                        deviceInfo.Address = string.Empty;
                        Base_DeviceInfoService.I.UpdateModel(deviceInfo);
                        
                        if (!Base_DeviceInfoService.I.SaveChanges())
                        {
                            errMsg = "解除设备绑定失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_Project_BindDeviceService.I.SaveChanges())
                        {
                            errMsg = "删除绑定设备失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
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