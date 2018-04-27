using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Common
{
    /// <summary>
    /// 参数验证
    /// </summary>
    public static class Valid
    {        
        /// <summary>
        /// 检查对象是否为空或字符串是否为空字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns></returns>
        private static bool IsNull(object o)
        {
            return ((o == null) || (((o.GetType() == typeof(string)) && string.IsNullOrEmpty(o.ToString().Trim())) || (o.GetType() == typeof(DBNull))));
        }
        /// <summary>
        /// 非空验证
        /// </summary>
        /// <param name="o"></param>
        /// <param name="discription"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool Nonempty<T>(this T t, string discription, out string errMsg)
        {
            errMsg = string.Empty;

            if (IsNull(t))
            {
                errMsg = discription + "不能为空";
                return false;
            }

            return true;
        }
        /// <summary>
        /// 非负整数验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="discription"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool Illegalint<T>(this T t, string discription, out string errMsg)
        {
            errMsg = string.Empty;

            if (!Nonempty(t, discription, out errMsg))
            {
                return false;
            }

            var value = 0;
            if (!int.TryParse(t.ToString(), out value) || value < 0)
            {
                errMsg = discription + "格式不正确，须为非负整数";
                return false;
            }

            return true;
        }
        /// <summary>
        /// 非负小数验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="discription"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool Illegaldec<T>(this T t, string discription, out string errMsg)
        {
            errMsg = string.Empty;

            if (!Nonempty(t, discription, out errMsg))
            {
                return false;
            }

            var value = 0M;
            if (!decimal.TryParse(t.ToString(), out value) || value < 0)
            {
                errMsg = discription + "格式不正确，须为非负数";
                return false;
            }

            return true;
        }    
    }
}
