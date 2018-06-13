using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business;
using XCCloudService.Business.Common;
using XCCloudService.Business.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Common;
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
        #region 发送短信验证码
        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getSMSCode(Dictionary<string, object> dicParas)
        {
            try
            {
                //是否模拟短信测试（1-模拟短信测试，不发送固定短信，不做短信验证）

                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString().Trim() : "";
                string templateId = "2";//短信模板

                if (!Utils.CheckMobile(mobile))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码无效");
                }

                //发送短信，并添加缓存成功
                string key = string.Empty;
                if (!FilterMobileBusiness.IsTestSMS && !FilterMobileBusiness.ExistMobile(mobile))
                {
                    string smsCode = string.Empty;
                    if (SMSBusiness.GetSMSCode(out smsCode))
                    {
                        RedisCacheHelper.StringSet(mobile, smsCode, TimeSpan.FromMinutes(5));
                        string errMsg = string.Empty;
                        if (SMSBusiness.SendSMSCode(templateId, mobile, smsCode, out errMsg))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                        }
                        else
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                        }
                    }
                    else
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "发送验证码出错");
                    }
                }
                else
                {
                    RedisCacheHelper.StringSet(mobile, "123456", TimeSpan.FromMinutes(5));
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region 获取会员信息
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getMemberInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(token))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
                }

                MemberTokenModel model = MemberTokenCache.GetModel(token);

                if (model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.StoreID == storeId).FirstOrDefault();
                if(store != null && model.CurrStoreId != store.StoreID)
                {
                    model.CurrStoreId = store.StoreID;
                }

                if (!string.IsNullOrEmpty(model.CurrStoreId) && !string.IsNullOrEmpty(model.MemberId))
                {
                    Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.MemberID == model.MemberId).OrderByDescending(t => t.UpdateTime).FirstOrDefault();
                    if (card != null)
                    {
                        MemberCard cardInfo = new MemberCard();
                        cardInfo.CardId = card.ID;
                        cardInfo.ICCardId = card.ICCardID;
                        cardInfo.MemberLevelId = card.MemberLevelID.Value;
                        cardInfo.MemberLevelName = XCCloudStoreBusiness.GetMemberLevel(card.MemberLevelID.Value).MemberLevelName;
                        //卡余额
                        cardInfo.CardBalanceList = XCCloudStoreBusiness.GetCardStoreBalanceList(card.MerchID, model.CurrStoreId, card.ID);
                        model.CurrentCardInfo = cardInfo;
                    }
                }

                //更新缓存
                MemberTokenCache.AddToken(token, model);

                return ResponseModelFactory<MemberTokenModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 注册会员(作废)
        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        //public object register(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
        //        string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString().Trim() : "";
        //        string smsCode = dicParas.ContainsKey("smsCode") ? dicParas["smsCode"].ToString().Trim() : "";
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
        //        }
        //        if (!Utils.CheckMobile(mobile))
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码无效");
        //        }

        //        string key = mobile + "_" + smsCode;
        //        if (!FilterMobileBusiness.IsTestSMS && !FilterMobileBusiness.ExistMobile(mobile))
        //        {
        //            if (!SMSCodeCache.IsExist(key))
        //            {
        //                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "短信验证码无效");
        //            }
        //        }

        //        if (SMSCodeCache.IsExist(key))
        //        {
        //            SMSCodeCache.Remove(key);
        //        }

        //        Base_MemberInfo member = null;
        //        //微信openid长度28位，阿里userid长度16位
        //        if(token.Length > 16)
        //        {
        //            member = Base_MemberInfoService.I.GetModels(t => t.Mobile == mobile && t.WechatOpenID == token).FirstOrDefault();
        //        }
        //        else
        //        {
        //            member = Base_MemberInfoService.I.GetModels(t => t.Mobile == mobile && t.AlipayOpenID == token).FirstOrDefault();
        //        }

        //        if(member == null)
        //        {
        //            member = new Base_MemberInfo();
        //            member.ID = RedisCacheHelper.CreateCloudSerialNo("0".PadLeft(15, '0'));
        //            if(token.Length > 16)
        //            {
        //                member.WechatOpenID = token;
        //            }
        //            else
        //            {
        //                member.AlipayOpenID = token;
        //            }
        //            member.Mobile = mobile;
        //            member.CreateTime = DateTime.Now;
        //            member.MemberState = 1;
        //            bool ret = Base_MemberInfoService.I.Add(member);
        //            if(!ret)
        //            {
        //                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "注册失败");
        //            }
        //        }

        //        MemberTokenModel model = MemberTokenCache.GetModel(token);

        //        if (model == null)
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
        //        }

        //        model.MemberId = member.ID;
        //        model.Mobile = mobile;

        //        if (model.CurrentCardInfo == null)
        //        {
        //            Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.MemberID == model.MemberId).OrderByDescending(t => t.UpdateTime).FirstOrDefault();
        //            if (card != null)
        //            {
        //                MemberCard cardInfo = new MemberCard();
        //                cardInfo.CardId = card.ID;
        //                cardInfo.ICCardId = card.ICCardID;
        //                cardInfo.MemberLevelId = card.MemberLevelID.Value;
        //                model.CurrentCardInfo = cardInfo;
        //            }
        //        }

        //        MemberTokenCache.AddToken(token, model);

        //        return ResponseModelFactory<MemberTokenModel>.CreateModel(isSignKeyReturn, model);

        //        //WechatInfo model = CommonHelper.GetWechatInfo(token);
        //        //return ResponseModelFactory<WechatInfo>.CreateModel(isSignKeyReturn, model);
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
        #endregion

        #region 验证卡并返回手机号码
        /// <summary>
        /// 验证卡并返回手机号码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getCardMobile(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(token))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
                }

                //判断是否为主卡
                Data_Member_Card currCard = Data_Member_CardService.I.GetModels(t => t.ICCardID == icCardId).FirstOrDefault();
                if (currCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }
                if(currCard.CardType == 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前卡片为附属卡，不能绑定");
                }

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == currCard.MemberID).FirstOrDefault();
                if (member == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }

                var model = new
                {
                    ICCardId = currCard.ICCardID,
                    Mobile = Regex.Replace(member.Mobile, "(\\d{3})\\d{4}(\\d{4})", "$1****$2")
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 绑定实体卡
        /// <summary>
        /// 绑定实体卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object BindCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString().Trim() : "";
                string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString().Trim() : "";
                string smsCode = dicParas.ContainsKey("smsCode") ? dicParas["smsCode"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(token))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
                }

                //短信验证码校验
                //if()

                Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.ICCardID == icCardId).FirstOrDefault();
                if(card==null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == card.MemberID && t.Mobile == mobile).FirstOrDefault();
                if (member == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码错误");
                }

                //更新主卡及附属卡的memberId
                //更新memberInfo的openid

                //MemberTokenModel model = MemberTokenCache.GetModel(token);

                //if (model == null)
                //{
                //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                //}

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
    }
}