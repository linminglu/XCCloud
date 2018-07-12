using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCCloud.User;
using XCCloudService.Model.XCCloud;

namespace XCCloudService.Business.XCCloud
{
    public class UserBusiness
    {
        public const string userInfoCacheKey = "redisXCUserInfoCacheKey";

        public static List<UserInfoCacheModel> XcUserInfoList 
        {
            get 
            {
                List<UserInfoCacheModel> List = RedisCacheHelper.HashGetAll<UserInfoCacheModel>(userInfoCacheKey);
                return List;
            }
        }

        public static void AddCache(string openId, UserInfoCacheModel model)
        {
            RedisCacheHelper.HashSet<UserInfoCacheModel>(userInfoCacheKey, openId, model);
        }

        public static void XcUserInit()
        {
            IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>();
            var list = base_UserInfoService.GetModels(p => p.UserType == (int)UserType.Xc).ToList();
            foreach (var item in list)
            {
                RedisCacheHelper.HashSet<UserInfoCacheModel>(userInfoCacheKey, item.OpenID, new UserInfoCacheModel() { OpenID = item.OpenID, UserID = item.ID });
            }
        }

        public static void Clear()
        {
            RedisCacheHelper.KeyDelete(userInfoCacheKey);  
        }

        public static bool IsEffectiveXcUser(string openId, out UserInfoCacheModel userInfoCacheModel)
        {
            userInfoCacheModel = null;
            if (RedisCacheHelper.HashExists(userInfoCacheKey, openId))
            {
                userInfoCacheModel = RedisCacheHelper.HashGet<UserInfoCacheModel>(userInfoCacheKey, openId);
                return true;
            }
            return false;
        }        
    }
}
