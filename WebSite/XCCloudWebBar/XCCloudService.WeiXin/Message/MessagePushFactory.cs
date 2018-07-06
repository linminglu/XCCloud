﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.WeiXin.Message;
using XCCloudWebBar.WeiXin.Common;

namespace XCCloudWebBar.WeiXin.Message
{
    public class MessagePushFactory
    {
        public static string CreateMsgJson<TConfig, TData>(string openId, TConfig configModel, TData dataModel)
        {
            string SAppPagePath = Utils.GetPropertyValue(configModel, "SAppPagePath").ToString();
            string TemplateId = Utils.GetPropertyValue(configModel, "TemplateId").ToString();
            string DetailsUrl = Utils.GetPropertyValue(configModel, "DetailsUrl").ToString();
            string Params = string.Empty;

            if (!string.IsNullOrEmpty(SAppPagePath))
            {
                var miniprogram = new
                {
                    appid = WeiXinConfig.WXSmallAppId,
                    pagepath = SAppPagePath
                };
                
                var msgData = new
                {
                    touser = openId,
                    template_id = TemplateId,
                    data = GetMsgData<TConfig, TData>(configModel, dataModel, out Params),
                    url = DetailsUrl + (!string.IsNullOrEmpty(Params) ? ("?" + Params) : string.Empty),
                    miniprogram = miniprogram                
                };
                return Utils.SerializeObject(msgData).ToString();
            }
            else
            {
                var msgData = new
                {
                    touser = openId,
                    template_id = TemplateId,
                    data = GetMsgData<TConfig, TData>(configModel, dataModel, out Params),
                    url = DetailsUrl + (!string.IsNullOrEmpty(Params) ? ("?" + Params) : string.Empty)
                };
                return Utils.SerializeObject(msgData).ToString();
            }
        }


        private static object GetMsgData<TConfig, TData>(TConfig configModel, TData dataModel, out string uriParams)
        {
            uriParams = string.Empty;
            if (typeof(TConfig) == typeof(BuySuccessNotifyConfigModel))
            {
                return GetBuySuccessNotifyData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(UserRegisterRemindConfigModel))
            {
                return GetUserRegisterRemindData(configModel, dataModel, out uriParams);
            }
            else if (typeof(TConfig) == typeof(OrderPaySuccessConfigModel))
            {
                return GetOrderPaySuccessData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(OrderFailSuccessConfigModel))
            {
                return GetOrderFailData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(MerchNewPasswordConfigModel))
            {
                return GetMerchNewPasswordData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(MerchResetPasswordConfigModel))
            {
                return GetMerchResetPasswordData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(StoreRegisterRemindConfigModel))
            {
                return GetStoreRegisterRemindData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(XcUserNewPasswordConfigModel))
            {
                return GetXcUserNewPasswordData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(XcUserResetPasswordConfigModel))
            {
                return GetXcUserResetPasswordData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(XcGameGetCoinSuccessConfigModel))
            {
                return GetXcGameGetCoinSuccessData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(MemberRechargeNotifyConfigModel))
            {
                return GetMemberRechargeNotifyData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(PhoneVerifyCodeConfigModel))
            {
                return GetPhoneVerifyCodeData(configModel, dataModel);
            }
            else if (typeof(TConfig) == typeof(DoScheduleConfigModel))
            {
                return GetDoScheduleNotifyData(configModel, dataModel, out uriParams);
            }
            return null;
        }


        private static object GetBuySuccessNotifyData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            BuySuccessNotifyConfigModel config = Utils.GetCopy<BuySuccessNotifyConfigModel>(configModel);
            BuySuccessNotifyDataModel data = Utils.GetCopy<BuySuccessNotifyDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keynote1 = new { value = data.ProductName, color = config.Keynote1Color },
                keynote2 = new { value = data.BuyPrice, color = config.Keynote2Color },
                keynote3 = new { value = data.BuyDate, color = config.Keynote3Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };

            return msgData;
        }

