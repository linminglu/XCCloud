using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XCCloudWebBar.Base
{
    /// <summary>
    /// 验证签名模式
    /// </summary>
    public enum SignKeyEnum
    {
        /// <summary>
        /// 使用会员token做验签
        /// </summary>
        XCGameMemberToken = 2,
        /// <summary>
        /// 使用手机token做验签
        /// </summary>
        MobileToken = 3,
        /// <summary>
        /// 使用会员token做验签或手机token做验签
        /// </summary>
        XCGameMemberOrMobileToken = 4,
        /// <summary>
        /// 方法token(方法体内验证token)
        /// </summary>
        MethodToken = 5,
        /// <summary>
        /// 使用狗号做验签
        /// </summary>
        DogNoToken = 6, 
        /// <summary>
        /// 用户token
        /// </summary>
        XCGameUserCacheToken=7,
        /// <summary>
        /// 云运营平台用户token
        /// </summary>
        XCCloudUserCacheToken = 8,
        /// <summary>
        /// 用户token(莘助手)
        /// </summary>
        XCManaUserHelperToken = 9,
        /// <summary>
        /// XCGameManaDB表管理权限
        /// </summary>
        XCGameAdminToken = 10, 
        /// <summary>
        /// XCGameManaDB表系统管理权限（t_AdminUser用户权限）
        /// </summary>
        XCGameManamAdminUserToken = 11
    }

    public enum RespDataTypeEnum
    { 
        Json = 0,
        ImgStream = 1
    }
}