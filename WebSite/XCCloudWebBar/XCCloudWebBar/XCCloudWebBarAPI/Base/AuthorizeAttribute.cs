using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using XCCloudWebBar.Common.Enum;

namespace XCCloudWebBar.Base
{
    public class AuthorizeAttribute:System.Attribute
    {
        public AuthorizeAttribute()
        {
            this.Roles = string.Empty;
            this.Users = string.Empty;
            this.Merches = string.Empty;
            this.Grants = string.Empty;
            this.Inherit = true;
        }

        /// <summary>
        /// 角色
        /// </summary>
        public string Roles { set; get; }

        /// <summary>
        /// 用户
        /// </summary>
        public string Users { get; set; }

        /// <summary>
        /// 商户
        /// </summary>
        public string Merches { set; get; }

        /// <summary>
        /// 操作权限
        /// </summary>
        public string Grants { set; get; }

        /// <summary>
        /// 是否继承
        /// </summary>
        public bool Inherit { set; get; }

    }

    public class AllowAnonymousAttribute : System.Attribute
    {
        public AllowAnonymousAttribute()
        {
            
        }        

    }
}