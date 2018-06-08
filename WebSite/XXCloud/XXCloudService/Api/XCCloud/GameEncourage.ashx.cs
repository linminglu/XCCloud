using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Common.Extensions;
using System.Data.SqlClient;
using System.Transactions;
using System.Data.Entity.Validation;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// GameEncourage 的摘要说明
    /// </summary>
    public class GameEncourage : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGameEncourage(Dictionary<string, object> dicParas)
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
                                    a.ID, b.GameName, a.State,
                                    ((case when ISNULL(a.StartDate,'')='' then '' else convert(varchar,a.StartDate,23) end) + '~' +
                                    (case when ISNULL(a.EndDate,'')='' then '' else convert(varchar,a.EndDate,23) end)) AS ValidDate,
                                    a.Note
                                FROM
                                	Data_GameEncourage a
                                LEFT JOIN 
                                    Data_GameInfo b ON a.GameIndex = b.ID                                
                                WHERE 1=1
                            ";
                sql += " AND a.StoreID=" + storeId;

                var list = Data_PushRuleService.I.SqlQuery<Data_GameEncourageList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var model = Data_GameEncourageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (model == null)
                {
                    errMsg = "该规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("gameIndex").Validintnozero("游戏机ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("startGames").Validintnozero("开始局数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("endGames").Validintnozero("结束局数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("encouragePrice").Validdecimalnozero("减免币数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var id = dicParas.Get("id").Toint(0);
                var gameIndex = dicParas.Get("gameIndex").Toint();
               
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_GameEncourageService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_GameEncourage();
                        model.MerchID = merchId;
                        model.StoreID = storeId;
                        Utils.GetModel(dicParas, ref model);
                        if (id == 0)
                        {
                            model.State = 1;
                            if (!Data_GameEncourageService.I.Add(model))
                            {
                                errMsg = "保存鼓励续玩规则失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该规则不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_GameEncourageService.I.Update(model))
                            {
                                errMsg = "保存鼓励续玩规则失败";
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
        public object DelGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_GameEncourageService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该规则不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_GameEncourageService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        Data_GameEncourageService.I.DeleteModel(model);

                        if (!Data_GameEncourageService.I.SaveChanges())
                        {
                            errMsg = "删除鼓励续玩规则失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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
        public object EnGameEncourage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("鼓励续玩ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("state").Validint("启停状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                

                var id = dicParas.Get("id").Toint();
                var state = dicParas.Get("state").Toint();

                var data_GameEncourageService = Data_GameEncourageService.I;
                if (!data_GameEncourageService.Any(a => a.ID == id))
                {
                    errMsg = "该鼓励续玩信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var model = data_GameEncourageService.GetModels(p => p.ID == id).FirstOrDefault();
                model.State = state;
                if (!data_GameEncourageService.Update(model))
                {
                    errMsg = "保存鼓励续玩规则失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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