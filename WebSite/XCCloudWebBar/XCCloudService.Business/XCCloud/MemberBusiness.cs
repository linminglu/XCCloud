using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.XCCloud;

namespace XCCloudWebBar.Business.XCCloud
{
    public class MemberBusiness
    {
        public static bool IsEffectiveStore(string mobile,ref XCCloudWebBar.Model.XCCloudRS232.t_member memberModel, out string errMsg)
        {
            errMsg = string.Empty;
            XCCloudWebBar.BLL.IBLL.XCCloudRS232.IMemberService memberService = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCCloudRS232.IMemberService>();
            var model = memberService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase)).FirstOrDefault<XCCloudWebBar.Model.XCCloudRS232.t_member>();
            if (model == null)
            {
                errMsg = "会员信息不存在";
                return false;
            }
            else if (model.Lock == 1)
            {
                errMsg = "会员已被锁定";
                return false;
            }
            else
            {
                memberModel = model;
                return true;
            }
        }

        public static List<MemberBalanceExchangeRateModel> GetMemberBalanceAndExchangeRate(string storeId, string ICCardId)
        {
            string storedProcedure = "GetMemberBalanceAndExchangeRate";
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@ICCardID", ICCardId);
            parameters[1] = new SqlParameter("@StoreID", storeId);
            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);

            List<MemberBalanceExchangeRateModel> memberBalance = new List<MemberBalanceExchangeRateModel>();
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                memberBalance = Utils.GetModelList<MemberBalanceExchangeRateModel>(ds.Tables[0]);
            }
            return memberBalance;
        }

        public static bool ExistsCardByICCardId(string iccardId, string merchId, string storeId, out string msg)
        {
            msg = string.Empty;
            //查询卡是否存在
            var cardQuery = from a in Data_Member_CardService.N.GetModels(t => t.MerchID == merchId && t.ICCardID == iccardId)
                            join b in Data_Member_Card_StoreService.N.GetModels(t => t.StoreID == storeId) on a.ID equals b.CardID
                            select new
                            {
                                CardId = a.ID
                            };

            int cardCount = cardQuery.Count();

            if (cardCount == 0)
            {
                msg = "会员卡不存在";
                return false;
            }
            else if (cardCount > 1)
            {
                msg = "存在多张相同卡号的会员卡";
                return false;
            }
            return true;
        }

        public static List<string> GetBalanceChainStoreList(string merchId, int balanceIndex)
        {
            //获取当前商户余额互通规则列表
            var balanceChainList = from a in Base_ChainRuleService.I.GetModels(t => t.MerchID == merchId && t.RuleType == balanceIndex)
                                   join b in Base_ChainRule_StoreService.I.GetModels(t => t.MerchID == merchId) on a.ID equals b.RuleGroupID
                                   select new
                                   {
                                       BalanceIndex = a.RuleType,
                                       StoreId = b.StoreID
                                   };
            //所有余额互通门店
            var storeIds = balanceChainList.GroupBy(t => t.StoreId).Select(t => t.Key).ToList();
            return storeIds;
        }

        public static bool UpdateBalanceFree(string merchId, string storeId, Base_MemberInfo member, Data_Member_Card memberCard, Flw_Schedule schedule, int userId, string workStation, string sourceId, int operationType, FreeDetailModel currFreeDetail, bool isFree, string note)
        {
            Data_Card_Balance balance = Data_Card_BalanceService.I.GetModels(t => t.MerchID == merchId && t.CardIndex == memberCard.ID && t.BalanceIndex == currFreeDetail.BalanceIndex).FirstOrDefault();
            Data_Card_Balance_Free balanceFree = Data_Card_Balance_FreeService.I.GetModels(t => t.MerchID == merchId && t.CardIndex == memberCard.ID && t.BalanceIndex == currFreeDetail.BalanceIndex).FirstOrDefault();
            if(isFree)
            {
                if (balanceFree != null)
                {
                    balanceFree.Balance += currFreeDetail.Quantity;
                    if (!Data_Card_Balance_FreeService.I.Update(balanceFree))
                    {
                        return false;
                    }

                    //记录余额变化流水
                    Flw_MemberData fmd = new Flw_MemberData();
                    fmd.ID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                    fmd.MerchID = merchId;
                    fmd.StoreID = storeId;
                    fmd.MemberID = member.ID;
                    fmd.MemberName = member.UserName;
                    fmd.CardIndex = memberCard.ID;
                    fmd.ICCardID = memberCard.ICCardID;
                    fmd.MemberLevelName = Data_MemberLevelService.I.GetModels(m => m.ID == memberCard.MemberLevelID).FirstOrDefault().MemberLevelName;
                    fmd.ChannelType = (int)MemberDataChannelType.吧台;
                    fmd.OperationType = operationType;
                    fmd.OPTime = DateTime.Now;
                    fmd.SourceType = 0;
                    fmd.SourceID = sourceId;
                    fmd.BalanceIndex = balance.BalanceIndex;
                    fmd.ChangeValue = 0;
                    fmd.Balance = balance == null ? 0 : balance.Balance.Todecimal(0);
                    fmd.FreeChangeValue = currFreeDetail.Quantity;
                    fmd.FreeBalance = balanceFree.Balance;
                    fmd.BalanceTotal = fmd.Balance + fmd.FreeBalance;
                    fmd.Note = note;
                    fmd.UserID = userId;
                    fmd.DeviceID = 0;
                    fmd.ScheduleID = schedule.ID;
                    fmd.AuthorID = 0;
                    fmd.WorkStation = workStation;
                    fmd.CheckDate = schedule.CheckDate;
                    if (!Flw_MemberDataService.I.Add(fmd))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (balance != null)
                {
                    balance.Balance += currFreeDetail.Quantity;
                    if (!Data_Card_BalanceService.I.Update(balance))
                    {
                        return false;
                    }

                    //记录余额变化流水
                    Flw_MemberData fmd = new Flw_MemberData();
                    fmd.ID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                    fmd.MerchID = merchId;
                    fmd.StoreID = storeId;
                    fmd.MemberID = member.ID;
                    fmd.MemberName = member.UserName;
                    fmd.CardIndex = memberCard.ID;
                    fmd.ICCardID = memberCard.ICCardID;
                    fmd.MemberLevelName = Data_MemberLevelService.I.GetModels(m => m.ID == memberCard.MemberLevelID).FirstOrDefault().MemberLevelName;
                    fmd.ChannelType = (int)MemberDataChannelType.吧台;
                    fmd.OperationType = operationType;
                    fmd.OPTime = DateTime.Now;
                    fmd.SourceType = 0;
                    fmd.SourceID = sourceId;
                    fmd.BalanceIndex = balance.BalanceIndex;
                    fmd.ChangeValue = currFreeDetail.Quantity;
                    fmd.Balance = balance.Balance;
                    fmd.FreeChangeValue = 0;
                    fmd.FreeBalance = balanceFree == null ? 0 : balanceFree.Balance.Todecimal(0);
                    fmd.BalanceTotal = fmd.Balance + fmd.FreeBalance;
                    fmd.Note = note;
                    fmd.UserID = userId;
                    fmd.DeviceID = 0;
                    fmd.ScheduleID = schedule.ID;
                    fmd.AuthorID = 0;
                    fmd.WorkStation = workStation;
                    fmd.CheckDate = schedule.CheckDate;
                    if (!Flw_MemberDataService.I.Add(fmd))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public static List<CardLockStateModel> GetMemberCardLockState(int lockState)
        {
            List<CardLockStateModel> list = new List<CardLockStateModel>();
            var strLockBinary = Convert.ToString(lockState, 2).PadLeft(8, '0').ToCharArray().Reverse().ToList();
            int index = 0;
            foreach (var item in strLockBinary)
            {
                if(item == '1')
                {
                    CardLockStateModel lockModel = new CardLockStateModel();
                    lockModel.LockType = index;
                    list.Add(lockModel);
                }
                index++;
            }
            return list;
        }

        public static Dict_BalanceType GetBalanceTypeModel(int id)
        {
            return Dict_BalanceTypeService.I.GetModels(t => t.ID == id).FirstOrDefault();
        }

        public static CardPurviewModel GetCardRight(string storeId, string cardIndex)
        {
            var cardRight = from a in Data_Card_RightService.N.GetModels(t => t.CardID == cardIndex)
                            join b in Data_Card_Right_StoreListService.N.GetModels(t => t.StoreID == storeId) on a.ID equals b.CardRightID
                            select new CardPurviewModel
                            {
                                AllowIn = a.AllowPush.Value,
                                AllowOut = a.AllowOut.Value,
                                AllowExitCoin = a.AllowExitCoin.Value,
                                AllowSaleCoin = a.AllowSaleCoin.Value,
                                AllowSaveCoin = a.AllowSaveCoin.Value,
                                AllowFreeCoin = a.AllowFreeCoin.Value,
                                AllowRenew = a.AllowRenew.Value
                            };
            var cardPurview = cardRight.FirstOrDefault();
            return cardPurview;
        }
    }
}
