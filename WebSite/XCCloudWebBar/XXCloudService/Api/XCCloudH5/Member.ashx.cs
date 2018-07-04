using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Business;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.XCCloud;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.WeiXin;
using XCCloudWebBar.Model.WeiXin.Message;
using XCCloudWebBar.Model.XCCloud;
using XCCloudWebBar.WeiXin.Message;
using XCCloudWebBar.WeiXin.WeixinPub;

namespace XXCloudService.Api.XCCloudH5
{
    /// <summary>
    /// Member 的摘要说明
    /// </summary>
    public class Member : ApiBase
    {
        #region 发送模版消息
        /// <summary>
        /// 发送模版消息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object sendTemplateMessage(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString().Trim() : "";

                if (!Utils.CheckMobile(mobile))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码无效");
                }

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.Mobile == mobile && t.WechatOpenID != "").FirstOrDefault();
                if(member == null || string.IsNullOrWhiteSpace( member.WechatOpenID))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先关注微信公众号");
                }                

                string key = string.Empty;
                string code = Utils.getNumRandomCode(6);
                RedisCacheHelper.StringSet(CommonConfig.PrefixKey + mobile, code, TimeSpan.FromMinutes(5));

                PhoneVerifyCodeModel dataModel = new PhoneVerifyCodeModel();
                dataModel.keyword1 = code;
                dataModel.keyword2 = "5分钟";
                dataModel.remark = "如非您本人操作，请忽略此消息。";
                if (MessageMana.PushMessage(WeiXinMesageType.PhoneVerifyCode, member.WechatOpenID, dataModel, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

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
                        cardInfo.StoreName = Base_StoreInfoService.I.GetModels(s => s.StoreID == card.StoreID).FirstOrDefault().StoreName ?? "";
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
        public object bindCard(Dictionary<string, object> dicParas)
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

                MemberTokenModel model = MemberTokenCache.GetModel(token);

                if (model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                //短信验证码校验
                string code = RedisCacheHelper.StringGet(mobile);
                if(!code.Equals(smsCode, StringComparison.OrdinalIgnoreCase))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "验证码错误");
                }

                Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.ICCardID == icCardId).FirstOrDefault();
                if(card==null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }
                if (card.CardType == 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前卡片为附属卡，不能绑定");
                }

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == card.MemberID && t.Mobile == mobile).FirstOrDefault();
                if (member == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码错误");
                }

                //更新主卡及附属卡的memberId
                //更新memberInfo的openid               
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    var cardList = Data_Member_CardService.I.GetModels(t => t.ParentCard == card.ID || t.ID == card.ID);
                    foreach (var item in cardList)
                    {
                        item.MemberID = model.MemberId;
                        if (!Data_Member_CardService.I.Update(item))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "绑定会员卡失败");
                        }
                    }

                    //微信openid长度28位，阿里userid长度16位
                    if (token.Length > 16)
                    {
                        member.WechatOpenID = token;
                    }
                    else
                    {
                        member.AlipayOpenID = token;
                    }
                    if (!Base_MemberInfoService.I.Update(member))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "绑定会员卡失败");
                    }
                    ts.Complete();
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获取会员卡列表
        /// <summary>
        /// 获取会员卡列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getCardList(Dictionary<string, object> dicParas)
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

                var cardList = Data_Member_CardService.I.GetModels(t => t.MemberID == model.MemberId && t.CardType == 0).Select(t => new
                {
                    Id = t.ID,
                    ICCardId = t.ICCardID,
                    StoreId = t.StoreID,
                    LevelId = t.MemberLevelID,
                    CreateDate = t.CreateTime,
                    EndDate = t.EndDate
                }).ToList().Select(t => new
                {
                    Id = t.Id,
                    ICCardId = t.ICCardId,
                    StoreId = t.StoreId,
                    StoreName = Base_StoreInfoService.I.GetModels(s=>s.StoreID == t.StoreId).FirstOrDefault().StoreName ?? "",
                    LevelName = Data_MemberLevelService.I.GetModels(m=>m.MemberLevelID == t.LevelId).FirstOrDefault().MemberLevelName,
                    CreateDate = t.CreateDate.Value.ToString("yyyy-MM-dd"),
                    EndDate = t.EndDate.Value.ToString("yyyy-MM-dd")
                }).ToList();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, cardList);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 切换会员卡
        /// <summary>
        /// 切换会员卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object chooseCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                string cardId = dicParas.ContainsKey("cardId") ? dicParas["cardId"].ToString().Trim() : "";

                if (string.IsNullOrEmpty(token))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
                }

                MemberTokenModel model = MemberTokenCache.GetModel(token);

                if (model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.ID == cardId).FirstOrDefault();
                if (cardId == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡无效");
                }

                MemberCard mc = new MemberCard();
                mc.CardId = card.ID;
                mc.ICCardId = card.ICCardID;
                mc.StoreName = Base_StoreInfoService.I.GetModels(s => s.StoreID == card.StoreID).FirstOrDefault().StoreName ?? "";
                mc.MemberLevelId = card.MemberLevelID.Value;
                mc.MemberLevelName = XCCloudStoreBusiness.GetMemberLevel(card.MemberLevelID.Value).MemberLevelName;
                //卡余额
                mc.CardBalanceList = XCCloudStoreBusiness.GetCardStoreBalanceList(card.MerchID, card.LastStore, card.ID);
                model.CurrentCardInfo = mc;
                //更新缓存
                MemberTokenCache.AddToken(token, model);

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获取会员个人资料
        /// <summary>
        /// 获取会员个人资料
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getMemberDetail(Dictionary<string, object> dicParas)
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

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == model.MemberId).FirstOrDefault();

                var info = new
                {
                    Gender = member.Gender == null ? "" : member.Gender.Value.ToString(),
                    Birthday = member.Birthday == null ? "" : member.Birthday.Value.ToString("yyyy-MM-dd"),
                    IdCard = member.IDCard ?? "",
                    Mobile = member.Mobile ?? ""
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, info);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 完善会员信息
        /// <summary>
        /// 完善会员信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object setMemberInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                string gender = dicParas.ContainsKey("gender") ? dicParas["gender"].ToString().Trim() : "";
                string birthday = dicParas.ContainsKey("birthday") ? dicParas["birthday"].ToString().Trim() : "";
                string idCard = dicParas.ContainsKey("idCard") ? dicParas["idCard"].ToString().Trim() : "";
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(token))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
                }

                MemberTokenModel model = MemberTokenCache.GetModel(token);

                if (model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == model.MemberId).FirstOrDefault();
                member.Gender = Convert.ToInt32(gender);
                if (!string.IsNullOrWhiteSpace(idCard))
                {
                    if(!Regex.IsMatch(idCard, @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", RegexOptions.IgnoreCase))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "身份证号码不正确");
                    }
                    else
                    {
                        member.IDCard = idCard;
                        birthday = idCard.Substring(6, 4) + "-" + idCard.Substring(10, 2) + "-" + idCard.Substring(12, 2);
                    }
                }
                member.Birthday = Convert.ToDateTime(birthday);
                if(string.IsNullOrWhiteSpace(mobile) || !Regex.IsMatch(mobile, @"^1(3|5|7|8)\d{9}$"))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码无效");
                }
                member.Mobile = mobile;

                if(!Base_MemberInfoService.I.Update(member))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "完善资料失败");
                }

                model.Mobile = mobile;
                MemberTokenCache.AddToken(token, model);

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获取充值套餐
        /// <summary>
        /// 获取充值套餐
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getRechargeFood(Dictionary<string, object> dicParas)
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
                if(string.IsNullOrEmpty(model.CurrStoreId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择附近门店");
                }
                int memberLevelId = 0;
                if(model.CurrentCardInfo != null)
                {
                    //return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先办理会员卡");
                    memberLevelId = model.CurrentCardInfo.MemberLevelId;
                }

                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.StoreID == model.CurrStoreId).FirstOrDefault();

                List<FoodInfoViewModel> foodList = XCCloudStoreBusiness.GetFoodInfoList(store.MerchID, store.StoreID, memberLevelId);

                return ResponseModelFactory<List<FoodInfoViewModel>>.CreateModel(isSignKeyReturn, foodList);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
    }
}