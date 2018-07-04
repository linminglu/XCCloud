using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XCCloudWebBar.Common
{
    public class Constant
    {
        /// <summary>
        /// 会员token
        /// </summary>
        public static string XCGameMemberTokenModel = "XCGameMemberTokenModel";

        /// <summary>
        /// 云运营平台用户token
        /// </summary>
        public static string XCCloudUserTokenModel = "XCCloudUserTokenModel";

        /// <summary>
        /// 手机token
        /// </summary>
        public static string MobileTokenModel = "MobileTokenModel";

        /// <summary>
        /// 微信访问token
        /// </summary>
        public static string WeiXinAccessToken = "WeiXinAccessToken";

        public static string SAppAccessToken = "SAppAccessToken";

        /// <summary>
        /// 微信用户openId
        /// </summary>
        public static string WeiXinOpenId = "WeiXinOpenId";

        /// <summary>
        /// 微信用户unionId
        /// </summary>
        public static string WeiXinUnionId = "WeiXinUnionId";

        public static string TCPSocketClinetForUDPOuputCoin = "TCPSocketClinetForUDPOuputCoin";

        public static string WeiXinSAppSession = "WeiXinSAppSession";

        public static string InvalidStore = "InvalidStore";

        public static string XCManaUserHelperToken = "XCManaUserHelperToken";

        public static string XCGameManamAdminUserToken = "XCGameManamAdminUserToken";

        public static string ImageFileType = "Image";//图片文件类型

        public static string MoveFileType = "Move";//视频文件类型

        public static string OtherFileType = "Other";//其他文件类型

        public static string WX_Auth_OrderAudit = "WX_Auth_OrderAudit";//微信订单审核

        #region "Dict_System权限配置常量"

        public static string SystemRole_OrderAudit = "订单审核授权";

        #endregion

        public static string Workflow = "Workflow";//工作流
    }
}