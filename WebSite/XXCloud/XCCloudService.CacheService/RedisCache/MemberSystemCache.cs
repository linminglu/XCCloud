using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.CacheService.RedisCache
{
    public class MemberSystemCache
    {
        public bool Init()
        {
            //存储方式：
            //表名 hashKey
            //字段       值
            //会员ID     序列化的会员信息JSON字符串

            //实现代码：
            //RedisStackExchangeHelper redisHelper = new RedisStackExchangeHelper();
            //List<HashEntry> memberList = new List<HashEntry>();
            //List<HashEntry> list = new List<HashEntry>();
            //foreach (var item in memberList)
            //{
            //    list.Add(new HashEntry(item.Id, item.ToJSON()));
            //}
            //redisHelper.HashSet(HashKey, list.ToArray());



            //string strSql = "SELECT * FROM Base_MemberInfo";
            //List<MemberEntity> list = null;

            //using (SqlConnection connection = new SqlConnection(DataAccess.GetConnString()))
            //{
            //    list = connection.Query<MemberEntity>(strSql).ToList();
            //}
            return true;
        }
    }
}