        private static object GetUserRegisterRemindData<TConfig, TData>(TConfig configModel, TData dataModel, out string uriParams)
        {
            uriParams = string.Empty;
            UserRegisterRemindConfigModel config = Utils.GetCopy<UserRegisterRemindConfigModel>(configModel);
            UserRegisterRemindDataModel data = Utils.GetCopy<UserRegisterRemindDataModel>(dataModel);
            var userType = string.Empty;
            switch ((UserType)data.UserType)
            {
                case UserType.Store: { userType = "门店"; break; }
                case UserType.Normal: { userType = "商户"; break; }
                case UserType.Heavy: { userType = "大客户"; break; }
                case UserType.Agent: { userType = "代理商"; break; }
            }
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.UserName, color = config.Keynote1Color },
                keyword2 = new { value = data.RegisterTime, color = config.Keynote2Color },
                remark = new { value = "工单号：" + data.WorkId + "\n" + "用户类型：" + userType + "\n" + data.Message + "\n" + config.Remark, color = config.RemarkColor }
            };
            uriParams = string.Format("workId={0}&userName={1}&message={2}&userType={3}", data.WorkId, Utils.UrlEncode(data.UserName), Utils.UrlEncode(data.Message), data.UserType);           
            return msgData;
        }

        private static object GetStoreRegisterRemindData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            StoreRegisterRemindConfigModel config = Utils.GetCopy<StoreRegisterRemindConfigModel>(configModel);
            StoreRegisterRemindDataModel data = Utils.GetCopy<StoreRegisterRemindDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.MerchAccount, color = config.Keynote1Color },
                keyword2 = new { value = data.RegisterTime, color = config.Keynote2Color },
                remark = new { value = "门店名称：" + data.StoreName + "\n工单号：" + data.WorkId + "\n" + config.Remark, color = config.RemarkColor }
            };

            return msgData;
        }

        private static object GetMerchNewPasswordData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            MerchNewPasswordConfigModel config = Utils.GetCopy<MerchNewPasswordConfigModel>(configModel);
            MerchNewPasswordDataModel data = Utils.GetCopy<MerchNewPasswordDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.UserName, color = config.Keynote1Color },
                keyword2 = new { value = data.Password, color = config.Keynote2Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };
            return msgData;
        }

        private static object GetMerchResetPasswordData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            MerchResetPasswordConfigModel config = Utils.GetCopy<MerchResetPasswordConfigModel>(configModel);
            MerchResetPasswordDataModel data = Utils.GetCopy<MerchResetPasswordDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.UserName, color = config.Keynote1Color },
                keyword2 = new { value = data.Password, color = config.Keynote2Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };
            return msgData;
        }

        private static object GetXcUserNewPasswordData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            XcUserNewPasswordConfigModel config = Utils.GetCopy<XcUserNewPasswordConfigModel>(configModel);
            XcUserNewPasswordDataModel data = Utils.GetCopy<XcUserNewPasswordDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.UserName, color = config.Keynote1Color },
                keyword2 = new { value = data.Password, color = config.Keynote2Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };
            return msgData;
        }

        private static object GetXcUserResetPasswordData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            XcUserResetPasswordConfigModel config = Utils.GetCopy<XcUserResetPasswordConfigModel>(configModel);
            XcUserResetPasswordDataModel data = Utils.GetCopy<XcUserResetPasswordDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.UserName, color = config.Keynote1Color },
                keyword2 = new { value = data.Password, color = config.Keynote2Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };
            return msgData;
        }

        private static object GetOrderPaySuccessData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            OrderPaySuccessConfigModel config = Utils.GetCopy<OrderPaySuccessConfigModel>(configModel);
            OrderPaySuccessDataModel data = Utils.GetCopy<OrderPaySuccessDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.ProductName, color = config.Keynote1Color },
                keyword2 = new { value = data.BuyPrice, color = config.Keynote2Color },
                keyword3 = new { value = data.BuyDate, color = config.Keynote3Color },
                keyword4 = new { value = data.Createtime, color = config.Keynote3Color },
                keyword5 = new { value = data.OrderNumber, color = config.Keynote3Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };

            return msgData;
        }

        private static object GetOrderFailData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            OrderFailSuccessConfigModel config = Utils.GetCopy<OrderFailSuccessConfigModel>(configModel);
            OrderFailSuccessDataModel data = Utils.GetCopy<OrderFailSuccessDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.ProductName, color = config.Keynote1Color },
                keyword2 = new { value = data.BuyPrice, color = config.Keynote2Color },
                keyword3 = new { value = data.BuyDate, color = config.Keynote3Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };

            return msgData;
        }

        private static object GetXcGameGetCoinSuccessData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            XcGameGetCoinSuccessConfigModel config = Utils.GetCopy<XcGameGetCoinSuccessConfigModel>(configModel);
            XcGameGetCoinSuccessDataModel data = Utils.GetCopy<XcGameGetCoinSuccessDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = string.Format("成功提币{0}个", data.Coins), color = config.Keynote1Color },
                keyword2 = new { value = data.OperationDate, color = config.Keynote2Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };

            return msgData;
        }

        private static object GetMemberRechargeNotifyData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            MemberRechargeNotifyConfigModel config = Utils.GetCopy<MemberRechargeNotifyConfigModel>(configModel);
            MemberRechargeNotifyDataModel data = Utils.GetCopy<MemberRechargeNotifyDataModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                accountType = new { value = data.AccountType },
                account = new { value = data.Account, color = config.Keynote1Color },
                amount = new { value = data.Amount, color = config.Keynote2Color },
                result = new { value = data.Status, color = config.Keynote3Color },
                remark = new { value = data.Remark, color = config.RemarkColor }
            };

            return msgData;
        }

        private static object GetPhoneVerifyCodeData<TConfig, TData>(TConfig configModel, TData dataModel)
        {
            PhoneVerifyCodeConfigModel config = Utils.GetCopy<PhoneVerifyCodeConfigModel>(configModel);
            PhoneVerifyCodeModel data = Utils.GetCopy<PhoneVerifyCodeModel>(dataModel);
            var msgData = new
            {
                first = new { value = config.Title, color = config.FirstColor },
                keyword1 = new { value = data.keyword1, color = config.Keynote1Color },
                keyword2 = new { value = data.keyword2, color = config.Keynote2Color },
                remark = new { value = data.remark, color = config.RemarkColor }
            };

            return msgData;
        }

        private static object GetDoScheduleNotifyData<TConfig, TData>(TConfig configModel, TData dataModel, out string uriParams)
        {
            uriParams = string.Empty;
            DoScheduleConfigModel config = Utils.GetCopy<DoScheduleConfigModel>(configModel);
            DoScheduleDataModel data = Utils.GetCopy<DoScheduleDataModel>(dataModel);          
            var msgData = new
            {
                first = new { value = data.ScheduleName + config.Title, color = config.FirstColor },
                keyword1 = new { value = string.Format("{0}年{1}月{2}日 {3}:{4}",data.OpenTime.Year, data.OpenTime.Month, data.OpenTime.Day, data.OpenTime.Hour, data.OpenTime.Minute), color = config.Keynote1Color },
                keyword2 = new { value = string.Format("{0}年{1}月{2}日 {3}:{4}", data.ShiftTime.Year, data.ShiftTime.Month, data.ShiftTime.Day, data.ShiftTime.Hour, data.ShiftTime.Minute), color = config.Keynote2Color },
                keyword3 = new { value = data.PayCount, color = config.Keynote3Color },
                keyword4 = new { value = data.FreePay, color = config.Keynote4Color },
                keyword5 = new { value = data.RealPay, color = config.Keynote5Color },
                remark = new { value = config.Remark, color = config.RemarkColor }
            };
            uriParams = string.Format("scheduleId={0}&userId={1}", data.ScheduleID, data.UserID);
            return msgData;
        }
    }
}