using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;

namespace XCCloudService.Common.Extensions
{
    public static class StringExt
    {
        /// <summary>
        /// 判断字符串是否可以转换为Int类型
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsInt(this string content)
        {
            try
            {
                int var1 = Convert.ToInt32(content);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 生成扩展门店ID
        /// </summary>
        /// <param name="content"></param>
        /// <param name="roleType"></param>
        /// <param name="merchId"></param>
        /// <returns></returns>
        public static string ToExtStoreID(this string content)
        {
            return (content ?? "").PadRight(15, '0');
        }
    }
}
