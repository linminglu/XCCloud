using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using System.Transactions;
using System.Data.Entity.SqlServer;
using XCCloudService.Common.Extensions;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Merches = "Normal,Heavy")]
    /// <summary>
    /// Parameter 的摘要说明
    /// </summary>
    public class Parameter : ApiBase
    {
        private bool checkParams(Dictionary<string, object> dicParas, out string errMsg)
        {
            errMsg = string.Empty;
            string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
            string system = dicParas.ContainsKey("system") ? dicParas["system"].ToString() : string.Empty;
            string pValue = dicParas.ContainsKey("pValue") ? dicParas["pValue"].ToString() : string.Empty;
            string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;
            string isAllow = dicParas.ContainsKey("isAllow") ? dicParas["isAllow"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(storeId))
            {
                errMsg = "门店编号不能为空";
                return false;
            }

            if (!string.IsNullOrEmpty(system) && system.Length > 30)
            {
                errMsg = "控件名称不能超过30个字符";
                return false;
            }

            if (!string.IsNullOrEmpty(pValue) && pValue.Length > 200)
            {
                errMsg = "参数值不能超过200个字符";
                return false;
            }

            if (!string.IsNullOrEmpty(note) && note.Length > 200)
            {
                errMsg = "参数说明不能超过200个字符";
                return false;
            }

            if (!string.IsNullOrEmpty(isAllow))
            {
                try
                {
                    Convert.ToInt32(isAllow);
                }
                catch
                {
                    errMsg = "是否启用数据格式转换异常";
                    return false;
                }
            }

            return true;
        }

        private bool checkGbRules(Dictionary<string, object> dicParas, out string errMsg)
        {
            errMsg = string.Empty;
            string memberLvlId = dicParas.ContainsKey("memberLvlId") ? dicParas["memberLvlId"].ToString() : string.Empty;
            string backMin = dicParas.ContainsKey("backMin") ? dicParas["backMin"].ToString() : string.Empty;
            string backMax = dicParas.ContainsKey("backMax") ? dicParas["backMax"].ToString() : string.Empty;
            string backScale = dicParas.ContainsKey("backScale") ? dicParas["backScale"].ToString() : string.Empty;
            string exitCardMin = dicParas.ContainsKey("exitCardMin") ? dicParas["exitCardMin"].ToString() : string.Empty;
            string allowBackPrincipal = dicParas.ContainsKey("allowBackPrincipal") ? dicParas["allowBackPrincipal"].ToString() : string.Empty;
            string backType = dicParas.ContainsKey("backType") ? dicParas["backType"].ToString() : string.Empty;
            string totalDays = dicParas.ContainsKey("totalDays") ? dicParas["totalDays"].ToString() : string.Empty;
            string allowContainToday = dicParas.ContainsKey("allowContainToday") ? dicParas["allowContainToday"].ToString() : string.Empty;

            //会员级别
            if (string.IsNullOrEmpty(memberLvlId))
            {
                errMsg = "会员级别不能为空";
                return false;
            }

            if (!Utils.isNumber(memberLvlId))
            {
                errMsg = "会员级别数据格式转换异常";
                return false;
            }

            //最小币数
            if (!string.IsNullOrEmpty(backMin) && !Utils.isNumber(backMin))
            {
                errMsg = "最小币数数据格式转换异常";
                return false;
            }

            //最大币数
            if (!string.IsNullOrEmpty(backMax) && !Utils.isNumber(backMax))
            {
                errMsg = "最大币数数据格式转换异常";
                return false;
            }

            //返还比例
            if (!string.IsNullOrEmpty(backScale) && !Utils.isNumber(backScale))
            {
                errMsg = "返回比例数据格式转换异常";
                return false;
            }

            //退卡时最少币数
            if (!string.IsNullOrEmpty(exitCardMin) && !Utils.isNumber(exitCardMin))
            {
                errMsg = "退卡时最少币数数据格式转换异常";
                return false;
            }

            //是否扣除返回币
            if (string.IsNullOrEmpty(allowBackPrincipal))
            {
                errMsg = "是否扣除返回币参数不能为空";
                return false;
            }

            if(!Utils.isNumber(allowBackPrincipal))
            {
                errMsg = "是否扣除返回币数据格式转换异常";
                return false;
            }
            
            //返还计算方式
            if (string.IsNullOrEmpty(backType))
            {
                errMsg = "返还计算方式参数不能为空";
                return false;
            }

            if (!Utils.isNumber(backType))
            {
                errMsg = "返还计算方式数据格式转换异常";
                return false;
            }

            //累计天数
            if (!string.IsNullOrEmpty(totalDays) && !Utils.isNumber(totalDays))
            {
                errMsg = "累计天数数据格式转换异常";
                return false;
            }

            //是否统计当天
            if (string.IsNullOrEmpty(allowContainToday))
            {
                errMsg = "是否统计当天参数不能为空";
                return false;
            }

            if (!Utils.isNumber(allowContainToday))
            {
                errMsg = "是否统计当天数据格式转换异常";
                return false;
            }

            return true;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        private object GetParam(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string system = dicParas.ContainsKey("system") ? dicParas["system"].ToString() : string.Empty;

                //验证参数信息
                if (!checkParams(dicParas, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(system))
                {
                    errMsg = "控件名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IData_ParametersService data_ParameterService = BLLContainer.Resolve<IData_ParametersService>();
                var data_Parameter = data_ParameterService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.System.Equals(system, StringComparison.OrdinalIgnoreCase)).ToList();

                var count = data_Parameter.Count;
                if (count == 0)
                {
                    errMsg = "参数" + system + "数据库不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (count > 1)
                {
                    errMsg = "参数" + system + "数据库存在多个";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_ParameterModel = data_Parameter.FirstOrDefault();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_ParameterModel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }            
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetParamList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;

                //验证参数信息
                if (!checkParams(dicParas, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var storeTag = Base_StoreInfoService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)).Select(o => o.StoreTag).FirstOrDefault() ?? 0;
                var paramId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("运营参数设定", StringComparison.OrdinalIgnoreCase) && p.PID == 0).FirstOrDefault().ID;
                var data_Parameter = from a in Dict_SystemService.N.GetModels(p => p.PID == paramId && p.MerchID == (storeTag + ""))
                                     join b in Data_ParametersService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.DictKey equals b.System into b1
                                     from b in b1.DefaultIfEmpty()
                                     select new 
                                     {
                                         ID = b != null ? b.ID : 0,
                                         StoreID = b != null ? b.StoreID : string.Empty,
                                         System = a.DictKey,
                                         ParameterName = a.Comment,
                                         IsAllow = b != null ? b.IsAllow : a.Enabled,
                                         ParameterValue = b != null ? b.ParameterValue : a.DictValue,
                                         Note = b != null ? b.Note : string.Empty,
                                     };
               
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_Parameter);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        private object SetParam(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string system = dicParas.ContainsKey("system") ? dicParas["system"].ToString() : string.Empty;
                string isAllow = dicParas.ContainsKey("isAllow") ? dicParas["isAllow"].ToString() : string.Empty;
                string pValue = dicParas.ContainsKey("pValue") ? dicParas["pValue"].ToString() : string.Empty;
                string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;

                //验证参数信息
                if (!checkParams(dicParas, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(system))
                {
                    errMsg = "控件名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_Parameter = Data_ParametersService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.System.Equals(system, StringComparison.OrdinalIgnoreCase)).ToList();                
                int count = data_Parameter.Count;
                if (count > 1)
                {
                    errMsg = "参数" + system + "数据库存在多个";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (count == 0)
                {
                    //errMsg = "参数" + system + "数据库不存在";
                    //return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    var data_ParameterModel = new Data_Parameters();
                    data_ParameterModel.StoreID = storeId;
                    data_ParameterModel.System = system;
                    data_ParameterModel.IsAllow = !string.IsNullOrEmpty(isAllow) ? Convert.ToInt32(isAllow) : (int?)null;
                    data_ParameterModel.ParameterValue = pValue;
                    data_ParameterModel.Note = note;
                    if (!Data_ParametersService.I.Add(data_ParameterModel))
                    {
                        errMsg = "设置参数失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    var data_ParameterModel = data_Parameter.FirstOrDefault();
                    data_ParameterModel.IsAllow = !string.IsNullOrEmpty(isAllow) ? Convert.ToInt32(isAllow) : (int?)null;
                    data_ParameterModel.ParameterValue = pValue;
                    data_ParameterModel.Note = note;
                    if (!Data_ParametersService.I.Update(data_ParameterModel))
                    {
                        errMsg = "设置参数失败";
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
        public object SetParamList(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string rc = string.Empty;

            //开启EF事务
            using (TransactionScope ts = new TransactionScope())
            {
                try
                {
                    var submitData = (object[])dicParas["submitData"];
                    foreach (IDictionary<string, object> el in submitData)
                    {
                        if (el != null)
                        {
                            var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                            var obj = SetParam(dicPara);
                            rc = obj is NoSignKeyResponseModel ? ((NoSignKeyResponseModel)obj).Result_Code : obj is ResponseModel ? ((ResponseModel)obj).Result_Code : string.Empty;
                            errMsg = obj is NoSignKeyResponseModel ? ((NoSignKeyResponseModel)obj).Result_Msg : obj is ResponseModel ? ((ResponseModel)obj).Result_Msg : string.Empty;
                            if (string.IsNullOrEmpty(rc))
                            {
                                errMsg = "不支持的返回类型";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                            else if (rc == "0")
                            {
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }                            
                        }
                        else
                        {
                            errMsg = "提交数据包含空对象";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                    }

                    ts.Complete();

                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
                }
                catch (Exception e)
                {
                    return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
                }
            }            
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGivebackRules(Dictionary<string, object> dicParas)
        {
            try
            { 
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var data_GivebackRuleList = (from a in Data_GivebackRuleService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                             join b in Data_MemberLevelService.N.GetModels(p => p.State == 1) on a.MemberLevelID equals b.MemberLevelID into b1
                                             from b in b1.DefaultIfEmpty()
                                             select new
                                             {
                                                 a = a,
                                                 MemberLevelName = b != null ? b.MemberLevelName : string.Empty
                                             }).ToList().AsFlatDictionaryList();
                                            //select new {
                                            //    ID = a.ID,
                                            //    MemberLevelID = a.MemberLevelID,
                                            //    MemberLevelName = b != null ? b.MemberLevelName : string.Empty,
                                            //    AllowBackPrincipal = a.AllowBackPrincipal,
                                            //    Backtype = a.Backtype,
                                            //    AllowContainToday = a.AllowContainToday,
                                            //    BackMin = a.BackMin,
                                            //    BackMax = a.BackMax,
                                            //    BackScale = a.BackScale,
                                            //    ExitCardMin = a.ExitCardMin,
                                            //    TotalDays = a.TotalDays
                                            //};

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_GivebackRuleList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGivebackRuleInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);

                if (id == 0)
                {
                    errMsg = "返还规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_GivebackRule = Data_GivebackRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (data_GivebackRule == null)
                {
                    errMsg = "该返还规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GivebackRule);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGivebackRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);                
                var memberLvlId = dicParas.Get("memberLvlId").Toint();                                
                string allowBackPrincipal = dicParas.ContainsKey("allowBackPrincipal") ? dicParas["allowBackPrincipal"].ToString() : string.Empty;
                string backType = dicParas.ContainsKey("backType") ? dicParas["backType"].ToString() : string.Empty;                
                string allowContainToday = dicParas.ContainsKey("allowContainToday") ? dicParas["allowContainToday"].ToString() : string.Empty;
                string backMin = dicParas.ContainsKey("backMin") ? dicParas["backMin"].ToString() : string.Empty;
                string backMax = dicParas.ContainsKey("backMax") ? dicParas["backMax"].ToString() : string.Empty;
                string backScale = dicParas.ContainsKey("backScale") ? dicParas["backScale"].ToString() : string.Empty;
                string exitCardMin = dicParas.ContainsKey("exitCardMin") ? dicParas["exitCardMin"].ToString() : string.Empty;
                string totalDays = dicParas.ContainsKey("totalDays") ? dicParas["totalDays"].ToString() : string.Empty;

                //验证参数
                if (!checkGbRules(dicParas, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                Data_GivebackRule data_GivebackRule = Data_GivebackRuleService.I.GetModels(p=>p.ID == id).FirstOrDefault() ?? new Data_GivebackRule();
                data_GivebackRule.MerchID = merchId;
                data_GivebackRule.MemberLevelID = memberLvlId;
                data_GivebackRule.AllowBackPrincipal = Convert.ToInt32(allowBackPrincipal);
                data_GivebackRule.Backtype = Convert.ToInt32(backType);
                data_GivebackRule.AllowContainToday = Convert.ToInt32(allowContainToday);
                data_GivebackRule.BackMin = !string.IsNullOrEmpty(backMin) ? Convert.ToInt32(dicParas["backMin"]) : (int?)null;
                data_GivebackRule.BackMax = !string.IsNullOrEmpty(backMax) ? Convert.ToInt32(dicParas["backMax"]) : (int?)null;
                data_GivebackRule.BackScale = !string.IsNullOrEmpty(backScale) ? Convert.ToInt32(dicParas["backScale"]) : (int?)null;
                data_GivebackRule.ExitCardMin = !string.IsNullOrEmpty(exitCardMin) ? Convert.ToInt32(dicParas["exitCardMin"]) : (int?)null;
                data_GivebackRule.TotalDays = !string.IsNullOrEmpty(totalDays) ? Convert.ToInt32(dicParas["totalDays"]) : (int?)null;

                if (Data_GivebackRuleService.I.Any(a => a.ID != id && a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && a.MemberLevelID == memberLvlId))
                {
                    errMsg = "该会员级别的返还规则已存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (id == 0)
                {                    
                    if (!Data_GivebackRuleService.I.Add(data_GivebackRule))
                    {
                        errMsg = "添加返还规则失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    if (!Data_GivebackRuleService.I.Any(a => a.ID == id))
                    {
                        errMsg = "该返还规则不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Data_GivebackRuleService.I.Update(data_GivebackRule))
                    {
                        errMsg = "更新返还规则失败";
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
        public object DelGivebackRuleInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);

                if (id == 0)
                {
                    errMsg = "返还规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_GivebackRule = Data_GivebackRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (data_GivebackRule == null)
                {
                    errMsg = "该返还规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Data_GivebackRuleService.I.Delete(data_GivebackRule))
                {
                    errMsg = "删除返还规则失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_GivebackRule);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }                

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetStandardCoinPrice(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;

                var linq = from a in Data_StandardCoinPriceService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                           join b in Dict_BalanceTypeService.N.GetModels(p => p.State == 1) on a.BalanceIndex equals b.ID into b1
                           from b in b1.DefaultIfEmpty()
                           select new
                           {
                               ID = a.ID,
                               BalanceIndex = a.BalanceIndex,
                               BalanceIndexStr = b != null ? b.TypeName : string.Empty,
                               CoinCount = a.CoinCount,
                               CashPrice = a.CashPrice
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveStandardCoinPrice(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;                

                string errMsg = string.Empty;
                if (!dicParas.Get("storeId").Nonempty("门店ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var storeId = dicParas.Get("storeId");
                var standardCoinPrice = dicParas.GetArray("standardCoinPrice");
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (standardCoinPrice != null && standardCoinPrice.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_StandardCoinPriceService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                            {
                                Data_StandardCoinPriceService.I.DeleteModel(model);
                            }

                            var data_StandardCoinPriceList = new List<Data_StandardCoinPrice>();
                            foreach (IDictionary<string, object> el in standardCoinPrice)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("balanceIndex").Validintnozero("余额类别ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("coinCount").Validintnozero("数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("cashPrice").Validdecimalnozero("现金价值", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var balanceIndex = dicPara.Get("balanceIndex").Toint();
                                    var coinCount = dicPara.Get("coinCount").Toint();
                                    var cashPrice = dicPara.Get("cashPrice").Todecimal();

                                    var data_StandardCoinPrice = new Data_StandardCoinPrice();
                                    data_StandardCoinPrice.MerchID = merchId;
                                    data_StandardCoinPrice.StoreID = storeId;
                                    data_StandardCoinPrice.BalanceIndex = balanceIndex;
                                    data_StandardCoinPrice.CoinCount = coinCount;
                                    data_StandardCoinPrice.CashPrice = cashPrice;
                                    data_StandardCoinPriceList.Add(data_StandardCoinPrice);
                                    Data_StandardCoinPriceService.I.AddModel(data_StandardCoinPrice);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return false;
                                }
                            }

                            if (data_StandardCoinPriceList.GroupBy(g => g.BalanceIndex).Select(o => new { Count = o.Count() }).Any(p => p.Count > 1))
                            {
                                errMsg = "同一余额类别的正价币规则必须唯一";
                                return false;
                            }

                            if (!Data_StandardCoinPriceService.I.SaveChanges())
                            {
                                errMsg = "保存正价币规则信息失败";
                                return false;
                            }
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
        public object GetFreeCoinRule(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;

                var linq = from a in Data_FreeCoinRuleService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                           join b in Dict_BalanceTypeService.N.GetModels(p => p.State == 1) on a.BalanceIndex equals b.ID into b1
                           from b in b1.DefaultIfEmpty()
                           select new
                           {
                               ID = a.ID,
                               BalanceIndex = a.BalanceIndex,
                               BalanceIndexStr = b != null ? b.TypeName : string.Empty,
                               OnceSigleMax = a.OnceSigleMax,
                               OnceWarningValue = a.OnceWarningValue,
                               DayMax = a.DayMax,
                               DayWaringValue = a.DayWarningValue
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveFreeCoinRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("storeId").Nonempty("门店ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var storeId = dicParas.Get("storeId");
                var freeCoinRule = dicParas.GetArray("freeCoinRule");

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (freeCoinRule != null && freeCoinRule.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_FreeCoinRuleService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                            {
                                Data_FreeCoinRuleService.I.DeleteModel(model);
                            }

                            var data_FreeCoinRuleList = new List<Data_FreeCoinRule>();
                            foreach (IDictionary<string, object> el in freeCoinRule)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("balanceIndex").Validintnozero("余额类别ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("onceSigleMax").Validintnozero("单次最大额度", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("onceWarningValue").Validintnozero("单次警戒值", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("dayMax").Validintnozero("每天最大额度", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("dayWarningValue").Validintnozero("每天警戒值", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var balanceIndex = dicPara.Get("balanceIndex").Toint();
                                    var onceSigleMax = dicPara.Get("onceSigleMax").Toint();
                                    var onceWarningValue = dicPara.Get("onceWarningValue").Toint();
                                    var dayMax = dicPara.Get("dayMax").Toint();
                                    var dayWarningValue = dicPara.Get("dayWarningValue").Toint();

                                    var data_FreeCoinRule = new Data_FreeCoinRule();
                                    data_FreeCoinRule.MerchID = merchId;
                                    data_FreeCoinRule.StoreID = storeId;
                                    data_FreeCoinRule.BalanceIndex = balanceIndex;
                                    data_FreeCoinRule.OnceSigleMax = onceSigleMax;
                                    data_FreeCoinRule.OnceWarningValue = onceWarningValue;
                                    data_FreeCoinRule.DayMax = dayMax;
                                    data_FreeCoinRule.DayWarningValue = dayWarningValue;
                                    data_FreeCoinRuleList.Add(data_FreeCoinRule);
                                    Data_FreeCoinRuleService.I.AddModel(data_FreeCoinRule);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return false;
                                }
                            }

                            if (data_FreeCoinRuleList.GroupBy(g => g.BalanceIndex).Select(o => new { Count = o.Count() }).Any(p => p.Count > 1))
                            {
                                errMsg = "同一余额类别的招待币补币规则必须唯一";
                                return false;
                            }

                            if (!Data_FreeCoinRuleService.I.SaveChanges())
                            {
                                errMsg = "保存招待币补币规则信息失败";
                                return false;
                            }
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
    }
}