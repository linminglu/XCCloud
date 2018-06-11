using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Model.WeiXin;
using XCCloudService.Model.XCCloud;
using XCCloudService.WeiXin.WeixinPub;

namespace XXCloudService.Api.XCCloudH5
{
    /// <summary>
    /// Member 的摘要说明
    /// </summary>
    public class Member : ApiBase
    {
        #region "获取会员微信信息"

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getMemberWechatInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(token))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
                }

                MemberTokenModel model = MemberTokenCache.GetModel(token);

                if (model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                if (!string.IsNullOrEmpty(model.MemberId) && model.CurrentCardInfo == null)
                {
                    Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.MemberID == model.MemberId).OrderByDescending(t => t.UpdateTime).FirstOrDefault();
                    if (card != null)
                    {
                        MemberCard cardInfo = new MemberCard();
                        cardInfo.CardId = card.ID;
                        cardInfo.ICCardId = card.ICCardID;
                        cardInfo.MemberLevelId = card.MemberLevelID.Value;
                        model.CurrentCardInfo = cardInfo;
                    }
                }

                return ResponseModelFactory<MemberTokenModel>.CreateModel(isSignKeyReturn, model);

                //WechatInfo model = CommonHelper.GetWechatInfo(token);
                //return ResponseModelFactory<WechatInfo>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}