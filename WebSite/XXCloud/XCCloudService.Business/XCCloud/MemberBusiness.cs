using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.XCCloud;
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

        public static MemberCardInfoViewModel GetMemberCardInfo(Base_MemberInfo member, string merchId, string storeId)
        {
            var cardList = from card in Data_Member_CardService.I.GetModels(t => t.MemberID == member.ID && t.MerchID == merchId)
                           join storeCard in Data_Member_Card_StoreService.I.GetModels(t => t.StoreID == storeId)
                           on card.ID equals storeCard.CardID
                           select new
                           {
                               Id = card.ID
                           };

            if (cardList.Count() == 0)
            {
                return null;
            }

            string cardId = cardList.FirstOrDefault().Id;
            Data_Member_Card memberCard = Data_Member_CardService.I.GetModels(t => t.ID == cardId).FirstOrDefault();
            if (memberCard == null)
            {
                return null;
            }

            MemberCardInfoViewModel model = new MemberCardInfoViewModel();
            model.CardId = memberCard.ID;
            model.ICCardId = memberCard.ICCardID;
            model.AllowIn = memberCard.AllowIn.Value;
            model.AllowOut = memberCard.AllowOut.Value;
            model.CardStatus = memberCard.CardStatus.Value;

            List<MemberBalanceExchangeRateModel> memberBalance = XCCloudService.Business.XCCloud.MemberBusiness.GetMemberBalanceAndExchangeRate(storeId, memberCard.ICCardID);
            if (memberBalance != null && memberBalance.Count > 0)
            {
                model.MemberBalances = memberBalance.Select(t => new BalanceModel
                {
                    BalanceIndex = t.BalanceIndex,
                    BalanceName = t.TypeName,
                    Quantity = t.Total
                }).ToList();
            }

            var cardRights = Data_Card_RightService.I.GetModels(t => t.CardID == memberCard.ID).ToList();
            var storeRights = Data_Card_Right_StoreListService.I.GetModels(t => t.StoreID == storeId).ToList();
            Data_Card_Right currCardRight = cardRights.Where(t => storeRights.Any(s => s.CardRightID == t.ID)).FirstOrDefault();
            if (currCardRight != null)
            {
                model.AllowExitCoin = currCardRight.AllowExitCoin.Value;
                model.AllowSaleCoin = currCardRight.AllowSaleCoin.Value;
                model.AllowSaveCoin = currCardRight.AllowSaveCoin.Value;
                model.AllowFreeCoin = currCardRight.AllowFreeCoin.Value;
            }
            return model;
        }
    }
}
