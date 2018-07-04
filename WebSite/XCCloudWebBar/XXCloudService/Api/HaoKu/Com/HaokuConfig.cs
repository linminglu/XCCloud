using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XXCloudService.Api.HaoKu.Com
{
    public enum ResponseCode
    {
        Success = 2000000,
        操作成功 = 100,
        实名认证成功 = 110,
        允许充值 = 120,
        充值成功 = 122,
        注销卡成功 = 130,
        绑定卡成功 = 132,
        积分转换成功 = 140,
        解绑成功 = 154,
        设备出奖成功 = 158
    }

    public class HaokuConfig
    {
        //供应商名
        public const string Caller = "xinchen";

        //供应商的密钥
        public const string CallerSecret = "3567523162";

        #region 请求的URL
        /// <summary>
        /// 网关地址
        /// </summary>
        public const string hostUrl = "http://api.mcs.uwan99.com/";

        /// <summary>
        /// 实名认证
        /// </summary>
        public static string Verify
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "verifyapi/verify");
            }
        }

        /// <summary>
        /// 开卡
        /// </summary>
        public static string CreateCard
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/createcard");
            }
        }

        /// <summary>
        /// 绑卡
        /// </summary>
        public static string BindCard
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/bindcard");
            }
        }

        /// <summary>
        /// 绑卡
        /// </summary>
        public static string CancelCard
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/cancelcard");
            }
        }

        /// <summary>
        /// 是否能充值
        /// </summary>
        public static string CanCharge
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/cancharge");
            }
        }

        /// <summary>
        /// 充值
        /// </summary>
        public static string ChargeLog
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/chargelog");
            }
        }

        /// <summary>
        /// 获取卡关联的用户
        /// </summary>
        public static string GetMemberListByCard
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/getmemberlistbycard");
            }
        }

        /// <summary>
        /// 代币换积分
        /// </summary>
        public static string ChargePoint
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/chargepoint");
            }
        }

        /// <summary>
        /// 生成卡绑定二维码
        /// </summary>
        public static string GetBindUrl
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "accountapi/getbindurl");
            }
        }

        /// <summary>
        /// 绑定设备
        /// </summary>
        public static string BindDevice
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "deviceapi/binddevice");
            }
        }

        /// <summary>
        /// 解绑设备
        /// </summary>
        public static string UnbindDevice
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "deviceapi/unbinddevice");
            }
        }

        /// <summary>
        /// 设备状态更新
        /// </summary>
        public static string UpdateDeviceStatus
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "deviceapi/updatedevicestatus");
            }
        }

        /// <summary>
        /// 设备出奖
        /// </summary>
        public static string DevicePrize
        {
            get
            {
                return string.Format("{0}{1}", hostUrl, "deviceapi/deviceprize");
            }
        }
        #endregion
    }
}