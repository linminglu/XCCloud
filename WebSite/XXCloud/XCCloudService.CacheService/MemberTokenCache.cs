using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Model.WeiXin;

namespace XCCloudService.CacheService
{
    public class MemberTokenCache
    {
        public const string memberTokenCacheKey = "redisMemberTokenCacheKey";

        public static void AddToken(string token, MemberTokenModel model)
        {
            RedisCacheHelper.HashSet<MemberTokenModel>(memberTokenCacheKey, token, model);
        }

        public static MemberTokenModel GetModel(string token)
        {
            MemberTokenModel model = RedisCacheHelper.HashGet<MemberTokenModel>(memberTokenCacheKey, token);
            return model;
        }
    }
}
