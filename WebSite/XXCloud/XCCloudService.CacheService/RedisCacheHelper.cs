using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.Common.Redis;

namespace XCCloudService.CacheService
{
    public class RedisCacheHelper
    {
        #region 生成流水号
        const string SerialNoDateKey = "SerialNoDateKey";
        const string SerialNoKey = "SerialNoKey";
        private static LoadedLuaScript luaScript { get; set; }

        public static string CreateSerialNo(string storeId)
        {
            try
            {
                RedisHelper redisHelper = new RedisHelper();

                string date = DateTime.Now.ToString("yyyyMMdd");
                IServer server = redisHelper.GetRedisServer();
                IDatabase db = redisHelper.GetDatabase();

                string strLuaScript =
                    " local currDate = tostring(@currDate) " +
                    " if not currDate then " +
                    "     return 0 " +
                    " end " +
                    " local vals = redis.call(\"HMGET\", @SerialNoKey, \"SerialNo\", \"RedisDate\"); " +
                    " local SerialNo = tonumber(vals[1]) " +
                    " local redisDate = vals[2] " +
                    " if redisDate ~= currDate then " +
                    "     redis.call(\"HMSET\", @SerialNoKey, \"SerialNo\", 1, \"RedisDate\", currDate); " +
                    "     SerialNo = 1; " +
                    " end " +
                    " if not SerialNo then " +
                    "     return 0 " +
                    " end " +
                    " redis.call(\"HINCRBY\", @SerialNoKey, \"SerialNo\", 1) " +
                    " return SerialNo";

                if (luaScript == null || !server.ScriptExists(luaScript.Hash))
                {
                    var prepared = LuaScript.Prepare(strLuaScript);
                    luaScript = prepared.Load(server);
                }

                RedisResult ret = luaScript.Evaluate(db, new { currDate = date, SerialNoDateKey = SerialNoDateKey, SerialNoKey = SerialNoKey });

                string serialNo = string.Empty;
                if (ret.IsNull || string.IsNullOrEmpty(ret.ToString()) || ret.ToString() == "0")
                {
                    return "";
                }

                serialNo = storeId + date + ret.ToString().PadLeft(9, '0');
                return serialNo;
            }
            catch
            {
                return "";
            }
        } 
        #endregion

        #region key
        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public static bool KeyDelete(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.KeyDelete(key);
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public static long KeyDelete(List<string> keys)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.KeyDelete(keys);
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public static bool KeyExists(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.KeyExists(key);
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.KeyRename(key, newKey);
        }

        /// <summary>
        /// 设置Key的过期时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.KeyExpire(key, expiry);
        }
        #endregion

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
        public static bool HashSet(string key, string dataKey, string str)
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
        public static bool HashSet<T>(string key, string dataKey, T t)
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
        public static bool HashDelete(string key, string dataKey)
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
        public static long HashDelete(string key, List<RedisValue> dataKeys)
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
        public static T HashGet<T>(string key, string dataKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashGet<T>(key, dataKey);
        }

        /// <summary>
        /// 从hash表获取所有Field/Value集合。
        /// HashEntry：Name为字段名，Value为值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static HashEntry[] HashGetAll(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashGetAll(key);
        }

        /// <summary>
        /// 从hash表获取所有数据集合，不包含Field。
        /// 适用于缓存value为JSON的情况
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<T> HashGetAll<T>(string key)
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
        public static List<T> HashGet<T>(string key, RedisValue[] dataKey)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashGetByFields<T>(key, dataKey);
        }

        /// <summary>
        /// 获取hash表所有的Field Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<string> HashKeys(string key)
        {
            RedisHelper redisHelper = new RedisHelper();
            return redisHelper.HashKeys(key);
        } 
        #endregion


    }
}
