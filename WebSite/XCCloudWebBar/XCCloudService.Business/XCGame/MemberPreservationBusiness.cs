using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCGame;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using XCCloudWebBar.Model.XCGame;
using XCCloudWebBar.Model.XCGameManager;

namespace XCCloudWebBar.Business.XCGame
{
    public class MemberPreservationBusiness
    {
        /// <summary>
        /// 会员存币处理
        /// </summary>
        /// <param name="iccard"></param>
        /// <param name="storeId"></param>
        /// <param name="balance"></param>
        /// <returns></returns>
        public static bool PreservationBusiness(int iccard,int storeId,int balance,out int lastBalance,out string storeName,out string mobile)
        {
            string errMsg = string.Empty ;
            lastBalance = 0;
            storeName = string.Empty;
            mobile = string.Empty;
            StoreBusiness storeBusiness = new StoreBusiness();
            StoreCacheModel storeModel = null;
            if (!storeBusiness.IsEffectiveStore(storeId.ToString(), ref storeModel, out errMsg))
            {
                errMsg = "门店信息不存在";
                return false;
            }

            storeName = storeModel.StoreName;

            IMemberService memberService = BLLContainer.Resolve<IMemberService>(storeModel.StoreDBName);
            var model = memberService.GetModels(p => p.ICCardID == iccard).FirstOrDefault<t_member>();
            if (model == null)
            {
                errMsg = "未查询到该店的会员信息";
                return false;
            }
            int num = 0;
            if (model.Balance == null)
            {
                num = 0;
            }
            else
            {
                num = (int)(model.Balance);
            }
            num += balance;
            model.Balance = num;
            lastBalance = (int)(model.Balance);
            mobile = model.Mobile;
            memberService.Update(model);
            return true;
        }
    }
}
