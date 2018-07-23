﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.SocketService.UDP.Factory;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// GameInfo 的摘要说明
    /// </summary>
    public class GameInfo : ApiBase
    {       

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameInfoList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //排除游乐项目类型的游戏机
                var projectGameTypes = getProjectGameTypes(out errMsg);
                if (!errMsg.IsNull())
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                

                string sql = @"SELECT
                                    a.ID, a.GameID, a.GameName, a.GameType, b.DictKey AS GameTypeStr, (case when a.AreaID=0 then '全部' else d.AreaName end) AS AreaName,
                                    c.Area, c.ChangeTime, c.Price, a.PushCoin1, (case a.AllowElecPush when 1 then '启用' when 0 then '禁用' else '' end) AS AllowElecPushStr,
                                    (case a.LotteryMode when 1 then '启用' when 0 then '禁用' else '' end) AS LotteryModeStr, (case a.ReadCat when 1 then '启用' when 0 then '禁用' else '' end) AS ReadCatStr,
                                    (case a.State when 1 then '启用' when 0 then '禁用' else '' end) AS StateStr
                                FROM
                                	Data_GameInfo a
                                LEFT JOIN Dict_System b ON a.GameType=b.ID
                                LEFT JOIN Data_GameInfo_Ext c ON a.ID=c.GameID
                                LEFT JOIN Data_GroupArea d ON a.AreaID=d.ID
                                WHERE c.ValidFlag=1 AND a.GameType NOT IN (" + string.Join(",", projectGameTypes) + @") AND a.StoreID='" + storeId + @"'
                            ";
                sql = sql + sqlWhere;
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Data_GameInfoList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
                //XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                //string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                //var errMsg = string.Empty;
                
                ////排除游乐项目类型的游戏机
                //var projectGameTypes = getProjectGameTypes(out errMsg);
                //if (!errMsg.IsNull())
                //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //var data_GameInfo = from a in Data_GameInfoService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                //                    join b in Dict_SystemService.N.GetModels() on a.GameType equals b.ID into b1
                //                    from b in b1.DefaultIfEmpty()
                //                    join c in Data_GameInfo_ExtService.N.GetModels(p => p.ValidFlag == 1) on a.ID equals c.GameID into c1
                //                    from c in c1.DefaultIfEmpty()
                //                    join d in Data_GroupAreaService.N.GetModels() on a.AreaID equals d.ID into d1
                //                    from d in d1.DefaultIfEmpty()
                //                    where !projectGameTypes.Contains(a.GameType ?? 0)
                //                    orderby a.ID
                //                    select new
                //                    {
                //                        ID = a.ID,
                //                        GameID = a.GameID,
                //                        GameName = a.GameName,
                //                        GameType = a.GameType,
                //                        GameTypeStr = b != null ? b.DictKey : string.Empty,
                //                        AreaName = a.AreaID == 0 ? "全部" : (d != null ? d.AreaName : string.Empty),
                //                        Area = c != null ? c.Area : (decimal?)null,
                //                        //ChangeTime = c != null ? SqlFunctions.DateName("yyyy", c.ChangeTime) + "-" + SqlFunctions.DateName("mm", c.ChangeTime) + "-" + SqlFunctions.DateName("dd", c.ChangeTime) + " " + 
                //                        //                        SqlFunctions.DateName("hh", c.ChangeTime) + ":" + SqlFunctions.DateName("n", c.ChangeTime) + ":" + SqlFunctions.DateName("ss", c.ChangeTime) : string.Empty,
                //                        ChangeTime = c != null ? c.ChangeTime : (DateTime?)null,
                //                        Price = c != null ? c.Price : (int?)null,
                //                        //PushReduceFromCard = a.PushReduceFromCard,
                //                        PushCoin1 = a.PushCoin1,
                //                        AllowElecPushStr = a.AllowElecPush != null ? (a.AllowElecPush == 1 ? "启用" : "禁用") : "",
                //                        LotteryModeStr = a.LotteryMode != null ? (a.LotteryMode == 1 ? "启用" : "禁用") : "",
                //                        ReadCatStr = a.ReadCat != null ? (a.ReadCat == 1 ? "启用" : "禁用") : "",
                //                        StateStr = a.State != null ? (a.State == 1 ? "启用" : "禁用") : ""
                //                    };
                                    
                //return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_GameInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取游戏机下拉菜单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameInfoDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var errMsg = string.Empty;

                //排除游乐项目类型的游戏机
                var projectGameTypes = getProjectGameTypes(out errMsg);
                if (!errMsg.IsNull())
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                Dictionary<int, string> gameInfo = Data_GameInfoService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.State == 1 && !projectGameTypes.Contains(p.GameType ?? 0))
                .Select(o => new
                {
                    ID = o.ID,
                    GameName = o.GameName
                }).Distinct().ToDictionary(d => d.ID, d => d.GameName);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, gameInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取游戏机下拉菜单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object GetGameInfoDicFromProgram(Dictionary<string, object> dicParas)
        {
            try
            {
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string storeId = userTokenModel.StoreId;

                var errMsg = string.Empty;

                //排除游乐项目类型的游戏机
                var projectGameTypes = getProjectGameTypes(out errMsg);
                if (!errMsg.IsNull())
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                Dictionary<int, string> gameInfo = Data_GameInfoService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.State == 1 && !projectGameTypes.Contains(p.GameType ?? 0))
                    .Select(o => new
                    {
                        ID = o.ID,
                        GameName = o.GameName
                    }).Distinct().ToDictionary(d => d.ID, d => d.GameName);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, gameInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameInfo(Dictionary<string, object> dicParas)
        {
            try
            {                
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;                

                int iId = 0;
                int.TryParse(id, out iId);
                IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
                var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == iId).FirstOrDefault() ?? new Data_GameInfo();
                if (iId == 0)
                    data_GameInfo.State = 1;

                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
                int GameInfoId = dict_SystemService.GetModels(p => p.DictKey.Equals("游戏机档案维护")).FirstOrDefault().ID;
                var result = (from a in data_GameInfo.AsDictionary()
                              join b in dict_SystemService.GetModels(p => p.PID == GameInfoId && p.Enabled == 1) on a.Key equals b.DictKey into b1
                              from b in b1.DefaultIfEmpty()
                              select new
                              {
                                  name = a.Key,
                                  value = a.Value ?? (b != null ? b.DictValue : null),
                                  comment = b != null ? b.Comment : string.Empty
                              }).ToList();

                IData_GameInfo_ExtService data_GameInfo_ExtService = BLLContainer.Resolve<IData_GameInfo_ExtService>();
                IData_GameInfo_PhotoService data_GameInfo_PhotoService = BLLContainer.Resolve<IData_GameInfo_PhotoService>();
                var data_GameInfo_Ext = data_GameInfo_ExtService.GetModels(p => p.GameID == iId && p.ValidFlag == 1).FirstOrDefault() ?? new Data_GameInfo_Ext();
                result.Add(new { name = "GameCode", value = (object)data_GameInfo_Ext.GameCode, comment = string.Empty });
                result.Add(new { name = "Area", value = (object)data_GameInfo_Ext.Area, comment = string.Empty });
                result.Add(new { name = "ChangeTime", value = (object)Utils.ConvertFromDatetime(data_GameInfo_Ext.ChangeTime), comment = string.Empty });
                result.Add(new { name = "Evaluation", value = (object)data_GameInfo_Ext.Evaluation, comment = string.Empty });
                result.Add(new { name = "Price", value = (object)data_GameInfo_Ext.Price, comment = string.Empty });
                result.Add(new { name = "LowLimit", value = (object)data_GameInfo_Ext.LowLimit, comment = string.Empty });
                result.Add(new { name = "HighLimit", value = (object)data_GameInfo_Ext.HighLimit, comment = string.Empty });

                List<string> PhotoURLs = new List<string>();
                if (data_GameInfo_PhotoService.Any(p => p.GameID == iId))
                {
                    foreach (var model in data_GameInfo_PhotoService.GetModels(p => p.GameID == iId))
                    {
                        PhotoURLs.Add(model.PhotoURL);
                    }
                }

                result.Add(new { name = "PhotoURLs", value = (object)PhotoURLs, comment = string.Empty });

                //会员电子存票赠送设置
                var FreeLotteryRules = from a in Data_GameFreeLotteryRuleService.N.GetModels(p => p.GameIndex == iId)
                                       join b in Data_MemberLevelService.N.GetModels() on a.MemberLevelID equals b.ID into b1
                                       from b in b1.DefaultIfEmpty()
                                       select new 
                                       {
                                           MemberLevelID = a.MemberLevelID,
                                           MemberLevelName = b != null ? b.MemberLevelName : string.Empty,
                                           BaseLottery = a.BaseLottery,
                                           FreeCount = a.FreeCount
                                       };
                result.Add(new { name = "FreeLotteryRules", value = (object)FreeLotteryRules.ToList(), comment = string.Empty });

                //会员消费积分设置
                var GamePointRules = from a in Data_Game_PointRuleService.N.GetModels(p => p.GameIndex == iId)
                                     join b in Data_MemberLevelService.N.GetModels() on a.MemberLevelID equals b.ID into b1
                                     from b in b1.DefaultIfEmpty()
                                     join c in Dict_BalanceTypeService.N.GetModels() on a.Key1BalanceIndex equals c.ID into c1
                                     from c in c1.DefaultIfEmpty()
                                     join d in Dict_BalanceTypeService.N.GetModels() on a.Key2BalanceIndex equals d.ID into d1
                                     from d in d1.DefaultIfEmpty()
                                     select new
                                     {
                                         MemberLevelID = a.MemberLevelID,
                                         MemberLevelName = b != null ? b.MemberLevelName : string.Empty,
                                         Key1BalanceIndex = a.Key1BalanceIndex,
                                         Key1BalanceName = c != null ? c.TypeName : string.Empty,
                                         Key1Count = a.Key1Count,
                                         Key2BalanceIndex = a.Key2BalanceIndex,
                                         Key2BalanceName = d != null ? d.TypeName : string.Empty,
                                         Key2Count = a.Key2Count
                                     };
                result.Add(new { name = "GamePointRules", value = (object)GamePointRules.ToList(), comment = string.Empty });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);                            
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取游戏机类型复合列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameTypeGameList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;

                string sql = " exec  SP_DictionaryNodes @MerchID,@DictKey,@PDictKey,@RootID output ";
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@MerchID", "");
                parameters[1] = new SqlParameter("@DictKey", "游戏机类型");
                parameters[2] = new SqlParameter("@PDictKey", "");
                parameters[3] = new SqlParameter("@RootID", SqlDbType.Int);
                parameters[3].Direction = System.Data.ParameterDirection.Output;
                System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
                if (ds.Tables.Count == 0)
                {
                    errMsg = "没有找到节点信息";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                var dictionaryResponse = Utils.GetModelList<GameListModel>(ds.Tables[0]).Where(w => w.Enabled == 1).ToList();

                int rootId = 0;
                int.TryParse(parameters[3].Value.ToString(), out rootId);

                //实例化一个根节点
                GameListModel rootRoot = new GameListModel();
                rootRoot.ID = rootId;
                TreeHelper.LoopToAppendChildren(dictionaryResponse, rootRoot, storeId);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, rootRoot);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGameInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                string resultMsg = string.Empty;
                string id = dicParas.ContainsKey("ID") ? (dicParas["ID"] + "") : string.Empty;
                string gameId = dicParas.ContainsKey("gameId") ? (dicParas["gameId"] + "") : string.Empty;
                string gameType = dicParas.ContainsKey("gameType") ? (dicParas["gameType"] + "") : string.Empty;
                string gameName = dicParas.ContainsKey("gameName") ? (dicParas["gameName"] + "") : string.Empty;
                string area = dicParas.ContainsKey("area") ? (dicParas["area"] + "") : string.Empty;
                string changeTime = dicParas.ContainsKey("changeTime") ? (dicParas["changeTime"] + "") : string.Empty;
                string evaluation = dicParas.ContainsKey("evaluation") ? (dicParas["evaluation"] + "") : string.Empty;
                string price = dicParas.ContainsKey("price") ? (dicParas["price"] + "") : string.Empty;
                string gameCode = dicParas.ContainsKey("gameCode") ? (dicParas["gameCode"] + "") : string.Empty;
                string lowLimit = dicParas.ContainsKey("lowLimit") ? (dicParas["lowLimit"] + "") : string.Empty;
                string highLimit = dicParas.ContainsKey("highLimit") ? (dicParas["highLimit"] + "") : string.Empty;
                string[] photoURLs = dicParas.ContainsKey("photoURLs") ? (string[])dicParas["photoURLs"]: null;
                var readCat = dicParas.Get("readCat").Toint();

                var freeLotteryRules = dicParas.GetArray("freeLotteryRules");
                var gamePointRules = dicParas.GetArray("gamePointRules");

                #region 参数验证

                if (string.IsNullOrEmpty(gameName))
                {
                    errMsg = "游戏机名称GameName不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!string.IsNullOrEmpty(id) && !Utils.isNumber(id))
                {
                    errMsg = "游戏机ID格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (gameType.Toint(0) == 0)
                {
                    errMsg = "游戏机类型gameType不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (string.IsNullOrEmpty(gameId))
                {
                    errMsg = "游戏机编号gameId不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (gameId.Length > 4)
                {
                    errMsg = "游戏机编号长度不能超过4个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!string.IsNullOrEmpty(area) && !Utils.IsDecimal(area))
                {
                    errMsg = "游戏机参数area格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!string.IsNullOrEmpty(evaluation) && !Utils.isNumber(evaluation))
                {
                    errMsg = "游戏机参数evaluation格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!string.IsNullOrEmpty(price) && !Utils.isNumber(price))
                {
                    errMsg = "游戏机参数price格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                
                if (!string.IsNullOrEmpty(lowLimit) && !Utils.IsDecimal(lowLimit))
                {
                    errMsg = "中奖概率下限参数lowLimit格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!string.IsNullOrEmpty(highLimit) && !Utils.IsDecimal(highLimit))
                {
                    errMsg = "中奖概率上限参数highLimit格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #endregion

                #region 游戏机参数验证
                if (!dicParas.Get("gameMode").Nonempty("gameMode参数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                var gameMode = dicParas.Get("gameMode").Toint(0);
                if (gameMode == 0)
                {
                    List<string> gameSimpleParameters = new List<string> { 
                    "PushBalanceIndex1","PushCoin1",
                    "PushLevel", "LotteryMode", "OnlyExitLottery", "ReadCat", 
                    //"ReadDelay", 
                    "chkCheckGift"
                    };

                    foreach (var parameter in gameSimpleParameters)
                    {
                        if (!dicParas.Get(parameter).Validint(parameter + "参数", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    List<string> gameAdvancedParameters = new List<string> { 
                    "PushBalanceIndex1","PushCoin1", "OutMode",
                    "ReturnCheck","OutsideAlertCheck","ICTicketOperation","NotGiveBack","LotteryMode","OnlyExitLottery","chkCheckGift","AllowElecPush","GuardConvertCard","ReadCat",
                    //"ReadDelay",
                    "AllowRealPush","BanOccupy","StrongGuardConvertCard",
                    "AllowElecOut","NowExit","BOLock","AllowRealOut","BOKeep","PushSpeed","PushPulse","PushLevel","PushStartInterval","UseSecondPush","SecondSpeed","OutBalanceIndex",
                    "SecondPulse","SecondLevel","SecondStartInterval","OutSpeed","OutPulse","CountLevel","OutLevel","OutReduceFromGame","OutAddToCard","OnceOutLimit","OncePureOutLimit","ExceptOutTest","ExceptOutSpeed","Frequency"
                    };

                    //是否启用从游戏机上分线上分
                    var userSecondPush = dicParas.Get("UseSecondPush").Toint();
                    if (userSecondPush == 0)
                    {
                        gameAdvancedParameters.Add("PushAddToGame1");
                        gameAdvancedParameters.Add("SecondAddToGame1");
                    }
                    else if (userSecondPush == 1)
                    {
                        gameAdvancedParameters.Add("PushAddToGame2");
                        gameAdvancedParameters.Add("SecondAddToGame2");
                    }
                    else
                    {
                        errMsg = "是否启用从游戏机上分线上分UseSecondPush参数值不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    foreach (var parameter in gameAdvancedParameters)
                    {
                        if (!dicParas.Get(parameter).Validint(parameter + "参数", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                             
                #endregion

                //开启EF事务
                var data_GameInfoService = Data_GameInfoService.I;
                var data_GameInfo_ExtService = Data_GameInfo_ExtService.I;
                var data_GameInfo_PhotoService = Data_GameInfo_PhotoService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        int iId = 0;
                        int.TryParse(id, out iId);                        

                        var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == iId).FirstOrDefault() ?? new Data_GameInfo();
                        if (data_GameInfoService.Any(a => a.ID != iId && a.GameID.Equals(gameId, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "该游戏机编号已使用";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!gameCode.IsNull() && data_GameInfo_ExtService.Any(b => b.GameID != iId && b.GameCode.Equals(gameCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "该游戏机出厂编号已使用";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //获取参数默认值
                        IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>();
                        int GameInfoId = dict_SystemService.GetModels(p => p.DictKey.Equals("游戏机档案维护")).FirstOrDefault().ID;
                        var result = dict_SystemService.GetModels(p => p.PID == GameInfoId && p.Enabled == 1).ToList();
                        foreach (var dict in result)
                        {
                            if (dicParas.ContainsKey(dict.DictKey))
                            {
                                dicParas[dict.DictKey] = dicParas[dict.DictKey] ?? dict.DictValue;
                            }
                            else
                            {
                                dicParas.Add(dict.DictKey, dict.DictValue);
                            }
                        }

                        Utils.GetModel(dicParas, ref data_GameInfo);

                        data_GameInfo.PushAddToGame1 = data_GameInfo.PushAddToGame1 ?? data_GameInfo.PushCoin1; //简易模式下“投币给游戏机脉冲数”与“单局投币数”相同
                        data_GameInfo.PushAddToGame2 = data_GameInfo.PushAddToGame2 ?? data_GameInfo.PushCoin1; //简易模式下“投币给游戏机脉冲数”与“单局投币数”相同
                        data_GameInfo.SSRTimeOut = data_GameInfo.SSRTimeOut ?? 0;
                        data_GameInfo.PushBalanceIndex2 = data_GameInfo.PushBalanceIndex2 ?? 0;
                        data_GameInfo.PushCoin2 = data_GameInfo.PushCoin2 ?? 0;
                        data_GameInfo.OutBalanceIndex = data_GameInfo.OutBalanceIndex ?? 0;
                        data_GameInfo.StoreID = storeId;
                        data_GameInfo.MerchID = merchId;

                        //有第二投币项刷卡即扣禁用
                        if (data_GameInfo.PushBalanceIndex2 > 0)
                        {
                            data_GameInfo.ReadCat = 0;

                            if (readCat != 0)
                            {
                                resultMsg = "存在第二投币项, 自动关闭“刷卡即扣”";
                            }                            
                        }

                        if (iId == 0)
                        {                            
                            data_GameInfo.State = 1;
                            if (!data_GameInfoService.Add(data_GameInfo, true, merchId, merchSecret))
                            {
                                errMsg = "新增游戏机信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (data_GameInfo.ID == 0)
                            {
                                errMsg = "该游戏机不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!data_GameInfoService.Update(data_GameInfo, true, merchId, merchSecret))
                            {
                                errMsg = "修改游戏机信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iId = data_GameInfo.ID;

                        //保存会员电子存票赠送设置
                        if (freeLotteryRules != null && freeLotteryRules.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_GameFreeLotteryRuleService.I.GetModels(p => p.GameIndex == iId))
                            {
                                Data_GameFreeLotteryRuleService.I.DeleteModel(model, true, merchId, merchSecret);
                            }

                            foreach (IDictionary<string, object> el in freeLotteryRules)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("memberLevelID").Validintnozero("会员级别ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("baseLottery").Validintnozero("彩票赠送基数", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("freeCount").Validintnozero("赠送彩票数", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var memberLevelID = dicPara.Get("memberLevelID").Toint();
                                    var baseLottery = dicPara.Get("baseLottery").Toint();
                                    var freeCount = dicPara.Get("freeCount").Toint();

                                    var data_GameFreeLotteryRule = new Data_GameFreeLotteryRule();
                                    data_GameFreeLotteryRule.MemberLevelID = memberLevelID;
                                    data_GameFreeLotteryRule.BaseLottery = baseLottery;
                                    data_GameFreeLotteryRule.FreeCount = freeCount;
                                    data_GameFreeLotteryRule.GameIndex = iId;
                                    data_GameFreeLotteryRule.MerchID = merchId;
                                    data_GameFreeLotteryRule.StoreID = storeId;
                                    Data_GameFreeLotteryRuleService.I.AddModel(data_GameFreeLotteryRule, true, merchId, merchSecret);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GameFreeLotteryRuleService.I.SaveChanges())
                            {
                                errMsg = "保存会员电子存票赠送设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //保存会员消费积分设置
                        if (gamePointRules != null && gamePointRules.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_Game_PointRuleService.I.GetModels(p => p.GameIndex == iId))
                            {
                                Data_Game_PointRuleService.I.DeleteModel(model, true, merchId, merchSecret);
                            }

                            foreach (IDictionary<string, object> el in gamePointRules)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("memberLevelID").Validintnozero("会员级别ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("state").Validint("是否启用", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("key1BalanceIndex").Validintnozero("按键一获赠类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("key1Count").Validintnozero("按键一获赠数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var memberLevelID = dicPara.Get("memberLevelID").Toint();
                                    var state = dicPara.Get("state").Toint();
                                    var key1BalanceIndex = dicPara.Get("key1BalanceIndex").Toint();
                                    var key1Count = dicPara.Get("key1Count").Toint();
                                    var key2BalanceIndex = dicPara.Get("key2BalanceIndex").Toint();
                                    var key2Count = dicPara.Get("key2Count").Toint();

                                    var data_Game_PointRule = new Data_Game_PointRule();
                                    data_Game_PointRule.MemberLevelID = memberLevelID;
                                    data_Game_PointRule.State = state;
                                    data_Game_PointRule.Key1BalanceIndex = key1BalanceIndex;
                                    data_Game_PointRule.Key1Count = key1Count;
                                    data_Game_PointRule.Key2BalanceIndex = key2BalanceIndex;
                                    data_Game_PointRule.Key2Count = key2Count;
                                    data_Game_PointRule.GameIndex = iId;
                                    data_Game_PointRule.MerchID = merchId;
                                    data_Game_PointRule.StoreID = storeId;
                                    Data_Game_PointRuleService.I.AddModel(data_Game_PointRule, true, merchId, merchSecret);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_Game_PointRuleService.I.SaveChanges())
                            {
                                errMsg = "保存会员消费积分设置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //保存游戏机扩展信息
                        foreach(var model in data_GameInfo_ExtService.GetModels(p=>p.GameID == iId))
                        {
                            model.ValidFlag = 0;
                            data_GameInfo_ExtService.UpdateModel(model, true, merchId, merchSecret);
                        }

                        var data_GameInfo_Ext = new Data_GameInfo_Ext();
                        Utils.GetModel(dicParas, ref data_GameInfo_Ext);
                        data_GameInfo_Ext.GameID = iId;
                        data_GameInfo_Ext.MerchID = merchId;
                        data_GameInfo_Ext.StoreID = storeId;                        
                        data_GameInfo_Ext.ValidFlag = 1;
                        data_GameInfo_ExtService.AddModel(data_GameInfo_Ext, true, merchId, merchSecret);
                        if (!data_GameInfo_ExtService.SaveChanges())
                        {
                            errMsg = "保存游戏机扩展信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //保存图片地址
                        if (photoURLs != null && photoURLs.Length >= 0)
                        {                            
                            foreach (var model in data_GameInfo_PhotoService.GetModels(p => p.GameID == iId))
                            {
                                data_GameInfo_PhotoService.DeleteModel(model, true, merchId, merchSecret);
                            }

                            foreach (string photoURL in photoURLs)
                            {
                                var data_GameInfo_Photo = new Data_GameInfo_Photo();
                                data_GameInfo_Photo.GameID = iId;
                                data_GameInfo_Photo.PhotoURL = photoURL;
                                data_GameInfo_Photo.UploadTime = DateTime.Now;
                                data_GameInfo_PhotoService.AddModel(data_GameInfo_Photo, true, merchId, merchSecret);
                            }

                            if (!data_GameInfo_PhotoService.SaveChanges())
                            {
                                errMsg = "保存图片地址失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //发送指令
                        var radarToken = "";
                        projectInsChange(new Guid("N").ToString(), storeId, "", data_GameInfo.ID.ToString(), out radarToken, out errMsg);
                        
                        ts.Complete();                        

                        return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, new { ID = iId, GameName = data_GameInfo.GameName }, resultMsg);
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
            }            
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object SaveGameInfoFromProgram(Dictionary<string, object> dicParas)
        {
            try
            {
                XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
                string storeId = userTokenModel.StoreId;
                string merchId = storeId.Substring(0, 6);
                string merchSecret = Base_MerchantInfoService.I.GetModels(p => p.ID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).Select(o => o.MerchSecret).FirstOrDefault();

                string errMsg = string.Empty;
                string resultMsg = string.Empty;
                string id = dicParas.ContainsKey("ID") ? (dicParas["ID"] + "") : string.Empty;
                string gameId = dicParas.ContainsKey("gameId") ? (dicParas["gameId"] + "") : string.Empty;
                string gameType = dicParas.ContainsKey("gameType") ? (dicParas["gameType"] + "") : string.Empty;
                string gameName = dicParas.ContainsKey("gameName") ? (dicParas["gameName"] + "") : string.Empty;
                var readCat = dicParas.Get("ReadCat").Toint();

                #region 参数验证
                if (string.IsNullOrEmpty(gameName))
                {
                    errMsg = "游戏机名称GameName不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(id) && !Utils.isNumber(id))
                {
                    errMsg = "游戏机参数ID格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (gameType.Toint(0) == 0)
                {
                    errMsg = "游戏机类型gameType不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    errMsg = "游戏机编号gameId不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (gameId.Length > 4)
                {
                    errMsg = "游戏机编号长度不能超过4个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                #endregion

                #region 游戏机参数验证
                if (!dicParas.Get("gameMode").Nonempty("gameMode参数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                var gameMode = dicParas.Get("gameMode").Toint(0);
                if (gameMode == 0)
                {
                    List<string> gameSimpleParameters = new List<string> { 
                    "PushBalanceIndex1","PushCoin1",
                    "PushLevel", "LotteryMode", "OnlyExitLottery", "ReadCat", /*"ReadDelay",*/ "chkCheckGift"
                    };

                    foreach (var parameter in gameSimpleParameters)
                    {
                        if (!dicParas.Get(parameter).Validint(parameter + "参数", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    List<string> gameAdvancedParameters = new List<string> { 
                    "PushBalanceIndex1","PushCoin1", "OutMode",
                    "ReturnCheck","OutsideAlertCheck","ICTicketOperation","NotGiveBack","LotteryMode","OnlyExitLottery","chkCheckGift","AllowElecPush","GuardConvertCard","ReadCat",/*"ReadDelay",*/"AllowRealPush","BanOccupy","StrongGuardConvertCard",
                    "AllowElecOut","NowExit","BOLock","AllowRealOut","BOKeep","PushSpeed","PushPulse","PushLevel","PushStartInterval","UseSecondPush","SecondSpeed","OutBalanceIndex",
                    "SecondPulse","SecondLevel","SecondStartInterval","OutSpeed","OutPulse","CountLevel","OutLevel","OutReduceFromGame","OutAddToCard","OnceOutLimit","OncePureOutLimit","ExceptOutTest","ExceptOutSpeed","Frequency"
                    };

                    //是否启用从游戏机上分线上分
                    var userSecondPush = dicParas.Get("UseSecondPush").Toint();
                    if (userSecondPush == 0)
                    {
                        gameAdvancedParameters.Add("PushAddToGame1");
                        gameAdvancedParameters.Add("SecondAddToGame1");
                    }
                    else if (userSecondPush == 1)
                    {
                        gameAdvancedParameters.Add("PushAddToGame2");
                        gameAdvancedParameters.Add("SecondAddToGame2");
                    }
                    else
                    {
                        errMsg = "是否启用从游戏机上分线上分UseSecondPush参数值不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    foreach (var parameter in gameAdvancedParameters)
                    {
                        if (!dicParas.Get(parameter).Validint(parameter + "参数", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                #endregion

                //开启EF事务
                var data_GameInfoService = Data_GameInfoService.I;
                var data_GameInfo_ExtService = Data_GameInfo_ExtService.I;
                var data_GameInfo_PhotoService = Data_GameInfo_PhotoService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        int iId = 0;
                        int.TryParse(id, out iId);

                        var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == iId).FirstOrDefault() ?? new Data_GameInfo();
                        if (data_GameInfoService.Any(a => a.ID != iId && a.GameID.Equals(gameId, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "该游戏机编号已使用";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //获取参数默认值
                        IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>();
                        int GameInfoId = dict_SystemService.GetModels(p => p.DictKey.Equals("游戏机档案维护")).FirstOrDefault().ID;
                        var result = dict_SystemService.GetModels(p => p.PID == GameInfoId && p.Enabled == 1).ToList();
                        foreach (var dict in result)
                        {
                            if (dicParas.ContainsKey(dict.DictKey))
                            {
                                dicParas[dict.DictKey] = dicParas[dict.DictKey] ?? dict.DictValue;
                            }
                            else
                            {
                                dicParas.Add(dict.DictKey, dict.DictValue);
                            }
                        }

                        Utils.GetModel(dicParas, ref data_GameInfo);

                        data_GameInfo.PushAddToGame1 = data_GameInfo.PushAddToGame1 ?? data_GameInfo.PushCoin1; //简易模式下“投币给游戏机脉冲数”与“单局投币数”相同
                        data_GameInfo.PushAddToGame2 = data_GameInfo.PushAddToGame2 ?? data_GameInfo.PushCoin1; //简易模式下“投币给游戏机脉冲数”与“单局投币数”相同
                        data_GameInfo.SSRTimeOut = data_GameInfo.SSRTimeOut ?? 0;
                        data_GameInfo.PushBalanceIndex2 = data_GameInfo.PushBalanceIndex2 ?? 0;
                        data_GameInfo.PushCoin2 = data_GameInfo.PushCoin2 ?? 0;
                        data_GameInfo.OutBalanceIndex = data_GameInfo.OutBalanceIndex ?? 0;
                        data_GameInfo.StoreID = storeId;
                        data_GameInfo.MerchID = merchId;

                        //有第二投币项刷卡即扣禁用
                        if (data_GameInfo.PushBalanceIndex2 > 0)
                        {
                            data_GameInfo.ReadCat = 0;

                            if (readCat != 0)
                            {
                                resultMsg = "存在第二投币项, 自动关闭“刷卡即扣”";
                            }
                        }

                        if (iId == 0)
                        {
                            data_GameInfo.State = 1;
                            if (!data_GameInfoService.Add(data_GameInfo, true, merchId, merchSecret))
                            {
                                errMsg = "新增游戏机信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (data_GameInfo.ID == 0)
                            {
                                errMsg = "该游戏机不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!data_GameInfoService.Update(data_GameInfo, true, merchId, merchSecret))
                            {
                                errMsg = "修改游戏机信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iId = data_GameInfo.ID;

                        //发送指令
                        var radarToken = "";
                        projectInsChange(new Guid("N").ToString(), storeId, "", iId.ToString(), out radarToken, out errMsg);

                        ts.Complete();                        

                        return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, new { ID = iId, GameName = data_GameInfo.GameName }, resultMsg);
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
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelGameInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("游戏机ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if (!id.Validintnozero("游戏机ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            int iId = Convert.ToInt32(id);
                            IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>();
                            if (!data_GameInfoService.Any(a => a.ID == iId))
                            {
                                errMsg = "该游戏机信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                            if (!data_GameInfoService.Delete(data_GameInfo, true, merchId, merchSecret))
                            {
                                errMsg = "删除游戏机信息失败";
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
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
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
        public object UploadGamePhoto(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                Dictionary<string, object> imageInfo = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                List<string> imageUrls = null;
                if (!Utils.UploadImageFile("/XCCloud/GameInfo/Photo/", out imageUrls, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                imageInfo.Add("ImageURL", imageUrls);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, imageInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameAppRule(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("gameId").Validintnozero("游戏机ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var gameId = dicParas.Get("gameId").Toint();

                var list = Data_GameAPP_RuleService.I.GetModels(p => p.GameID == gameId).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGameAppRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                if (!dicParas.Get("gameId").Validintnozero("游戏机ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var gameId = dicParas.Get("gameId").Toint();
                var gameAppRules = dicParas.GetArray("gameAppRules");
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {                        
                        //保存散客扫码规则
                        if (gameAppRules != null && gameAppRules.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_GameAPP_RuleService.I.GetModels(p => p.GameID == gameId))
                            {
                                Data_GameAPP_RuleService.I.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in gameAppRules)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("payCount").Validint("支付金额", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("playCount").Validdecimal("启动局数", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var payCount = dicPara.Get("payCount").Todecimal();
                                    var playCount = dicPara.Get("playCount").Toint();

                                    var data_GameAPP_Rule = new Data_GameAPP_Rule();
                                    data_GameAPP_Rule.MerchID = merchId;
                                    data_GameAPP_Rule.StoreID = storeId;
                                    data_GameAPP_Rule.GameID = gameId;
                                    data_GameAPP_Rule.PayCount = payCount;
                                    data_GameAPP_Rule.PlayCount = playCount;
                                    Data_GameAPP_RuleService.I.AddModel(data_GameAPP_Rule, true, merchId, merchSecret);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_GameAPP_RuleService.I.SaveChanges())
                            {
                                errMsg = "保存散客扫码规则失败";
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
        public object GetGameAppMemberRule(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("gameId").Validintnozero("游戏机ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var gameId = dicParas.Get("gameId").Toint();

                var list = (from a in Data_GameAPP_MemberRuleService.N.GetModels(p => p.GameID == gameId)
                            join b in Data_MemberLevelService.N.GetModels() on a.MemberLevelID equals b.ID into b1
                            from b in b1.DefaultIfEmpty()
                            join c in Dict_BalanceTypeService.N.GetModels() on a.PushBalanceIndex1 equals c.ID into c1
                            from c in c1.DefaultIfEmpty()
                            join d in Dict_BalanceTypeService.N.GetModels() on a.PushBalanceIndex2 equals d.ID into d1
                            from d in d1.DefaultIfEmpty()
                            select new
                            {
                                a = a,
                                MemberLevelName = b != null ? b.MemberLevelName : string.Empty,
                                PushBalanceName1 = c != null ? c.TypeName : string.Empty,
                                PushBalanceName2 = d != null ? d.TypeName : string.Empty
                            }).AsFlatDictionaryList();


                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGameAppMemberRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                if (!dicParas.Get("gameId").Validintnozero("游戏机ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("memberLevelId").Validintnozero("会员级别ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("gameAppMemberRule").Nonempty("会员扫码规则信息", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var gameId = dicParas.Get("gameId").Toint();
                var memberLevelId = dicParas.Get("memberLevelId").Toint();
                var gameAppMemberRule = dicParas.GetObject("gameAppMemberRule");

                //保存会员扫码规则
                if (gameAppMemberRule != null)
                {
                    var dicPara = new Dictionary<string, object>((IDictionary<string, object>)gameAppMemberRule, StringComparer.OrdinalIgnoreCase);
                    if (!dicPara.Get("pushBalanceIndex1").Validint("投币种1", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicPara.Get("pushCoin1").Validdecimal("投币数量1", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicPara.Get("pushBalanceIndex2").Validint("投币种2", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicPara.Get("pushCoin2").Validdecimal("投币数量2", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicPara.Get("playCount").Validint("启动局数", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                    var id = dicPara.Get("id").Toint(0);
                    var pushBalanceIndex1 = dicPara.Get("pushBalanceIndex1").Toint();
                    var pushCoin1 = dicPara.Get("pushCoin1").Todecimal();
                    var pushBalanceIndex2 = dicPara.Get("pushBalanceIndex2").Toint();
                    var pushCoin2 = dicPara.Get("pushCoin2").Todecimal();
                    var playCount = dicPara.Get("playCount").Toint();

                    if (pushBalanceIndex1 == pushBalanceIndex2)
                    {
                        errMsg = "投币种1、2不能相同";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (Data_GameAPP_MemberRuleService.I.Any(a => a.ID != id && a.GameID == gameId && a.MemberLevelID == memberLevelId && a.PushBalanceIndex1 == pushBalanceIndex1 && a.PushBalanceIndex2 == pushBalanceIndex2))
                    {
                        errMsg = "相同种类投币规则不能重复";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    var data_GameAPP_MemberRule = Data_GameAPP_MemberRuleService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GameAPP_MemberRule();
                    data_GameAPP_MemberRule.MerchID = merchId;
                    data_GameAPP_MemberRule.StoreID = storeId;
                    data_GameAPP_MemberRule.GameID = gameId;
                    data_GameAPP_MemberRule.MemberLevelID = memberLevelId;
                    data_GameAPP_MemberRule.PushBalanceIndex1 = pushBalanceIndex1;
                    data_GameAPP_MemberRule.PushCoin1 = pushCoin1;
                    data_GameAPP_MemberRule.PushBalanceIndex2 = pushBalanceIndex2;
                    data_GameAPP_MemberRule.PushCoin2 = pushCoin2;
                    data_GameAPP_MemberRule.PlayCount = playCount;

                    if (id == 0)
                    {
                        if (!Data_GameAPP_MemberRuleService.I.Add(data_GameAPP_MemberRule, true, merchId, merchSecret))
                        {
                            errMsg = "保存会员扫码规则失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        id = data_GameAPP_MemberRule.ID;
                    }
                    else
                    {
                        if (data_GameAPP_MemberRule.ID == 0)
                        {
                            errMsg = "该会员扫码规则信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_GameAPP_MemberRuleService.I.Update(data_GameAPP_MemberRule, true, merchId, merchSecret))
                        {
                            errMsg = "保存会员扫码规则失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                    }

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, new { ID = id });
                }
                else
                {
                    errMsg = "提交数据包含空对象";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelGameAppMemberRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("会员扫码规则ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if (!id.Validintnozero("会员扫码规则ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            if (!Data_GameAPP_MemberRuleService.I.Any(a => a.ID == (int)id))
                            {
                                errMsg = "该会员扫码规则信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var data_GameAPP_MemberRule = Data_GameAPP_MemberRuleService.I.GetModels(p => p.ID == (int)id).FirstOrDefault();

                            if (!Data_GameAPP_MemberRuleService.I.Delete(data_GameAPP_MemberRule, true, merchId, merchSecret))
                            {
                                errMsg = "删除会员扫码规则失败";
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
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
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