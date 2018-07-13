using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCCloud.User;
using XCCloudWebBar.Model.XCCloud;

namespace XCCloudWebBar.Business.XCCloud
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
