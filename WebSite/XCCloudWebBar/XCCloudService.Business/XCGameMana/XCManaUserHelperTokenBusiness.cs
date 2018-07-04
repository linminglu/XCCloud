using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.CacheService.XCGameMana;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.XCGameManager;

namespace XCCloudWebBar.Business.XCGameMana
{
    public class XCManaUserHelperTokenBusiness
    {
        public static string SetToken(string mobile,string storeId,string storeName,int userId)
        {
            //设置会员token
            string newToken = System.Guid.NewGuid().ToString("N");
            string token = string.Empty;
            if (GetUserTokenModel(storeId, mobile, out token))
            {
                XCManaUserHelperTokenCache.Remove(token);
            }

            SetDBManaUserToken(newToken, mobile, storeId, storeName, userId);
            XCManaUserHelperTokenModel tokenModel = new XCManaUserHelperTokenModel(newToken, storeId, storeName, mobile, userId);
            XCManaUserHelperTokenCache.AddToken(newToken, tokenModel);

            return newToken;
        }


        public static XCManaUserHelperTokenModel GetManaUserTokenModel(string token)
        {
            if (XCManaUserHelperTokenCache.ExistToken(token))
            {
                XCManaUserHelperTokenModel tokenModel = XCManaUserHelperTokenCache.GetModel(token);
                return tokenModel;
            }
            else
            {
                return null;
            }
        }


        public static bool GetUserTokenModel(string mobile, ref List<XCManaUserHelperTokenResultModel> storeList)
        {
            storeList = new List<XCManaUserHelperTokenResultModel>();
            var query = XCManaUserHelperTokenCache.UserTokenList.Where(t => t.Mobile.Equals(mobile)).ToList();

            if (query.Count == 0)
            {
                return false;
            }
            else
            {
                string storeName = string.Empty;
                foreach (var item in query)
                {
                    XCCloudWebBar.Model.CustomModel.XCGameManager.StoreCacheModel storeModel = StoreCache.GetStoreModel(item.StoreId);
                    if (storeModel == null)
                    {
                        storeName = string.Empty;
                    }
                    else
                    {
                        storeName = storeModel.StoreName;
                    }

                    XCManaUserHelperTokenResultModel userTokenResultModel = new XCManaUserHelperTokenResultModel(item.StoreId, storeName, item.Token);
                    storeList.Add(userTokenResultModel);
                }
                return true;
            }
        }

        public static bool GetUserTokenModel(string storeId, string mobile, out string token)
        {
            token = string.Empty;

            var model = XCManaUserHelperTokenCache.UserTokenList.FirstOrDefault(t => t.StoreId.Equals(storeId) && t.Mobile.Equals(mobile));
            if (model == null)
            {
                return false;
            }
            else
            {
                token = model.Token;
                return true;
            }
        }


        public static void Init()
        {
            if (!RedisCacheHelper.KeyExists(XCManaUserHelperTokenCache.XCManaUserTokenCacheKey))
            {
                IUserTokenService userTokenService = BLLContainer.Resolve<IUserTokenService>();
                var list = userTokenService.GetModels(p => 1 == 1).ToList<t_usertoken>();
                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        XCManaUserHelperTokenModel tokenModel = new XCManaUserHelperTokenModel(list[i].Token, list[i].StoreId, list[i].StoreName, list[i].Mobile, Convert.ToInt32(list[i].UserId));
                        XCManaUserHelperTokenCache.AddToken(list[i].Token, tokenModel);
                    }
                }
            }
        }


        public static void Clear()
        {
            XCManaUserHelperTokenCache.Clear();
        }

        private static void SetDBManaUserToken(string token,string mobile , string storeId, string storeName,int userId)
        {
            IUserTokenService userTokenService = BLLContainer.Resolve<IUserTokenService>();
            var model = userTokenService.GetModels(p => p.StoreId.Equals(storeId) & p.Mobile.Equals(mobile)).FirstOrDefault<t_usertoken>();
            
            if (model == null)
            {
                t_usertoken userToken = new t_usertoken();
                userToken.Token = token;
                userToken.Mobile = mobile;
                userToken.StoreId = storeId;
                userToken.StoreName = storeName;
                userToken.CreateTime = DateTime.Now;
                userToken.UserId = userId;
                userTokenService.Add(userToken);
            }
            else
            {
                model.Token = token;
                model.Mobile = mobile;
                model.StoreId = storeId;
                model.StoreName = storeName;
                model.UpdateTime = DateTime.Now;
                model.UserId = userId;
                userTokenService.Update(model);
            }
        }

    }
}
