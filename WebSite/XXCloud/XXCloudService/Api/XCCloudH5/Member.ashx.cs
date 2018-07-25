using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business;
using XCCloudService.Business.Common;
using XCCloudService.Business.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.WeiXin;
using XCCloudService.Model.WeiXin.Message;
using XCCloudService.Model.XCCloud;
using XCCloudService.WeiXin.Message;
using XCCloudService.WeiXin.WeixinPub;

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

                if(string.IsNullOrEmpty(storeId))
                {
                    storeId = model.CurrStoreId;
                }


                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.ID == storeId).FirstOrDefault();
                if(store != null && model.CurrStoreId != store.ID)
                {
                    model.CurrStoreId = store.ID;
                }

                if (model.CurrentCardInfo == null)
                {
                    Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.MemberID == model.MemberId).OrderByDescending(t => t.UpdateTime).FirstOrDefault();
                    if (card != null)
                    {
                        MemberCard cardInfo = new MemberCard();
                        cardInfo.CardId = card.ID;
                        cardInfo.ICCardId = card.ICCardID;
                        cardInfo.StoreName = Base_StoreInfoService.I.GetModels(s => s.ID == card.StoreID).FirstOrDefault().StoreName ?? "";
                        cardInfo.MemberLevelId = card.MemberLevelID.Value;
                        cardInfo.MemberLevelName = XCCloudStoreBusiness.GetMemberLevelName(card.MemberLevelID.Value);

                        model.CurrentCardInfo = cardInfo;
                    }
                }

                //更新缓存
                MemberTokenCache.AddToken(token, model);

                //卡余额
                model.CurrentCardInfo.CardBalanceList = MemberBusiness.GetMemberBalanceAndExchangeRate(storeId, model.CurrentCardInfo.ICCardId).Select(t => new CardBalance
                {
                    BalanceIndex = t.BalanceIndex,
                    BalanceName = t.TypeName,
                    TotalQuantity = t.Total,
                    Balance = t.Balance,
                    BalanceFree = t.BalanceFree
                }).ToList();

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
                if(string.IsNullOrEmpty(code) || !code.Equals(smsCode, StringComparison.OrdinalIgnoreCase))
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
                    StoreName = Base_StoreInfoService.I.GetModels(s=>s.ID == t.StoreId).FirstOrDefault().StoreName ?? "",
                    LevelName = Data_MemberLevelService.I.GetModels(m=>m.ID == t.LevelId).FirstOrDefault().MemberLevelName,
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
                mc.StoreName = Base_StoreInfoService.I.GetModels(s => s.ID == card.StoreID).FirstOrDefault().StoreName ?? "";
                mc.MemberLevelId = card.MemberLevelID.Value;
                mc.MemberLevelName = XCCloudStoreBusiness.GetMemberLevelName(card.MemberLevelID.Value);
                //卡余额
                mc.CardBalanceList = MemberBusiness.GetMemberBalanceAndExchangeRate(card.LastStore, card.ICCardID).Select(t => new CardBalance
                {
                    BalanceIndex = t.BalanceIndex,
                    BalanceName = t.TypeName,
                    TotalQuantity = t.Total,
                    Balance = t.Balance,
                    BalanceFree = t.BalanceFree
                }).ToList();
                model.CurrentCardInfo = mc;
                model.CurrStoreId = card.LastStore;
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

        #region 微信H5提币
        ///// <summary>
        ///// 微信H5提币
        ///// </summary>
        ///// <param name="dicParas"></param>
        ///// <returns></returns>
        //[ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        //public object wechatOutCoin(Dictionary<string, object> dicParas)
        //{
        //    try
        //    {
        //        string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
        //        string qty = dicParas.ContainsKey("qty") ? dicParas["qty"].ToString().Trim() : string.Empty;
        //        string pwd = dicParas.ContainsKey("pwd") ? dicParas["pwd"].ToString().Trim() : string.Empty;

        //        if (string.IsNullOrEmpty(token))
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
        //        }

        //        MemberTokenModel model = MemberTokenCache.GetModel(token);

        //        if (model == null)
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
        //        }
        //        if (string.IsNullOrEmpty(model.CurrStoreId))
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择附近门店");
        //        }
        //        int memberLevelId = 0;
        //        if (model.CurrentCardInfo != null)
        //        {
        //            memberLevelId = model.CurrentCardInfo.MemberLevelId;
        //        }
        //        else
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择会员卡或绑定会员卡");
        //        }

        //        //当前会员卡
        //        Data_Member_Card memberCard = Data_Member_CardService.I.GetModels(t => t.ID == model.CurrentCardInfo.CardId).FirstOrDefault();
        //        if (memberCard == null)
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡无效");    
        //        }
        //        if (memberCard.CardType == 1)
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该会员卡没有提币权限");
        //        }

        //        //判断消费密码
        //        if (memberCard.CardPassword != pwd)
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡密码错误");
        //        }

        //        Data_MemberLevel levelModel = Data_MemberLevelService.I.GetModels(m => m.ID == memberLevelId).FirstOrDefault();
        //        if (levelModel == null || levelModel.AllowGetCoin == 0)
        //        {
        //            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该会员卡没有提币权限");
        //        }

        //        //查询余额类别映射为代币的、判断提币数量是否大于代币余额数量
                

        //        //Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.StoreID == model.CurrStoreId).FirstOrDefault();

        //        //List<FoodInfoViewModel> foodList = XCCloudStoreBusiness.GetFoodInfoList(store.MerchID, store.StoreID, memberLevelId);

        //        return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, null);
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
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

                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.ID == model.CurrStoreId).FirstOrDefault();

                List<FoodInfoViewModel> foodList = XCCloudStoreBusiness.GetFoodInfoList(store.MerchID, store.ID, memberLevelId);

                return ResponseModelFactory<List<FoodInfoViewModel>>.CreateModel(isSignKeyReturn, foodList);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 散客扫设备码创建订单
        /// <summary>
        /// 散客扫设备码创建订单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object createOrderByDeviceCode(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : string.Empty;
                string deviceToken = dicParas.ContainsKey("deviceToken") ? dicParas["deviceToken"].ToString().Trim() : "";
                string coinRuleId = dicParas.ContainsKey("coinRuleId") ? dicParas["coinRuleId"].ToString().Trim() : string.Empty;

                if (string.IsNullOrEmpty(deviceToken))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }

                MemberTokenModel memberTokenModel = MemberTokenCache.GetModel(token);
                if (memberTokenModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                if (string.IsNullOrEmpty(memberTokenModel.CurrStoreId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择附近门店");
                }

                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.ID == memberTokenModel.CurrStoreId).FirstOrDefault();
                if(store == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店信息无效");
                }

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels().FirstOrDefault(t => t.Token == deviceToken);
                if (device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }
                if (!string.IsNullOrEmpty(memberTokenModel.CurrStoreId) && memberTokenModel.CurrStoreId != device.StoreID)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前门店与设备所属门店不符，请先选择门店");
                }

                Data_GameInfo game = Data_GameInfoService.I.GetModels(t => t.ID == device.GameIndexID).FirstOrDefault();
                if (game == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "游戏机不存在");
                }

                //当前班次
                Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == store.ID && t.State == 1).FirstOrDefault();
                if (schedule == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前班次为空");
                }

                int OrderSource = 5; //订单来源 1 自助机
                decimal PayCount = 0m; //应付金额
                decimal FreePay = 0m; //减免金额

                string coinNote = string.Empty;

                int currCoinRuleId = 0;
                if (!int.TryParse(coinRuleId, out currCoinRuleId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "参数错误，请重试");
                }

                //散客扫码投币
                Data_GameAPP_Rule coinRule = Data_GameAPP_RuleService.I.GetModels(t => t.ID == currCoinRuleId).FirstOrDefault();
                PayCount = coinRule.PayCount.Value;
                coinNote = string.Format("{0}元{1}局", PayCount, coinRule.PlayCount);

                string orderId = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    //记录扫码规则
                    Flw_GameAPP_Rule_Entry gameRule = new Flw_GameAPP_Rule_Entry();
                    gameRule.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    gameRule.RuleID = coinRule.ID;
                    gameRule.MerchID = store.MerchID;
                    gameRule.StoreID = store.ID;
                    gameRule.GameIndexID = coinRule.GameID;
                    gameRule.DeviceID = device.ID;
                    gameRule.SiteName = device.SiteName;
                    gameRule.PayCount = PayCount;
                    gameRule.PlayCount = coinRule.PlayCount;
                    if (!Flw_GameAPP_Rule_EntryService.I.Add(gameRule))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "生成扫码记录失败");
                    }

                    Flw_Food_Sale foodSale = new Flw_Food_Sale();
                    foodSale.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    foodSale.MerchID = store.MerchID;
                    foodSale.StoreID = store.ID;
                    foodSale.FlowType = 0;
                    foodSale.SingleType = 7;//散客投币
                    foodSale.FoodID = gameRule.ID;
                    foodSale.SaleCount = 1;
                    foodSale.Point = 0;
                    foodSale.PointBalance = 0;
                    foodSale.MemberLevelID = memberTokenModel.CurrentCardInfo.MemberLevelId;
                    foodSale.Deposit = 0;
                    foodSale.OpenFee = 0;
                    foodSale.RenewFee = 0;
                    foodSale.ChangeFee = 0;
                    foodSale.ReissueFee = 0;
                    foodSale.TotalMoney = 0;
                    foodSale.Note = coinNote;
                    foodSale.BuyFoodType = 1;
                    foodSale.TaxFee = 0;
                    foodSale.TaxTotal = 0;
                    if (!Flw_Food_SaleService.I.Add(foodSale))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建销售流水失败");
                    }

                    Flw_Food_SaleDetail saleDetail = new Flw_Food_SaleDetail();
                    saleDetail.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    saleDetail.MerchID = store.MerchID;
                    saleDetail.FlwFoodID = foodSale.ID;
                    saleDetail.ContainCount = 1;
                    saleDetail.Status = 1;
                    if (!Flw_Food_SaleDetailService.I.Add(saleDetail))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建销售流水明细失败");
                    }

                    Flw_Order order = new Flw_Order();
                    order.ID = orderId;
                    order.MerchID = store.MerchID;
                    order.StoreID = device.StoreID;
                    order.FoodCount = 1;
                    order.GoodCount = 1;
                    order.MemberID = memberTokenModel.MemberId;
                    order.CardID = memberTokenModel.CurrentCardInfo.CardId;
                    order.OrderSource = OrderSource;
                    order.CreateTime = DateTime.Now;
                    order.PayCount = PayCount;
                    order.FreePay = FreePay;
                    order.OrderStatus = 1; //待支付
                    order.CheckDate = schedule.CheckDate;
                    order.Note = string.Format("{0}--投币，{1}", game.GameName, coinNote);

                    bool ret = Flw_OrderService.I.Add(order);
                    if (!ret)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建订单失败");
                    }

                    //订单明细
                    Flw_Order_Detail orderDetail = new Flw_Order_Detail();
                    orderDetail.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    orderDetail.MerchID = store.MerchID;
                    orderDetail.OrderFlwID = order.ID;
                    orderDetail.FoodFlwID = foodSale.ID;
                    orderDetail.GoodsCount = 1;
                    if (!Flw_Order_DetailService.I.Add(orderDetail))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建订单明细失败");
                    }

                    ts.Complete();
                }

                var result = new
                {
                    OrderId = orderId
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception ex)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
            }
        }
        #endregion

        #region 会员扫码支付
        /// <summary>
        /// 会员扫码支付
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object scanPayByDeviceCode(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : string.Empty;
                string deviceToken = dicParas.ContainsKey("deviceToken") ? dicParas["deviceToken"].ToString().Trim() : "";
                string coinRuleId = dicParas.ContainsKey("coinRuleId") ? dicParas["coinRuleId"].ToString().Trim() : string.Empty;

                if (string.IsNullOrEmpty(deviceToken))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }

                MemberTokenModel memberTokenModel = MemberTokenCache.GetModel(token);
                if (memberTokenModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                if (memberTokenModel.CurrentCardInfo == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择会员卡或绑定会员卡");
                }

                if (string.IsNullOrEmpty(memberTokenModel.CurrStoreId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择附近门店");
                }

                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.ID == memberTokenModel.CurrStoreId).FirstOrDefault();
                if (store == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店信息无效");
                }

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels().FirstOrDefault(t => t.Token == deviceToken);
                if (device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }
                if (!string.IsNullOrEmpty(memberTokenModel.CurrStoreId) && memberTokenModel.CurrStoreId != device.StoreID)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前门店与设备所属门店不符，请先选择门店");
                }

                Data_GameInfo game = Data_GameInfoService.I.GetModels(t => t.ID == device.GameIndexID).FirstOrDefault();
                if (game == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "游戏机不存在");
                }

                //当前班次
                Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == store.ID && t.State == 1).FirstOrDefault();
                if (schedule == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前班次为空");
                }

                string coinNote = string.Empty;

                int currCoinRuleId = 0;
                if (!int.TryParse(coinRuleId, out currCoinRuleId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "参数错误，请重试");
                }

                string cardId = memberTokenModel.CurrentCardInfo.CardId;
                //卡余额
                List<CardBalance> memberBalanceList = MemberBusiness.GetMemberBalanceAndExchangeRate(store.ID, memberTokenModel.CurrentCardInfo.ICCardId).Select(t => new CardBalance
                {
                    BalanceIndex = t.BalanceIndex,
                    BalanceName = t.TypeName,
                    TotalQuantity = t.Total,
                    Balance = t.Balance,
                    BalanceFree = t.BalanceFree
                }).ToList();

                decimal balance1 = 0;
                decimal balanceFree1 = 0;
                decimal balance2 = 0;
                decimal balanceFree2 = 0;

                //会员扫码
                Data_GameAPP_MemberRule coinRule = Data_GameAPP_MemberRuleService.I.GetModels(t => t.ID == currCoinRuleId).FirstOrDefault();
                if (coinRule != null)
                {
                    if (coinRule.PushCoin1 > 0)
                    {
                        Dict_BalanceType dic = Dict_BalanceTypeService.I.GetModels(t => t.ID == coinRule.PushBalanceIndex1).FirstOrDefault();
                        CardBalance currBalance = memberBalanceList.FirstOrDefault(t => t.BalanceIndex == coinRule.PushBalanceIndex1);
                        if (currBalance == null || currBalance.TotalQuantity < coinRule.PushCoin1)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("{0}余额不足，请充值", dic.TypeName));
                        }                        
                        coinNote = string.Format("扫码投币-{0}{1}", coinRule.PushCoin1, dic.TypeName);

                        balance1 = Convert.ToDecimal(coinRule.PushCoin1 * currBalance.Balance / (currBalance.Balance + currBalance.BalanceFree));

                        Dict_BalanceType bt = Dict_BalanceTypeService.I.GetModels(t => t.ID == coinRule.PushBalanceIndex1).FirstOrDefault();
                        if (bt.AddingType == 0)
                        {
                            //小数不保留
                            balance1 = Convert.ToDecimal((int)balance1);
                            balanceFree1 = Convert.ToDecimal(coinRule.PushCoin1 - (int)balance1);
                        }
                        else if (bt.AddingType == 1)
                        {
                            //保留全部，只要有小数就进位
                            if (((int)(balance1 * 100)) % 100 > 0)
                                balance1 = Convert.ToDecimal(((int)balance1) + 1);
                            balanceFree1 = Convert.ToDecimal(coinRule.PushCoin1 - (int)balance1);
                        }
                        else
                        {
                            balance1 = Decimal.Round(balance1, bt.DecimalNumber.Value, MidpointRounding.AwayFromZero);
                            balanceFree1 = Decimal.Round(balanceFree1, bt.DecimalNumber.Value, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (coinRule.PushCoin2 > 0)
                    {
                        Dict_BalanceType dic = Dict_BalanceTypeService.I.GetModels(t => t.ID == coinRule.PushBalanceIndex2).FirstOrDefault();
                        CardBalance currBalance = memberBalanceList.FirstOrDefault(t => t.BalanceIndex == coinRule.PushBalanceIndex2);
                        if (currBalance == null || currBalance.TotalQuantity < coinRule.PushCoin2)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("{0}余额不足，请充值", dic.TypeName));
                        }
                        
                        coinNote += string.Format("+{0}{1}", coinRule.PushCoin2, dic.TypeName);

                        balance2 = Convert.ToDecimal(coinRule.PushCoin2 * currBalance.Balance / (currBalance.Balance + currBalance.BalanceFree));

                        Dict_BalanceType bt = Dict_BalanceTypeService.I.GetModels(t => t.ID == coinRule.PushBalanceIndex1).FirstOrDefault();
                        if (bt.AddingType == 0)
                        {
                            //小数不保留
                            balance2 = Convert.ToDecimal((int)balance2);
                            balanceFree2 = Convert.ToDecimal(coinRule.PushCoin1 - (int)balance2);
                        }
                        else if (bt.AddingType == 1)
                        {
                            //保留全部，只要有小数就进位
                            if (((int)(balance2 * 100)) % 100 > 0)
                                balance2 = Convert.ToDecimal(((int)balance2) + 1);
                            balanceFree2 = Convert.ToDecimal(coinRule.PushCoin1 - (int)balance2);
                        }
                        else
                        {
                            balance2 = Decimal.Round(balance2, bt.DecimalNumber.Value, MidpointRounding.AwayFromZero);
                            balanceFree2 = Decimal.Round(balanceFree2, bt.DecimalNumber.Value, MidpointRounding.AwayFromZero);
                        }
                    }

                    string orderId = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        decimal? remainBalnce1 = 0;
                        decimal? remainBalnceFree1 = 0;
                        decimal? remainBalnce2 = 0;
                        decimal? remainBalnceFree2 = 0;
                        //扣减余额
                        if (balance1 > 0)
                        {
                            Data_Card_Balance dcb = Data_Card_BalanceService.I.GetModels(t => t.BalanceIndex == coinRule.PushBalanceIndex1 && t.CardIndex == cardId).FirstOrDefault();
                            dcb.Balance -= balance1;
                            if (!Data_Card_BalanceService.I.Update(dcb))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "更新卡余额失败");
                            }
                            remainBalnce1 = dcb.Balance;
                        }
                        if (balanceFree1 > 0)
                        {
                            Data_Card_Balance_Free dcbf = Data_Card_Balance_FreeService.I.GetModels(t => t.BalanceIndex == coinRule.PushBalanceIndex1 && t.CardIndex == cardId).FirstOrDefault();
                            dcbf.Balance -= balanceFree1;
                            if (!Data_Card_Balance_FreeService.I.Update(dcbf))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "更新卡余额失败");
                            }
                            remainBalnceFree1 = dcbf.Balance;
                        }
                        if (balance2 > 0)
                        {
                            Data_Card_Balance dcb = Data_Card_BalanceService.I.GetModels(t => t.BalanceIndex == coinRule.PushBalanceIndex2 && t.CardIndex == cardId).FirstOrDefault();
                            dcb.Balance -= balance2;
                            if (!Data_Card_BalanceService.I.Update(dcb))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "更新卡余额失败");
                            }
                            remainBalnce2 = dcb.Balance;
                        }
                        if (balanceFree2 > 0)
                        {
                            Data_Card_Balance_Free dcbf = Data_Card_Balance_FreeService.I.GetModels(t => t.BalanceIndex == coinRule.PushBalanceIndex2 && t.CardIndex == cardId).FirstOrDefault();
                            dcbf.Balance -= balanceFree2;
                            if (!Data_Card_Balance_FreeService.I.Update(dcbf))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "更新卡余额失败");
                            }
                            remainBalnceFree2 = dcbf.Balance;
                        }

                        Flw_DeviceData fdd = new Flw_DeviceData();
                        fdd.ID = RedisCacheHelper.CreateStoreSerialNo(store.ID);
                        fdd.MerchID = store.MerchID;
                        fdd.StoreID = store.ID;
                        fdd.DeviceID = device.ID;
                        fdd.GameIndexID = game.ID;
                        fdd.SiteName = "";
                        fdd.SN = 0;
                        fdd.BusinessType = 3;
                        fdd.State = 1;
                        fdd.RealTime = DateTime.Now;
                        fdd.MemberID = memberTokenModel.MemberId;
                        fdd.CreateStoreID = memberTokenModel.CurrentCardInfo.StoreId;
                        fdd.MemberName = memberTokenModel.Info == null ? "" : memberTokenModel.Info.nickname ?? "";
                        fdd.CardID = memberTokenModel.CurrentCardInfo.CardId;
                        fdd.BalanceIndex = coinRule.PushBalanceIndex1;
                        fdd.Coin = coinRule.PushCoin1;
                        fdd.RemainBalance = remainBalnce1 + remainBalnceFree1;
                        fdd.OrderID = orderId;
                        fdd.Note = coinNote;
                        fdd.CheckDate = schedule.CheckDate;
                        if (!Flw_DeviceDataService.I.Add(fdd))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建投币记录失败");
                        }

                        //记录余额变化流水
                        Flw_MemberData fmd = new Flw_MemberData();
                        fmd.ID = RedisCacheHelper.CreateStoreSerialNo(store.ID);
                        fmd.MerchID = store.MerchID;
                        fmd.StoreID = store.ID;
                        fmd.MemberID = memberTokenModel.MemberId;
                        fmd.MemberName = memberTokenModel.Info == null ? "散客" : memberTokenModel.Info.nickname ?? "散客";
                        fmd.CardIndex = cardId;
                        fmd.ICCardID = memberTokenModel.CurrentCardInfo.ICCardId;
                        fmd.MemberLevelName = memberTokenModel.CurrentCardInfo.MemberLevelName ?? "";
                        fmd.ChannelType = (int)MemberDataChannelType.吧台;
                        fmd.OperationType = (int)MemberDataOperationType.投币;
                        fmd.OPTime = DateTime.Now;
                        fmd.SourceType = 0;
                        fmd.SourceID = fdd.ID;
                        fmd.BalanceIndex = coinRule.PushBalanceIndex1;
                        fmd.ChangeValue = 0 - balance1;
                        fmd.Balance = remainBalnce1;
                        fmd.FreeChangeValue = 0 - balanceFree1;
                        fmd.FreeBalance = remainBalnceFree1;
                        fmd.BalanceTotal = fmd.Balance + fmd.FreeBalance;
                        fmd.Note = coinNote;
                        fmd.UserID = 0;
                        fmd.DeviceID = device.ID;
                        fmd.ScheduleID = schedule.ID;
                        fmd.AuthorID = 0;
                        fmd.WorkStation = "";
                        fmd.CheckDate = schedule.CheckDate;
                        if (!Flw_MemberDataService.I.Add(fmd))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建余额流水记录失败");
                        }

                        if (balance2 > 0 || balanceFree2 > 0)
                        {
                            fdd = new Flw_DeviceData();
                            fdd.ID = RedisCacheHelper.CreateStoreSerialNo(store.ID);
                            fdd.MerchID = store.MerchID;
                            fdd.StoreID = store.ID;
                            fdd.DeviceID = device.ID;
                            fdd.GameIndexID = game.ID;
                            fdd.SiteName = "";
                            fdd.SN = 0;
                            fdd.BusinessType = 3;
                            fdd.State = 1;
                            fdd.RealTime = DateTime.Now;
                            fdd.MemberID = memberTokenModel.MemberId;
                            fdd.CreateStoreID = memberTokenModel.CurrentCardInfo.StoreId;
                            fdd.MemberName = memberTokenModel.Info == null ? "散客" : memberTokenModel.Info.nickname ?? "散客";
                            fdd.CardID = memberTokenModel.CurrentCardInfo.CardId;
                            fdd.BalanceIndex = coinRule.PushBalanceIndex1;
                            fdd.Coin = coinRule.PushCoin2;
                            fdd.RemainBalance = remainBalnce2 + remainBalnceFree2; ;
                            fdd.OrderID = orderId;
                            fdd.Note = coinNote;
                            fdd.CheckDate = schedule.CheckDate;
                            if (!Flw_DeviceDataService.I.Add(fdd))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建投币记录失败");
                            }

                            //记录余额变化流水
                            fmd = new Flw_MemberData();
                            fmd.ID = RedisCacheHelper.CreateStoreSerialNo(store.ID);
                            fmd.MerchID = store.MerchID;
                            fmd.StoreID = store.ID;
                            fmd.MemberID = memberTokenModel.MemberId;
                            fmd.MemberName = memberTokenModel.Info == null ? "散客" : memberTokenModel.Info.nickname ?? "散客";
                            fmd.CardIndex = cardId;
                            fmd.ICCardID = memberTokenModel.CurrentCardInfo.ICCardId;
                            fmd.MemberLevelName = memberTokenModel.CurrentCardInfo.MemberLevelName ?? "";
                            fmd.ChannelType = (int)MemberDataChannelType.吧台;
                            fmd.OperationType = (int)MemberDataOperationType.投币;
                            fmd.OPTime = DateTime.Now;
                            fmd.SourceType = 0;
                            fmd.SourceID = fdd.ID;
                            fmd.BalanceIndex = coinRule.PushBalanceIndex2;
                            fmd.ChangeValue = 0 - balance2;
                            fmd.Balance = remainBalnce2;
                            fmd.FreeChangeValue = 0 - balanceFree2;
                            fmd.FreeBalance = remainBalnceFree2;
                            fmd.BalanceTotal = fmd.Balance + fmd.FreeBalance;
                            fmd.Note = coinNote;
                            fmd.UserID = 0;
                            fmd.DeviceID = device.ID;
                            fmd.ScheduleID = schedule.ID;
                            fmd.AuthorID = 0;
                            fmd.WorkStation = "";
                            fmd.CheckDate = schedule.CheckDate;
                            if (!Flw_MemberDataService.I.Add(fmd))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建余额流水记录失败");
                            }
                        }

                        Flw_GameAPP_MemberRule gmr = new Flw_GameAPP_MemberRule();
                        gmr.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                        gmr.RuleID = coinRule.ID;
                        gmr.MerchID = store.MerchID;
                        gmr.StoreID = store.ID;
                        gmr.GameIndexID = coinRule.GameID;
                        gmr.DeviceID = device.ID;
                        gmr.SiteName = device.SiteName;
                        gmr.MemberLevelID = memberTokenModel.CurrentCardInfo.MemberLevelId;
                        gmr.PushBalanceIndex1 = coinRule.PushBalanceIndex1;
                        gmr.PushCoin1 = coinRule.PushCoin1;
                        gmr.PushBalanceIndex2 = coinRule.PushBalanceIndex2;
                        gmr.PushCoin2 = coinRule.PushCoin2;
                        gmr.PlayCount = coinRule.PlayCount;
                        if (!Flw_GameAPP_MemberRuleService.I.Add(gmr))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建会员投币记录失败");
                        }

                        Flw_Food_Sale foodSale = new Flw_Food_Sale();
                        foodSale.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                        foodSale.MerchID = store.MerchID;
                        foodSale.StoreID = store.ID;
                        foodSale.FlowType = 0;
                        foodSale.SingleType = 8;//会员扫码投币
                        foodSale.FoodID = gmr.ID;
                        foodSale.SaleCount = 1;
                        foodSale.Point = 0;
                        foodSale.PointBalance = 0;
                        foodSale.MemberLevelID = memberTokenModel.CurrentCardInfo.MemberLevelId;
                        foodSale.Deposit = 0;
                        foodSale.OpenFee = 0;
                        foodSale.RenewFee = 0;
                        foodSale.ChangeFee = 0;
                        foodSale.ReissueFee = 0;
                        foodSale.TotalMoney = 0;
                        foodSale.Note = coinNote;
                        foodSale.BuyFoodType = 1;
                        foodSale.TaxFee = 0;
                        foodSale.TaxTotal = 0;
                        if (!Flw_Food_SaleService.I.Add(foodSale))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建销售流水失败");
                        }

                        Flw_Food_SaleDetail saleDetail = new Flw_Food_SaleDetail();
                        saleDetail.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                        saleDetail.MerchID = store.MerchID;
                        saleDetail.FlwFoodID = foodSale.ID;
                        saleDetail.ContainCount = 1;
                        saleDetail.Status = 1;
                        if (!Flw_Food_SaleDetailService.I.Add(saleDetail))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建销售流水明细失败");
                        }

                        Flw_Order order = new Flw_Order();
                        order.ID = orderId;
                        order.MerchID = store.MerchID;
                        order.StoreID = device.StoreID;
                        order.FoodCount = 1;
                        order.GoodCount = 1;
                        order.MemberID = memberTokenModel.MemberId;
                        order.CardID = memberTokenModel.CurrentCardInfo.CardId;
                        order.OrderSource = (int)OrderSource.H5_APP;
                        order.CreateTime = DateTime.Now;
                        order.PayCount = 0;
                        order.FreePay = 0;
                        order.OrderStatus = 2; //已支付
                        order.CheckDate = schedule.CheckDate;
                        order.Note = coinNote;

                        bool ret = Flw_OrderService.I.Add(order);
                        if (!ret)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建订单失败");
                        }

                        //订单明细
                        Flw_Order_Detail orderDetail = new Flw_Order_Detail();
                        orderDetail.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                        orderDetail.MerchID = store.MerchID;
                        orderDetail.OrderFlwID = order.ID;
                        orderDetail.FoodFlwID = foodSale.ID;
                        orderDetail.GoodsCount = 1;
                        if (!Flw_Order_DetailService.I.Add(orderDetail))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建订单明细失败");
                        }

                        ts.Complete();
                    }
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "投币规则无效");
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
            }
        }
        #endregion

        #region 创建充值套餐订单
        /// <summary>
        /// 创建充值套餐订单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object createRechargeOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                string strFoodId = dicParas.ContainsKey("foodId") ? dicParas["foodId"].ToString().Trim() : "";

                int foodId = strFoodId.Toint(0);
                if (string.IsNullOrEmpty(token))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效");
                }

                MemberTokenModel model = MemberTokenCache.GetModel(token);

                if (model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                if (model.CurrentCardInfo == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择会员卡或绑定会员卡");
                }

                if (string.IsNullOrEmpty(model.CurrStoreId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请先选择附近门店");
                }

                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.ID == model.CurrStoreId).FirstOrDefault();
                if (store == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店信息无效");
                }

                Data_FoodInfo food = Data_FoodInfoService.I.GetModels(t => t.ID == foodId).FirstOrDefault();
                if(food == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "选择的套餐无效");
                }

                //当前班次
                Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == store.ID && t.State == 1).FirstOrDefault();
                if (schedule == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前班次为空");
                }

                int memberLevelId = 0;
                if (model.CurrentCardInfo != null)
                {
                    memberLevelId = model.CurrentCardInfo.MemberLevelId;
                }
                string orderId = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    Flw_Food_Sale foodSale = new Flw_Food_Sale();
                    foodSale.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    foodSale.MerchID = store.MerchID;
                    foodSale.StoreID = store.ID;
                    foodSale.FlowType = 0;
                    foodSale.SingleType = 0;
                    foodSale.FoodID = food.ID.ToString();
                    foodSale.SaleCount = 1;
                    foodSale.Point = 0;
                    foodSale.PointBalance = 0;
                    foodSale.MemberLevelID = memberLevelId;
                    foodSale.Deposit = 0;
                    foodSale.OpenFee = 0;
                    foodSale.RenewFee = 0;
                    foodSale.ChangeFee = 0;
                    foodSale.ReissueFee = 0;
                    foodSale.TotalMoney = food.MemberPrice;
                    foodSale.Note = food.FoodName;
                    foodSale.BuyFoodType = 1;
                    foodSale.TaxFee = 0;
                    foodSale.TaxTotal = 0;
                    if (!Flw_Food_SaleService.I.Add(foodSale))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建销售流水失败");
                    }

                    Flw_Food_SaleDetail saleDetail = new Flw_Food_SaleDetail();
                    saleDetail.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    saleDetail.MerchID = store.MerchID;
                    saleDetail.FlwFoodID = foodSale.ID;
                    saleDetail.ContainCount = 1;
                    saleDetail.Status = 1;
                    if (!Flw_Food_SaleDetailService.I.Add(saleDetail))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建销售流水明细失败");
                    }

                    Flw_Order order = new Flw_Order();
                    order.ID = orderId;
                    order.MerchID = store.MerchID;
                    order.StoreID = store.ID;
                    order.FoodCount = 1;
                    order.GoodCount = 1;
                    order.MemberID = model.MemberId;
                    order.CardID = model.CurrentCardInfo.CardId;
                    order.OrderSource = 5;
                    order.CreateTime = DateTime.Now;
                    order.PayCount = food.MemberPrice;
                    order.FreePay = 0;
                    order.OrderStatus = 1; //待支付
                    order.CheckDate = schedule.CheckDate;
                    order.Note = food.FoodName;

                    bool ret = Flw_OrderService.I.Add(order);
                    if (!ret)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建订单失败");
                    }

                    //订单明细
                    Flw_Order_Detail orderDetail = new Flw_Order_Detail();
                    orderDetail.ID = RedisCacheHelper.CreateCloudSerialNo(store.ID);
                    orderDetail.MerchID = store.MerchID;
                    orderDetail.OrderFlwID = order.ID;
                    orderDetail.FoodFlwID = foodSale.ID;
                    orderDetail.GoodsCount = 1;
                    if (!Flw_Order_DetailService.I.Add(orderDetail))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建订单明细失败");
                    }

                    ts.Complete();
                }

                var result = new
                {
                    OrderId = orderId
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
    }
}