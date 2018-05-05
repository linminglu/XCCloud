using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Redis;

namespace XCCloudService.CacheService
{
    public class RedisCacheHelper
    {
        #region String
        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间，可不传</param>
        /// <returns></returns>
        public static bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static bool StringSet<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.StringSet<T>(key, obj, expiry);
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="keyValues">键值对集合</param>
        /// <returns></returns>
        public bool StringSet(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.StringSet(keyValues);
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public static string StringGet(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.StringGet(key);
        }

        /// <summary>
        /// 获取多个Key的值
        /// </summary>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public static RedisValue[] StringGet(List<string> listKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.StringGet(listKey);
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T StringGet<T>(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.StringGet<T>(key);
        } 
        #endregion

        #region Hash
        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key">缓存key</param>
        /// <param name="dataKey">hash项索引</param>
        /// <returns></returns>
        public static bool HashExists(string key, string dataKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashExists(key, dataKey);
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool HashSet(string key, string dataKey, string str)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashSet(key, dataKey, str);
        }

        /// <summary>
        /// 存储对象数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="dataKey">hash项索引</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string dataKey, T t)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashSet<T>(key, dataKey, t);
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashDelete(string key, string dataKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashDelete(key, dataKey);
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public long HashDelete(string key, List<RedisValue> dataKeys)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashDelete(key, dataKeys);
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string dataKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashGet<T>(key, dataKey);
        }

        /// <summary>
        /// 从hash表获取所有数据集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> HashGetAll<T>(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashGetAll<T>(key);
        }

        /// <summary>
        /// 从hash表获取多个字段的数据集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public List<T> HashGet<T>(string key, RedisValue[] dataKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashGetByFields<T>(key, dataKey);
        }

        /// <summary>
        /// 获取hash表所有的Field Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> HashKeys(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashKeys(key);
        } 
        #endregion


    }
}
