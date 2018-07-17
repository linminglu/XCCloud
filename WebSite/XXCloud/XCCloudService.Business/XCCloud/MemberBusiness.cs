using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XCCloudService.Business.XCCloud
{
    public class MemberBusiness
    {
        public static bool IsEffectiveStore(string mobile,ref XCCloudService.Model.XCCloudRS232.t_member memberModel, out string errMsg)
        {
            errMsg = string.Empty;
            XCCloudService.BLL.IBLL.XCCloudRS232.IMemberService memberService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCCloudRS232.IMemberService>();
            var model = memberService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase)).FirstOrDefault<XCCloudService.Model.XCCloudRS232.t_member>();
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

        public static bool UpdateBalanceFree(string merchId, string storeId, string cardIndex, FreeCoinModel currFreeDetail)
        {
            Data_Card_Balance_Free balanceFree = Data_Card_Balance_FreeService.I.GetModels(t => t.MerchID == merchId && t.CardIndex == cardIndex && t.BalanceIndex == currFreeDetail.balanceIndex).FirstOrDefault();
            if (balanceFree == null)
            {
                balanceFree.Balance += currFreeDetail.qty;
                if (!Data_Card_Balance_FreeService.I.Update(balanceFree))
                {
                    return false;
                }
            }
            else
            {
                //如果没有该币种余额就添加
                balanceFree = new Data_Card_Balance_Free();
                balanceFree.ID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                balanceFree.MerchID = merchId;
                balanceFree.CardIndex = cardIndex;
                balanceFree.BalanceIndex = currFreeDetail.balanceIndex;
                balanceFree.Balance = currFreeDetail.qty;
                balanceFree.UpdateTime = DateTime.Now;
                if (!Data_Card_Balance_FreeService.I.Add(balanceFree))
                {
                    return false;
                }
                var storeIds = GetBalanceChainStoreList(merchId, currFreeDetail.balanceIndex);
                foreach (var sid in storeIds)
                {
                    Data_Card_Balance_StoreList sl = new Data_Card_Balance_StoreList();
                    sl.ID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                    sl.CardBalanceID = balanceFree.ID;
                    sl.StoreID = sid;
                    if (!Data_Card_Balance_StoreListService.I.Add(sl))
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
    }
}
