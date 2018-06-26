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
    }
}
