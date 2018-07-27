using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Common
{
    public class CommonConfig
    {
        /// <summary>
        /// 短信用户名
        /// </summary>
        public static string SMSName = System.Configuration.ConfigurationManager.AppSettings["SmsName"] ?? "";
        /// <summary>
        /// 短信用户密码
        /// </summary>
        public static string SMSPassWord = System.Configuration.ConfigurationManager.AppSettings["SmsPassWord"] ?? "";
        /// <summary>
        /// 文本日志路径
        /// </summary>
        public static string TxtLogPath = System.Configuration.ConfigurationManager.AppSettings["TxtLogPath"] ?? "";
        /// <summary>
        /// 系统初始化日志
        /// </summary>
        public static string SystemInitLog = "Init/";

        public static int IconOutLockPerSecond = int.Parse(System.Configuration.ConfigurationManager.AppSettings["IconOutLockPerSecond"] ?? "10");
        /// <summary>
        /// 查询订单数量
        /// </summary>
        public static string DataOrderPageSize = System.Configuration.ConfigurationManager.AppSettings["DataOrderPageSize"] ?? "20";

        
        public static string DesKey = System.Configuration.ConfigurationManager.AppSettings["DesKey"] ?? "";

        /// <summary>
        /// 缓存key前缀
        /// </summary>
        public static string PrefixKey = "weChat";

        public static bool IsVerifyCode
        {
            get
            {
                string val = ConfigurationManager.AppSettings["isVerifyCode"] ?? "true";
                bool isVerify = true;
                bool.TryParse(val, out isVerify);
                return isVerify;
            }
        }


        public static string SAppMessagePushXmlFilePath = System.Web.HttpContext.Current.Server.MapPath("/Config/SAppMessageTemplate.xml");

        /// <summary>
        /// 雷达超时
        /// </summary>
        public static int RadarOffLineTimeLong = int.Parse(System.Configuration.ConfigurationManager.AppSettings["radarOffLineTimeLong"] ?? "60");
        /// <summary>
        /// H5微信授权跳转页面
        /// </summary>        
        public static string H5WeiXinAuthRedirectUrl = System.Configuration.ConfigurationManager.AppSettings["H5WeiXinAuthRedirectUrl"];

        public static string H5_M_WeiXinAuthRedirectUrl = System.Configuration.ConfigurationManager.AppSettings["H5_M_WeiXinAuthRedirectUrl"];
        /// <summary>
        /// 微信订单审核跳转页面
        /// </summary>
        public static string WX_Auth_OrderAuditRedirectUrl = System.Configuration.ConfigurationManager.AppSettings["WX_Auth_OrderAuditRedirectUrl"];

        public static string AddWatch_FileServerPhysicsPath = System.Configuration.ConfigurationManager.AppSettings["AddWatch_FileServerPhysicsPath"];
        /// <summary>
        /// 添加码表文件上次传格式
        /// </summary>
        public static string AddWatch_FileFormat = System.Configuration.ConfigurationManager.AppSettings["AddWatch_FileFormat"];
        
        /// <summary>
        /// 图片服务器Host
        /// </summary>
        public static string ImageFileHost = System.Configuration.ConfigurationManager.AppSettings["ImageFileHost"];

        /// <summary>
        /// 视频服务器Host
        /// </summary>
        public static string MovieFileHost = System.Configuration.ConfigurationManager.AppSettings["MovieFileHost"];

        /// <summary>
        /// 莘拍档H5支付宝支付回调
        /// </summary>
        public static string PayTitle
        {
            get
            {
                return ConfigurationManager.AppSettings["PayTitle"];
            }
        }

        /// <summary>
        /// 微信公众号AccessToken键名
        /// </summary>
        public static string WxPubAccessToken
        {
            get
            {
                return "WxPub_AccessToken";
            }
        }

        /// <summary>
        /// 微信公众号JsApi_Ticket键名
        /// </summary>
        public static string WxPubApiTicket
        {
            get
            {
                return "WxPub_ApiTicket";
            }
        }

        /// <summary>
        /// UPD订单号与sn缓存KEY
        /// </summary>
        public static string UdpOrderSNCache = "UPD_ORDERID_SN";

        /// <summary>
        /// 设备缓存key
        /// </summary>
        public static string DeviceStateKey = "DeviceStateRedisKey";
        /// <summary>
        /// 门店UDP数据缓存
        /// </summary>
        public static string StoreCommandCache = "StoreCommandCache";
    }
}
