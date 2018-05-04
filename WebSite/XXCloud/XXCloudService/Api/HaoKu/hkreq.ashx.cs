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
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);
                cardInfo.cardName = AES.AESEncrypt(cardName);
                if (!string.IsNullOrWhiteSpace(realName))
                {
                    cardInfo.realName = AES.AESEncrypt(realName);
                }
                if (!string.IsNullOrWhiteSpace(idCard))
                {
                    cardInfo.idCard = AES.AESEncrypt(idCard);
                }  
                cardInfo.phone = AES.AESEncrypt(phone);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.CreateCard, cardInfo, 20);
                HaokuData.BindCardACK ack = JsonConvert.DeserializeObject<HaokuData.BindCardACK>(response);

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                if (string.IsNullOrWhiteSpace(cardInfo.idCard))
                {
                    HaokuViewModel.BindViewModel model = new HaokuViewModel.BindViewModel();
                    model.Id = ack.data.data.id;
                    model.QRCodeUrl = "";
                    return ResponseModelFactory<HaokuViewModel.BindViewModel>.CreateModel(isSignKeyReturn, model);
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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);
                cardInfo.cardName = AES.AESEncrypt(cardName);
                if (!string.IsNullOrWhiteSpace(realName))
                {
                    cardInfo.realName = AES.AESEncrypt(realName);
                }
                if (!string.IsNullOrWhiteSpace(idCard))
                {
                    cardInfo.idCard = AES.AESEncrypt(idCard);
                }                
                cardInfo.phone = AES.AESEncrypt(phone);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.BindCard, cardInfo, 20);
                HaokuData.BindCardACK ack = JsonConvert.DeserializeObject<HaokuData.BindCardACK>(response);

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.code != (int)ResponseCode.操作成功))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, ack.data != null ? ack.data.description : ack.statusMsg);
                }

                if (string.IsNullOrWhiteSpace(cardInfo.idCard))
                {
                    HaokuViewModel.BindViewModel model = new HaokuViewModel.BindViewModel();
                    model.Id = ack.data.data.id;
                    model.QRCodeUrl = ack.data.data.qrCodeUrl;
                    return ResponseModelFactory<HaokuViewModel.BindViewModel>.CreateModel(isSignKeyReturn, model);
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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.CancelCard, cardInfo, 20);
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.CanCharge, cardInfo, 20);
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.ChargeData charge = new HaokuData.ChargeData();
                charge.shopId = storeHK.HKShopID;
                charge.caller = HaokuConfig.Caller;
                charge.cardId = AES.AESEncrypt(cardId);
                charge.amount = AES.AESEncrypt(amount);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.ChargeLog, charge, 20);
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo card = new HaokuData.CardInfo();
                card.shopId = storeHK.HKShopID;
                card.caller = HaokuConfig.Caller;
                card.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.GetMemberListByCard, card, 20);
                HaokuData.MemberListACK ack = JsonConvert.DeserializeObject<HaokuData.MemberListACK>(response);

                if (ack.statusCode != (int)ResponseCode.Success || (ack.data != null && ack.data.list.Count == 0))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该卡号未绑定用户");
                }

                List<HaokuViewModel.MemberListViewModel> model = new List<HaokuViewModel.MemberListViewModel>();
                foreach (HaokuData.MemberListDetail item in ack.data.list)  
                {
                    model.Add(new HaokuViewModel.MemberListViewModel() { memberId = item.memberId, nickName = item.nickname });
                }

                return ResponseModelFactory<List<HaokuViewModel.MemberListViewModel>>.CreateModel(isSignKeyReturn, model);
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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

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
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                //string cardId = memberTokenModel.ICCardId;
                string storeId = "100025420106001";
                string cardId = dicParas["cardId"].ToString();

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.CardInfo cardInfo = new HaokuData.CardInfo();
                cardInfo.shopId = storeHK.HKShopID;
                cardInfo.caller = HaokuConfig.Caller;
                cardInfo.cardId = AES.AESEncrypt(cardId);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.GetBindUrl, cardInfo, 20);
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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
                string strDeviceId = dicParas.ContainsKey("deviceId") ? dicParas["deviceId"].ToString() : string.Empty;

                int deviceId = 0;
                if (string.IsNullOrWhiteSpace(strDeviceId) || !int.TryParse(strDeviceId, out deviceId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备号错误");
                }

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels(t => t.ID == Convert.ToInt32(deviceId)).FirstOrDefault();
                if (device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备不存在");
                }

                Data_GameInfo game = Data_GameInfoService.I.GetModels(t => t.ID == device.GameIndexID).FirstOrDefault();
                if (game == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "游戏机不存在");
                }

                Dict_System ds = Dict_SystemService.I.GetModels(t => t.ID == 0).FirstOrDefault();
                if (ds == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取积分兑换比例失败");
                }

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.DeviceData bindData = new HaokuData.DeviceData();
                bindData.shopId = storeHK.HKShopID;
                bindData.caller = HaokuConfig.Caller;
                bindData.sn = AES.AESEncrypt(device.MCUID);
                bindData.name = AES.AESEncrypt(device.DeviceName);
                bindData.machineSn = AES.AESEncrypt(game.GameID);
                bindData.machineName = AES.AESEncrypt(game.GameName);
                bindData.deviceType = AES.AESEncrypt("4"); //默认 游乐设备
                bindData.dopCode = AES.AESEncrypt(device.MCUID);
                bindData.cost = AES.AESEncrypt(game.PushReduceFromCard.ToString());
                bindData.point = AES.AESEncrypt(ds.DictKey);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.BindDevice, bindData, 20);
                HaokuData.BindDeviceACK ack = JsonConvert.DeserializeObject<HaokuData.BindDeviceACK>(response);

                if (ack.statusCode != (int)ResponseCode.Success || string.IsNullOrWhiteSpace(ack.data.sceneUrl))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备绑定失败");
                }

                device.BarCode = ack.data.sceneUrl;
                bool ret = Base_DeviceInfoService.I.Update(device);
                if (!ret)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备绑定失败");
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
                string strDeviceId = dicParas.ContainsKey("deviceId") ? dicParas["deviceId"].ToString() : string.Empty;
                int deviceId = 0;
                if (string.IsNullOrWhiteSpace(strDeviceId) || !int.TryParse(strDeviceId, out deviceId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备号错误");
                }

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels(t => t.ID == Convert.ToInt32(deviceId)).FirstOrDefault();
                if (device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备不存在");
                }

                Data_GameInfo game = Data_GameInfoService.I.GetModels(t => t.ID == device.GameIndexID).FirstOrDefault();
                if (game == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "游戏机不存在");
                }

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.DeviceData deviceData = new HaokuData.DeviceData();
                deviceData.shopId = storeHK.HKShopID;
                deviceData.caller = HaokuConfig.Caller;
                deviceData.sn = AES.AESEncrypt(device.MCUID);
                deviceData.machineSn = AES.AESEncrypt(game.GameID);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.UnbindDevice, deviceData, 20);
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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
                string status = dicParas.ContainsKey("status") ? dicParas["status"].ToString() : string.Empty;
                string strDeviceId = dicParas.ContainsKey("deviceId") ? dicParas["deviceId"].ToString() : string.Empty;

                int deviceId = 0;
                if (string.IsNullOrWhiteSpace(strDeviceId) || !int.TryParse(strDeviceId, out deviceId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备号错误");
                }

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels(t => t.ID == Convert.ToInt32(deviceId)).FirstOrDefault();
                if (device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备不存在");
                }

                //XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                //string storeId = memberTokenModel.StoreId;
                string storeId = "100025420106001";

                Base_StoreHKConfig storeHK = Base_StoreHKConfigService.I.GetModels(s => s.StoreID == storeId).FirstOrDefault();

                AES.Key = HaokuConfig.CallerSecret + storeHK.HKStoreSecret;

                HaokuData.DeviceStatusData deviceData = new HaokuData.DeviceStatusData();
                deviceData.shopId = storeHK.HKShopID;
                deviceData.caller = HaokuConfig.Caller;
                deviceData.sn = AES.AESEncrypt(device.MCUID);
                deviceData.status = AES.AESEncrypt(status);

                HaokuAPI api = new HaokuAPI();
                string response = api.Get(HaokuConfig.UpdateDeviceStatus, deviceData, 20);
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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
                HaokuData.RequestACK ack = JsonConvert.DeserializeObject<HaokuData.RequestACK>(response);

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