using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCGame;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.HaoKu.Com;

namespace XXCloudService.Api.HaoKu
{
    /// <summary>
    /// hkreq 的摘要说明
    /// </summary>
    public class hkreq : ApiBase
    {
        #region 用户实名认证
        /// <summary>
        /// 用户实名认证
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object MemberVerify(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string realName = dicParas.ContainsKey("realName") ? dicParas["realName"].ToString() : string.Empty;
                string idCard = dicParas.ContainsKey("idCard") ? dicParas["idCard"].ToString() : string.Empty;
                string phone = dicParas.ContainsKey("phone") ? dicParas["phone"].ToString() : string.Empty;

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.MemberVerify memberInfo = new HaokuData.MemberVerify();
                memberInfo.shopId = storeHK.HKShopID;
                memberInfo.caller = HaokuConfig.Caller;
                memberInfo.realName = AES.AESEncrypt(realName);
                memberInfo.idCard = AES.AESEncrypt(idCard);
                memberInfo.phone = AES.AESEncrypt(phone);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.Verify, memberInfo, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.实名认证成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 开卡
        /// <summary>
        /// 开卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object CreateCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string cardName = dicParas.ContainsKey("cardName") ? dicParas["cardName"].ToString() : string.Empty;
                string realName = dicParas.ContainsKey("realName") ? dicParas["realName"].ToString() : string.Empty;
                string idCard = dicParas.ContainsKey("idCard") ? dicParas["idCard"].ToString() : string.Empty;
                string phone = dicParas.ContainsKey("phone") ? dicParas["phone"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);
                cardInfo.cardName = AES.AESEncrypt(cardName);
                cardInfo.realName = AES.AESEncrypt(realName);
                cardInfo.idCard = AES.AESEncrypt(idCard);
                cardInfo.phone = AES.AESEncrypt(phone);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.CreateCard, cardInfo, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 绑卡
        /// <summary>
        /// 绑卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object BindCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string cardName = dicParas.ContainsKey("cardName") ? dicParas["cardName"].ToString() : string.Empty;
                string realName = dicParas.ContainsKey("realName") ? dicParas["realName"].ToString() : string.Empty;
                string idCard = dicParas.ContainsKey("idCard") ? dicParas["idCard"].ToString() : string.Empty;
                string phone = dicParas.ContainsKey("phone") ? dicParas["phone"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);
                cardInfo.cardName = AES.AESEncrypt(cardName);
                cardInfo.realName = AES.AESEncrypt(realName);
                cardInfo.idCard = AES.AESEncrypt(idCard);
                cardInfo.phone = AES.AESEncrypt(phone);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.BindCard, cardInfo, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 注销卡
        /// <summary>
        /// 注销卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object CancelCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.CancelCard, cardInfo, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.注销卡成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 是否能充值
        /// <summary>
        /// 是否能充值
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object CanCharge(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.CanCharge, cardInfo, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.允许充值))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 充值
        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object ChargeLog(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string amount = dicParas.ContainsKey("amount") ? dicParas["amount"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.ChargeData charge = new HaokuData.ChargeData();
                charge.shopId = storeHK.HKShopID;
                charge.caller = HaokuConfig.Caller;
                charge.cardId = AES.AESEncrypt(cardId);
                charge.amount = AES.AESEncrypt(amount);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.ChargeLog, charge, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.充值成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 获取卡关联的用户
        /// <summary>
        /// 获取卡关联的用户
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object GetMemberListByCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string amount = dicParas.ContainsKey("amount") ? dicParas["amount"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo card = new HaokuData.CardInfo();
                card.shopId = storeHK.HKShopID;
                card.caller = HaokuConfig.Caller;
                card.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.GetMemberListByCard, card, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 代币换积分
        /// <summary>
        /// 代币换积分
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object ChargePoint(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;                
                string memberId = dicParas.ContainsKey("memberId") ? dicParas["memberId"].ToString() : string.Empty;
                string type = dicParas.ContainsKey("type") ? dicParas["type"].ToString() : string.Empty;
                string amount = dicParas.ContainsKey("amount") ? dicParas["amount"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.ChargePointData chargePoint = new HaokuData.ChargePointData();
                chargePoint.shopId = storeHK.HKShopID;
                chargePoint.caller = HaokuConfig.Caller;
                chargePoint.cardId = AES.AESEncrypt(cardId);
                chargePoint.memberId = AES.AESEncrypt(memberId);
                chargePoint.type = AES.AESEncrypt(type);
                chargePoint.amount = AES.AESEncrypt(amount);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.ChargePoint, chargePoint, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.积分转换成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 生成卡绑定二维码
        /// <summary>
        /// 生成卡绑定二维码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object GetBindUrl(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string cardId = "10004143";

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.GetBindUrl, cardInfo, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 绑定设备
        /// <summary>
        /// 绑定设备
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object BindDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string sn = dicParas.ContainsKey("sn") ? dicParas["sn"].ToString() : string.Empty;
                string name = dicParas.ContainsKey("name") ? dicParas["name"].ToString() : string.Empty;
                string machineSn = dicParas.ContainsKey("machineSn") ? dicParas["machineSn"].ToString() : string.Empty;
                string machineName = dicParas.ContainsKey("machineName") ? dicParas["machineName"].ToString() : string.Empty;
                string deviceType = dicParas.ContainsKey("deviceType") ? dicParas["deviceType"].ToString() : string.Empty;
                string dopCode = dicParas.ContainsKey("dopCode") ? dicParas["dopCode"].ToString() : string.Empty;
                string cost = dicParas.ContainsKey("cost") ? dicParas["cost"].ToString() : string.Empty;
                string point = dicParas.ContainsKey("point") ? dicParas["point"].ToString() : string.Empty;

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.DeviceData device = new HaokuData.DeviceData();
                device.shopId = storeHK.HKShopID;
                device.caller = HaokuConfig.Caller;
                device.sn = AES.AESEncrypt(sn);
                device.name = AES.AESEncrypt(name);
                device.machineSn = AES.AESEncrypt(machineSn);
                device.machineName = AES.AESEncrypt(machineName);
                device.deviceType = AES.AESEncrypt(deviceType);
                device.dopCode = AES.AESEncrypt(dopCode);
                device.cost = AES.AESEncrypt(cost);
                device.point = AES.AESEncrypt(point);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.BindDevice, device, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 解绑设备
        /// <summary>
        /// 解绑设备
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object UnbindDevice(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string sn = dicParas.ContainsKey("sn") ? dicParas["sn"].ToString() : string.Empty;
                string machineSn = dicParas.ContainsKey("machineSn") ? dicParas["machineSn"].ToString() : string.Empty;

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.DeviceData device = new HaokuData.DeviceData();
                device.shopId = storeHK.HKShopID;
                device.caller = HaokuConfig.Caller;
                device.sn = AES.AESEncrypt(sn);
                device.machineSn = AES.AESEncrypt(machineSn);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.UnbindDevice, device, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.解绑成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 设备状态更新
        /// <summary>
        /// 设备状态更新
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object UpdateDeviceStatus(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string sn = dicParas.ContainsKey("sn") ? dicParas["sn"].ToString() : string.Empty;
                string status = dicParas.ContainsKey("status") ? dicParas["status"].ToString() : string.Empty;

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.DeviceStatusData device = new HaokuData.DeviceStatusData();
                device.shopId = storeHK.HKShopID;
                device.caller = HaokuConfig.Caller;
                device.sn = AES.AESEncrypt(sn);
                device.status = AES.AESEncrypt(status);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.UpdateDeviceStatus, device, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        
        #region 设备出奖
        /// <summary>
        /// 设备出奖
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object DevicePrize(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string source = dicParas.ContainsKey("source") ? dicParas["source"].ToString() : string.Empty;
                string type = dicParas.ContainsKey("type") ? dicParas["type"].ToString() : string.Empty;
                string num = dicParas.ContainsKey("num") ? dicParas["num"].ToString() : string.Empty;
                string sn = dicParas.ContainsKey("sn") ? dicParas["sn"].ToString() : string.Empty;
                string ext = dicParas.ContainsKey("ext") ? dicParas["ext"].ToString() : string.Empty;

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.DevicePrizeData prize = new HaokuData.DevicePrizeData();
                prize.shopId = storeHK.HKShopID;
                prize.caller = HaokuConfig.Caller;
                prize.source = AES.AESEncrypt(source);
                prize.type = AES.AESEncrypt(type);
                prize.num = AES.AESEncrypt(num);                
                prize.sn = AES.AESEncrypt(sn);
                if (!string.IsNullOrWhiteSpace(ext))
                {
                    prize.ext = AES.AESEncrypt(ext);
                }

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.DevicePrize, prize, 20);

                JsonSerializer serializer = new JsonSerializer();
                StringReader sr = new StringReader(response);
                object o = serializer.Deserialize(new JsonTextReader(sr), typeof(HaokuData.RequestACK));
                HaokuData.RequestACK ack = o as HaokuData.RequestACK;

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.设备出奖成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}