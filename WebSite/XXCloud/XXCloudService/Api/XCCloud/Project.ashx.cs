﻿using System;
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

        private string getProjectGameTypes(out string errMsg)
        {
            string projectGameTypes = string.Empty;
            errMsg = string.Empty;
            string sql = " exec  SP_DictionaryNodes @MerchID,@DictKey,@PDictKey,@RootID output ";
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@MerchID", "");
            parameters[1] = new SqlParameter("@DictKey", "游乐项目");
            parameters[2] = new SqlParameter("@PDictKey", "游戏机类型");
            parameters[3] = new SqlParameter("@RootID", SqlDbType.Int);
            parameters[3].Direction = System.Data.ParameterDirection.Output;
            System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
            if (ds.Tables.Count == 0)
            {
                errMsg = "没有找到节点信息";
                return projectGameTypes;
            }
            var dictionaryResponse = Utils.GetModelList<DictionaryResponseModel>(ds.Tables[0]).Where(w => w.Enabled == 1).ToList();
            projectGameTypes = string.Join(",", dictionaryResponse.Select(o => o.ID)).Trim(',');

            return projectGameTypes;
        }

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

                //排除游乐项目类型的游戏机
                var projectGameTypes = getProjectGameTypes(out errMsg);
                if (!errMsg.IsNull())
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
                                WHERE a.State=1 AND a.ProjectType not in (" + projectGameTypes + ")";
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
                var data_GameInfo = Data_GameInfoService.I.GetModels(p => p.ID == gameIndex).FirstOrDefault();
                var model = new
                {
                    data_ProjectInfo = data_ProjectInfo,
                    GameInfo = new 
                    {
                        PushBalanceIndex1 = data_GameInfo != null ? data_GameInfo.PushBalanceIndex1 : null,
                        PushCoin1 = data_GameInfo != null ? data_GameInfo.PushCoin1 : null,
                        PushBalanceIndex2 = data_GameInfo != null ? data_GameInfo.PushBalanceIndex2 : null,
                        PushCoin2 = data_GameInfo != null ? data_GameInfo.PushCoin2 : null,
                        ReadCat = data_GameInfo != null ? data_GameInfo.ReadCat : null,
                        PushLevel = data_GameInfo != null ? data_GameInfo.PushLevel : null,
                    }
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

                            var gameIndex = model.GameIndex;
                            var data_GameInfoService = Data_GameInfoService.I;
                            var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == gameIndex).FirstOrDefault() ?? new Data_GameInfo();
                            data_GameInfo.GameID = string.Empty;
                            data_GameInfo.GameName = model.ProjectName;
                            data_GameInfo.GameType = model.ProjectType;
                            data_GameInfo.PushBalanceIndex1 = dicParas.Get("pushBalanceIndex1").Toint();
                            data_GameInfo.PushCoin1 = dicParas.Get("pushCoin1").Toint();
                            data_GameInfo.PushBalanceIndex2 = dicParas.Get("pushBalanceIndex2").Toint();
                            data_GameInfo.PushCoin2 = dicParas.Get("pushCoin2").Toint();
                            data_GameInfo.ReadCat = dicParas.Get("readCat").Toint();
                            data_GameInfo.PushLevel = dicParas.Get("pushLevel").Toint();
                            if (gameIndex == 0)
                            {
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
                        if (!Data_ProjectInfoService.I.Update(model))
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var linq = from a in Data_ProjectInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
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
                               && (p.GameIndexID ?? 0) == 0 && p.DeviceStatus == 1 && (p.type == (int)DeviceType.卡头 || p.type == (int)DeviceType.闸机)).ToList()
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

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, bindDevice);
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
                string errMsg = string.Empty;
                if (!dicParas.Get("projectId").Validintnozero("游乐项目ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("deviceId").Validintnozero("设备ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_Project_BindDeviceService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_Project_BindDevice();
                        Utils.GetModel(dicParas, ref model);
                        if (id == 0)
                        {
                            if (!Data_Project_BindDeviceService.I.Add(model))
                            {
                                errMsg = "添加绑定设备失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该绑定流水号不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_Project_BindDeviceService.I.Update(model))
                            {
                                errMsg = "更新绑定设备失败";
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
                        if (!Data_Project_BindDeviceService.I.Delete(model))
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