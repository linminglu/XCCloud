using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Common.Extensions
{
    /// <summary>
    /// Object扩展类
    /// </summary>
    public static class ObjectExt
    {
        /// <summary>
        /// 检查对象是否为空或字符串是否为空字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns></returns>
        public static bool IsNull(this object o)
        {
            return ((o == null) || (((o.GetType() == typeof(string)) && string.IsNullOrEmpty(o.ToString().Trim())) || (o.GetType() == typeof(DBNull))));
        }
        /// <summary>
        /// 将泛型转换为string型数字（失败返回defaultvalue）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <param name="defaultvalue">转换失败或无法转换时的默认值</param>
        /// <returns></returns>
        public static string Tostring<T>(this T key, string defaultvalue)
        {
            if (key.IsNull()) return defaultvalue;
            return Convert.ToString(key);
        }
        /// <summary>
        /// 将泛型转换为string型数字（失败返回null）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <returns></returns>
        public static string Tostring<T>(this T key)
        {
            if (key.IsNull()) return null;
            return Convert.ToString(key);
        }
        /// <summary>
        /// 将泛型转换为int型数字（失败返回defaultvalue）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <param name="defaultvalue">转换失败或无法转换时的默认值</param>
        /// <returns></returns>
        public static int Toint<T>(this T key, int defaultvalue)
        {
            if (key.IsNull()) return defaultvalue;
            if (int.TryParse(key.ToString(), out defaultvalue))
                return defaultvalue;
            return defaultvalue;
        }
        /// <summary>
        /// 将泛型转换为int型数字（失败返回null）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <returns></returns>
        public static int? Toint<T>(this T key)
        {
            if (key.IsNull()) return null;
            var id = 0;
            if (int.TryParse(key.ToString(), out id))
                return id;
            return null;
        }
        /// <summary>
        /// 将泛型转换为decimal型数字（失败返回defaultvalue）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <param name="defaultvalue">转换失败或无法转换时的默认值</param>
        /// <returns></returns>
        public static decimal Todecimal<T>(this T key, decimal defaultvalue)
        {
            if (key.IsNull()) return defaultvalue;
            if (decimal.TryParse(key.ToString(), out defaultvalue))
                return defaultvalue;
            return defaultvalue;
        }
        /// <summary>
        /// 将泛型转换为decimal型数字（失败返回null）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <returns></returns>
        public static decimal? Todecimal<T>(this T key)
        {
            if (key.IsNull()) return null;
            var id = 0M;
            if (decimal.TryParse(key.ToString(), out id))
                return id;
            return null;
        }
        /// <summary>
        /// 将泛型转换为datetime型数字（失败返回defaultvalue）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <param name="defaultvalue">转换失败或无法转换时的默认值</param>
        /// <returns></returns>
        public static DateTime Todatetime<T>(this T key, DateTime defaultvalue)
        {
            if (key.IsNull()) return defaultvalue;
            if (DateTime.TryParse(key.ToString(), out defaultvalue))
                return defaultvalue;
            return defaultvalue;
        }
        /// <summary>
        /// 将泛型转换为datetime型数字（失败返回null）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <returns></returns>
        public static DateTime? Todatetime<T>(this T key)
        {
            if (key.IsNull()) return null;
            var date = new DateTime();
            if (DateTime.TryParse(key.ToString(), out date))
                return date;
            return null;
        }
        /// <summary>
        /// 将泛型转换为date型数字（失败返回defaultvalue）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <param name="defaultvalue">转换失败或无法转换时的默认值</param>
        /// <returns></returns>
        public static DateTime Todate<T>(this T key, DateTime defaultvalue)
        {
            if (key.IsNull()) return Convert.ToDateTime(defaultvalue.ToShortDateString());
            if (DateTime.TryParse(key.ToString(), out defaultvalue))
                return Convert.ToDateTime(defaultvalue.ToShortDateString());
            return Convert.ToDateTime(defaultvalue.ToShortDateString());
        }
        /// <summary>
        /// 将泛型转换为date型数字（失败返回null）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <returns></returns>
        public static DateTime? Todate<T>(this T key)
        {
            if (key.IsNull()) return null;
            var date = new DateTime();
            if (DateTime.TryParse(key.ToString(), out date))
                return Convert.ToDateTime(date.ToShortDateString());
            return null;
        }

        /// <summary>
        /// 将泛型转换为timespan型数字（失败返回defaultvalue）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <param name="defaultvalue">转换失败或无法转换时的默认值</param>
        /// <returns></returns>
        public static TimeSpan Totimespan<T>(this T key, TimeSpan defaultvalue)
        {
            if (key.IsNull()) return defaultvalue;
            return Utils.StrToTimeSpan(key.ToString(), defaultvalue);           
        }
        /// <summary>
        /// 将泛型转换为timespan型数字（失败返回null）
        /// </summary>
        /// <typeparam name="T">转换前的类型</typeparam>
        /// <param name="key">转换前的值</param>
        /// <returns></returns>
        public static TimeSpan? Totimespan<T>(this T key)
        {
            if (key.IsNull()) return null;
            return Utils.StrToTimeSpan(key.ToString());
        }
    }
}
