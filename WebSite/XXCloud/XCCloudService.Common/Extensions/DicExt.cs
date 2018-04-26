using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Common.Extensions
{
    /// <summary>
    /// 字典扩展类
    /// </summary>
    public static class DicExt
    {
        /// <summary>
        /// 返回键值
        /// </summary>
        /// <param name="enumSubitem"></param>
        /// <returns></returns>
        public static string Get(this Dictionary<string, object> dicPara, string key)
        {
            if (dicPara == null || string.IsNullOrEmpty(key)) return string.Empty;
            object o = null;
            dicPara.TryGetValue(key, out o);
            return Convert.ToString(o);
        }
    }
}
