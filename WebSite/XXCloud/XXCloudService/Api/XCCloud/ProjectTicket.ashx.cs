using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Common.Extensions;
using System.Transactions;
using XCCloudService.BLL.XCCloud;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Model.XCCloud;
using System.Data.SqlClient;
using XCCloudService.Common.Enum;
using XCCloudService.DBService.BLL;
using System.Data;
using XCCloudService.BLL.CommonBLL;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// ProjectTicket 的摘要说明
    /// </summary>
    public class ProjectTicket : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryProjectTicket(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("ticketType").Validint("门票类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("businessType").Validint("业务类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var ticketType = dicParas.Get("ticketType").Toint();
                var businessType = dicParas.Get("businessType").Toint();

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;
                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                #region Sql语句

                //排除游乐项目类型的游戏机
                var projectGameTypes = getProjectGameTypes(out errMsg);
                if (!errMsg.IsNull())
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@projectGameTypes", projectGameTypes);

//                string sql = @"SELECT
//                                    a.*, stuff((
//                                    select '|' + t.ProjectName 
//                                    from (
//                                        select (case when CHARINDEX(CONVERT(varchar, c.ProjcetType), @projectGameTypes)>0 then d.ProjectName else e.GameName end) AS ProjectName
//                                        from Data_ProjectTicket b
//                                        inner join Data_ProjectTicket_Bind c on b.ID = c.ProjcetTicketID
//                                        left join Data_ProjectInfo d on c.ProjcetID = d.ID and d.ChargeType = 0 and d.State = 1
//                                        left join Data_GameInfo e on c.ProjcetID = e.ID and e.State = 1
//                                        where b.ID=a.ID
//                                    ) t
//                                    for xml path('')),1,1,'') as BindProjects
//                                FROM
//                                	Data_ProjectTicket a
//                                WHERE 1=1
//                            ";
                string sql = @"SELECT
                                    a.*, b.ProjectName + '等多个' as BindProjects
                                FROM
                                	Data_ProjectTicket a
                                LEFT JOIN (                                
                                	SELECT 
                                		b.ID, (case when CHARINDEX(CONVERT(varchar, c.ProjcetType), @projectGameTypes)>0 then d.ProjectName else e.GameName end) AS ProjectName,
                                        ROW_NUMBER() over(partition by b.ID order by b.ID) as RowNum
                                	FROM
                                		Data_ProjectTicket b                                                        
                                	inner join Data_ProjectTicket_Bind c on b.ID = c.ProjcetTicketID
                                    left join Data_ProjectInfo d on c.ProjcetID = d.ID and d.ChargeType = 0 and d.State = 1
                                    left join Data_GameInfo e on c.ProjcetID = e.ID and e.State = 1                                          
                                ) b ON a.ID = b.ID and b.RowNum <= 1
                                WHERE 1=1
                            ";
                sql += " AND a.StoreID=" + storeId; 
                sql += " AND a.TicketType=" + ticketType;
                sql += " AND a.BusinessType=" + businessType;

                #endregion

                var list = Data_ProjectTicketService.I.SqlQuery<Data_ProjectTicketList>(sql, parameters).ToList();
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectTicket(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("门票ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var model = Data_ProjectTicketService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (model == null)
                {
                    errMsg = "该门票不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //排除游乐项目类型的游戏机
                var projectGameTypes = getProjectGameTypes(out errMsg);
                if (!errMsg.IsNull())
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var data_ProjectTicket = new
                {
                    model = model,
                    StartTimeStr = Utils.TimeSpanToStr(model.StartTime),
                    EndTimeStr = Utils.TimeSpanToStr(model.EndTime),
                    ProjectTicketBinds = from a in Data_ProjectTicket_BindService.N.GetModels(p => p.ProjcetTicketID == id)
                                         join b in Data_ProjectInfoService.N.GetModels(p => p.ChargeType == (int)ProjectInfoChargeType.Count && p.State == 1) on a.ProjcetID equals b.ID into b1
                                         from b in b1.DefaultIfEmpty()
                                         join c in Dict_SystemService.N.GetModels() on a.ProjcetType equals c.ID into c1
                                         from c in c1.DefaultIfEmpty()
                                         join d in Data_GameInfoService.N.GetModels(p => p.State == 1) on a.ProjcetID equals d.ID into d1
                                         from d in d1.DefaultIfEmpty()
                                         select new
                                         {
                                             ID = a.ID,
                                             ProjcetTicketID = a.ProjcetTicketID,
                                             ProjcetID = a.ProjcetID,
                                             ProjcetType = a.ProjcetType,
                                             UseCount = a.UseCount,
                                             AllowShareCount = a.AllowShareCount,
                                             WeightValue = a.WeightValue,
                                             PushCoin1 = (a.UseCount ?? 0) > 0 ? (a.WeightValue / a.UseCount) : 0,
                                             ProjectName = projectGameTypes.Contains(a.ProjcetType + "") ? b.ProjectName : d.GameName,
                                             ProjcetTypeStr = c != null ? c.DictKey : string.Empty
                                         }
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_ProjectTicket);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveProjectTicket(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("businessType").Validint("业务类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("businessType").Toint() == (int)BusinessType.Ticket)
                    if (!dicParas.Get("divideType").Validint("分摊方式", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg); 
                if (!dicParas.Get("ticketType").Validint("门票类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("ticketName").Nonempty("门票名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                                       
                if (!dicParas.Get("price").Validdecimalnozero("售价", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                var id = dicParas.Get("id").Toint(0);
                var ticketType = dicParas.Get("ticketType").Toint();
                var divideType = dicParas.Get("divideType").Toint(0);
                var businessType = dicParas.Get("businessType").Toint();
                var projectTicketBinds = dicParas.GetArray("projectTicketBinds");
                var allowExitTimes = dicParas.Get("allowExitTimes").Toint(0);
                var groupStartupCount = dicParas.Get("groupStartupCount").Toint(0);
                var readFace = dicParas.Get("readFace").Toint(0);
                var accompanyCash = dicParas.Get("accompanyCash").Todecimal(0);
                var balanceIndex = dicParas.Get("balanceIndex").Toint(0);
                var balanceValue = dicParas.Get("balanceValue").Todecimal(0);
                var allowRestrict = dicParas.Get("allowRestrict").Toint(0);
                var restrictShareCount = dicParas.Get("restrictShareCount").Toint(0);
                var restrictPeriodType = dicParas.Get("restrictPeriodType").Toint(0);
                var restrictPeriodValue = dicParas.Get("restrictPeriodValue").Toint(0);
                var restrctCount = dicParas.Get("restrctCount").Toint(0);
                var note = dicParas.Get("note");
                var vaildStartDate = dicParas.Get("vaildStartDate").Todate();
                var vaildEndDate = dicParas.Get("vaildEndDate").Todate();
                var noStartDate = dicParas.Get("noStartDate").Todate();
                var noEndDate = dicParas.Get("noEndDate").Todate();

                if ((businessType == (int)BusinessType.Ticket && ticketType == (int)TicketType.Period) || ticketType == (int)TicketType.Group) //年月日票或团体票
                {
                    if (divideType != (int)DivideType.Day)
                    {
                        errMsg = "年月日票或团体票应为按天分摊方式";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                if (businessType == (int)BusinessType.Ticket && ticketType == (int)TicketType.Count) //次票
                {
                    if (divideType != (int)DivideType.Once && divideType != (int)DivideType.Count)
                    {
                        errMsg = "次票应为一次性分摊或按次分摊方式";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var model = Data_ProjectTicketService.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_ProjectTicket();                        
                        Utils.GetModel(dicParas, ref model);
                        model.DivideType = divideType;
                        model.AllowExitTimes = allowExitTimes;
                        model.GroupStartupCount = groupStartupCount;
                        model.ReadFace = readFace;
                        model.AccompanyCash = accompanyCash;
                        model.BalanceIndex = balanceIndex;
                        model.BalanceValue = balanceValue;
                        model.AllowRestrict = allowRestrict;
                        model.RestrictShareCount = restrictShareCount;
                        model.RestrictPeriodType = restrictPeriodType;
                        model.RestrictPreiodValue = restrictPeriodValue;
                        model.RestrctCount = restrctCount;
                        model.Note = note;
                        model.VaildStartDate = vaildStartDate;
                        model.VaildEndDate = vaildEndDate;
                        model.NoStartDate = noStartDate;
                        model.NoEndDate = noEndDate;
                        if (id == 0)
                        {
                            model.MerchID = merchId;
                            model.StoreID = storeId;
                            if (!Data_ProjectTicketService.I.Add(model))
                            {
                                errMsg = "保存门票失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (model.ID == 0)
                            {
                                errMsg = "该门票不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (!Data_ProjectTicketService.I.Update(model))
                            {
                                errMsg = "保存门票失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = model.ID;

                        //保存绑定项目
                        if (projectTicketBinds != null && projectTicketBinds.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var bindModel in Data_ProjectTicket_BindService.I.GetModels(p => p.ProjcetTicketID == id))
                            {
                                Data_ProjectTicket_BindService.I.DeleteModel(bindModel);
                            }

                            foreach (IDictionary<string, object> el in projectTicketBinds)
                            {
                                if (el != null)
                                {
                                    var dicPar = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPar.Get("projcetId").Validintnozero("游乐项目ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPar.Get("projcetType").Validintnozero("游乐项目类型", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPar.Get("weightValue").Validint("权重值", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (ticketType == (int)TicketType.Count || ticketType == (int)TicketType.Group) //次票或团体票
                                    {
                                        if (!dicPar.Get("allowShareCount").Validint("次数共享", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        if (!dicPar.Get("useCount").Validintnozero("游玩次数", out errMsg))
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var bindModel = new Data_ProjectTicket_Bind();
                                    bindModel.MerchID = merchId;
                                    bindModel.ProjcetType = dicPar.Get("projcetType").Toint();
                                    bindModel.ProjcetTicketID = id;
                                    bindModel.ProjcetID = dicPar.Get("projcetId").Toint();
                                    bindModel.UseCount = dicPar.Get("useCount").Toint();
                                    bindModel.AllowShareCount = dicPar.Get("allowShareCount").Toint();
                                    bindModel.WeightValue = dicPar.Get("weightValue").Toint();
                                    Data_ProjectTicket_BindService.I.AddModel(bindModel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_ProjectTicket_BindService.I.SaveChanges())
                            {
                                errMsg = "保存绑定项目失败";
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
        public object DelProjectTicket(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("门票ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_ProjectTicketService.I.Any(p => p.ID == id))
                        {
                            errMsg = "该门票不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var model = Data_ProjectTicketService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        Data_ProjectTicketService.I.DeleteModel(model);

                        foreach (var bindModel in Data_ProjectTicket_BindService.I.GetModels(p => p.ProjcetTicketID == id))
                        {
                            Data_ProjectTicket_BindService.I.DeleteModel(bindModel);
                        }

                        if (!Data_ProjectTicketService.I.SaveChanges())
                        {
                            errMsg = "删除门票信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Data_ProjectTicket_BindService.I.SaveChanges())
                        {
                            errMsg = "删除门票绑定信息失败";
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
        public object GetProjectGameInfoList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                var errMsg = string.Empty;
                if (!dicParas.Get("businessType").Validint("业务类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var businessType = dicParas.Get("businessType").Toint();

                //绑定游乐项目
                var projectGameInfoList = from a in Data_ProjectInfoService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.State == 1 && p.ChargeType == (int)ProjectInfoChargeType.Count)
                                          join b in Dict_SystemService.N.GetModels() on a.ProjectType equals b.ID into b1
                                          from b in b1.DefaultIfEmpty()
                                          join c in Data_GameInfoService.N.GetModels(p => p.State == 1) on a.GameIndex equals c.ID
                                          select new
                                          {
                                              ProjectID = a.ID,
                                              ProjectName = a.ProjectName,
                                              ProjcetType = a.ProjectType,
                                              ProjcetTypeStr = b != null ? b.DictKey : string.Empty,
                                              PushCoin1 = c.PushCoin1 ?? 0
                                          };

                //如果是限时任玩或机台打包，则还可以选择机台绑定
                if (businessType != (int)BusinessType.Ticket)
                {
                    //排除游乐项目类型的游戏机
                    var projectGameTypes = getProjectGameTypes(out errMsg);
                    if (!errMsg.IsNull())
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                    projectGameInfoList =
                        projectGameInfoList.Union(
                            from a in Data_GameInfoService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.State == 1)
                            join b in Dict_SystemService.N.GetModels() on a.GameType equals b.ID into b1
                            from b in b1.DefaultIfEmpty()
                            where !projectGameTypes.Contains(a.GameType + "")
                            select new
                            {
                                ProjectID = a.ID,
                                ProjectName = a.GameName,
                                ProjcetType = a.GameType,
                                ProjcetTypeStr = b != null ? b.DictKey : string.Empty,
                                PushCoin1 = a.PushCoin1 ?? 0
                            });
                }

                projectGameInfoList = projectGameInfoList.OrderBy(or => new { or.ProjcetType, or.ProjectID });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, projectGameInfoList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

    }
}