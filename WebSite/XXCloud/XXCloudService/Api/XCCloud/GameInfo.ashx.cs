using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;

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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                var data_GameInfo = from a in Data_GameInfoService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.State == 1)
                                    join b in Dict_SystemService.N.GetModels() on a.GameType equals (b.ID + "") into b1
                                    from b in b1.DefaultIfEmpty()
                                    join c in Data_GameInfo_ExtService.N.GetModels(p => p.ValidFlag == 1) on a.ID equals c.GameID into c1
                                    from c in c1.DefaultIfEmpty()
                                    orderby a.GameID
                                    select new
                                    {
                                        ID = a.ID,
                                        GameID = a.GameID,
                                        GameName = a.GameName,
                                        GameTypeStr = b != null ? b.DictKey : string.Empty,
                                        Area = c != null ? c.Area : (decimal?)null,
                                        //ChangeTime = c != null ? SqlFunctions.DateName("yyyy", c.ChangeTime) + "-" + SqlFunctions.DateName("mm", c.ChangeTime) + "-" + SqlFunctions.DateName("dd", c.ChangeTime) + " " + 
                                        //                        SqlFunctions.DateName("hh", c.ChangeTime) + ":" + SqlFunctions.DateName("n", c.ChangeTime) + ":" + SqlFunctions.DateName("ss", c.ChangeTime) : string.Empty,
                                        ChangeTime = c != null ? c.ChangeTime : (DateTime?)null,
                                        Price = c != null ? c.Price : (int?)null,
                                        //PushReduceFromCard = a.PushReduceFromCard,
                                        PushCoin1 = a.PushCoin1,
                                        AllowElecPushStr = a.AllowElecPush != null ? (a.AllowElecPush == 1 ? "启用" : "禁用") : "",
                                        LotteryModeStr = a.LotteryMode != null ? (a.LotteryMode == 1 ? "启用" : "禁用") : "",
                                        ReadCatStr = a.ReadCat != null ? (a.ReadCat == 1 ? "启用" : "禁用") : "",
                                        StateStr = a.State != null ? (a.State == 1 ? "启用" : "禁用") : ""
                                    };
                                    
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_GameInfo);
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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                
                Dictionary<int, string> gameInfo = Data_GameInfoService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.State == 1).Select(o => new
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
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "游戏机ID参数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = 0;
                int.TryParse(id, out iId);
                IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
                var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == iId).FirstOrDefault() ?? new Data_GameInfo();
                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
                int GameInfoId = dict_SystemService.GetModels(p => p.DictKey.Equals("游戏机档案维护")).FirstOrDefault().ID;
                var result = (from a in data_GameInfo.AsDictionary()
                              join b in dict_SystemService.GetModels(p => p.PID == GameInfoId) on a.Key equals b.DictKey into b1
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
                                       join b in Data_MemberLevelService.N.GetModels() on a.MemberLevelID equals b.MemberLevelID into b1
                                       from b in b1.DefaultIfEmpty()
                                       select new 
                                       {
                                           MemberLevelID = a.MemberLevelID,
                                           MemberLevelName = b != null ? b.MemberLevelName : string.Empty,
                                           BaseLottery = a.BaseLottery,
                                           FreeCount = a.FreeCount
                                       };
                result.Add(new { name = "FreeLotteryRules", value = (object)FreeLotteryRules.ToList(), comment = string.Empty });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);                            
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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("ID") ? (dicParas["ID"] + "") : string.Empty;
                string gameId = dicParas.ContainsKey("gameId") ? (dicParas["gameId"] + "") : string.Empty;
                string gameName = dicParas.ContainsKey("GameName") ? (dicParas["GameName"] + "") : string.Empty;
                string area = dicParas.ContainsKey("area") ? (dicParas["area"] + "") : string.Empty;
                string changeTime = dicParas.ContainsKey("changeTime") ? (dicParas["changeTime"] + "") : string.Empty;
                string evaluation = dicParas.ContainsKey("evaluation") ? (dicParas["evaluation"] + "") : string.Empty;
                string price = dicParas.ContainsKey("price") ? (dicParas["price"] + "") : string.Empty;
                string gameCode = dicParas.ContainsKey("gameCode") ? (dicParas["gameCode"] + "") : string.Empty;
                string lowLimit = dicParas.ContainsKey("lowLimit") ? (dicParas["lowLimit"] + "") : string.Empty;
                string highLimit = dicParas.ContainsKey("highLimit") ? (dicParas["highLimit"] + "") : string.Empty;
                string[] photoURLs = dicParas.ContainsKey("photoURLs") ? (string[])dicParas["photoURLs"]: null;
                var freeLotteryRules = dicParas.GetArray("freeLotteryRules");

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
                if (!string.IsNullOrEmpty(gameId) && gameId.Length > 4)
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
                if (string.IsNullOrEmpty(gameCode))
                {
                    errMsg = "游戏机出厂编号gameCode不能为空";
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

                //开启EF事务
                var data_GameInfoService = Data_GameInfoService.I;
                var data_GameInfo_ExtService = Data_GameInfo_ExtService.I;
                var data_GameInfo_PhotoService = Data_GameInfo_PhotoService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var data_GameInfo = new Data_GameInfo();
                        int iId = 0;
                        int.TryParse(id, out iId);                        
                        if (data_GameInfo_ExtService.Any(a => a.GameID != iId && a.GameCode.Equals(gameCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "该游戏机出厂编号已使用";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (iId == 0)
                        {
                            Utils.GetModel(dicParas, ref data_GameInfo);
                            data_GameInfo.State = 1;
                            data_GameInfo.StoreID = storeId;
                            data_GameInfo.MerchID = merchId;
                            if (!data_GameInfoService.Add(data_GameInfo))
                            {
                                errMsg = "新增游戏机信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!data_GameInfoService.Any(a => a.ID == iId))
                            {
                                errMsg = "该游戏机不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                            
                            data_GameInfo = data_GameInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                            Utils.GetModel(dicParas, ref data_GameInfo);
                            if (!data_GameInfoService.Update(data_GameInfo))
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
                                Data_GameFreeLotteryRuleService.I.DeleteModel(model);
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
                                    Data_GameFreeLotteryRuleService.I.AddModel(data_GameFreeLotteryRule);
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

                        //保存游戏机扩展信息
                        foreach(var model in data_GameInfo_ExtService.GetModels(p=>p.GameID == iId))
                        {
                            model.ValidFlag = 0;
                            data_GameInfo_ExtService.UpdateModel(model);
                        }

                        var data_GameInfo_Ext = new Data_GameInfo_Ext();
                        data_GameInfo_Ext.Area = area.Todecimal();
                        data_GameInfo_Ext.ChangeTime = changeTime.Todatetime();
                        data_GameInfo_Ext.Evaluation = evaluation.Toint();
                        data_GameInfo_Ext.Price = price.Toint();
                        data_GameInfo_Ext.LowLimit = lowLimit.Todecimal();
                        data_GameInfo_Ext.HighLimit = highLimit.Todecimal();
                        data_GameInfo_Ext.GameCode = gameCode;
                        data_GameInfo_Ext.GameID = iId;
                        data_GameInfo_Ext.MerchID = merchId;
                        data_GameInfo_Ext.StoreID = storeId;                        
                        data_GameInfo_Ext.ValidFlag = 1;
                        data_GameInfo_ExtService.AddModel(data_GameInfo_Ext);
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
                                data_GameInfo_PhotoService.DeleteModel(model);
                            }

                            foreach (string photoURL in photoURLs)
                            {
                                var data_GameInfo_Photo = new Data_GameInfo_Photo();
                                data_GameInfo_Photo.GameID = iId;
                                data_GameInfo_Photo.PhotoURL = photoURL;
                                data_GameInfo_Photo.UploadTime = DateTime.Now;
                                data_GameInfo_PhotoService.AddModel(data_GameInfo_Photo);
                            }

                            if (!data_GameInfo_PhotoService.SaveChanges())
                            {
                                errMsg = "保存图片地址失败";
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
        public object DelGameInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "游戏机流水号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);
                IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>();
                if (!data_GameInfoService.Any(a => a.ID == iId))
                {
                    errMsg = "该游戏机信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_GameInfo = data_GameInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                data_GameInfo.State = 0;
                if (!data_GameInfoService.Update(data_GameInfo))
                {
                    errMsg = "删除游戏机信息失败";
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
                
                var id = dicParas.Get("id").Toint();

                var list = Data_GameAPP_RuleService.I.GetModels(p => p.GameID == id);

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
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

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
                                    Data_GameAPP_RuleService.I.AddModel(data_GameAPP_Rule);
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
    }
}