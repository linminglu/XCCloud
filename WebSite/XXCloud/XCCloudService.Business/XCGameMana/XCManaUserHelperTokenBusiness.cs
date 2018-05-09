using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCGameManager;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCGameManager;

namespace XCCloudService.Business.XCGameMana
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
                SetDBManaUserToken(newToken, mobile, storeId, storeName, userId);
                XCManaUserHelperTokenCache.Remove(token);
                XCManaUserHelperTokenModel tokenModel = new XCManaUserHelperTokenModel(storeId, storeName, mobile, userId);
                XCManaUserHelperTokenCache.AddToken(newToken, tokenModel);
            }
            else
            {
                SetDBManaUserToken(newToken, mobile, storeId, storeName, userId);
                XCManaUserHelperTokenModel tokenModel = new XCManaUserHelperTokenModel(storeId, storeName, mobile, userId);
                XCManaUserHelperTokenCache.AddToken(newToken, tokenModel);
            }

            return newToken;
        }


        public static XCManaUserHelperTokenModel GetManaUserTokenModel(string token)
        {
            if (XCManaUserHelperTokenCache.UserTokenHTDic.ContainsKey(token))
            {
                XCManaUserHelperTokenModel tokenModel = (XCManaUserHelperTokenModel)(XCManaUserHelperTokenCache.UserTokenHTDic[token]);
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
            var query = from item in XCManaUserHelperTokenCache.UserTokenHTDic
                        where ((XCManaUserHelperTokenModel)(item.Value)).Mobile.Equals(mobile)
                        select item.Key.ToString();
            if (query.Count() == 0)
            {
                return false;
            }
            else
            {
                List<string> tmpList = query.ToList<string>();
                string storeName = string.Empty;
                for (int i = 0; i < tmpList.Count; i++)
                {
                    XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(XCManaUserHelperTokenCache.UserTokenHTDic[tmpList[i]]);
                    XCCloudService.Model.CustomModel.XCGameManager.StoreCacheModel storeModel = null;
                    string errMsg = string.Empty;
                    StoreBusiness storeBusiness = new StoreBusiness();
                    if (storeBusiness.IsEffectiveStore(userTokenModel.StoreId, ref storeModel, out errMsg))
                    {
                        if (storeModel == null)
                        {
                            storeName = string.Empty;
                        }
                        else
                        {
                            storeName = storeModel.StoreName;
                        }
                    }

                    XCManaUserHelperTokenResultModel userTokenResultModel = new XCManaUserHelperTokenResultModel(userTokenModel.StoreId, storeName, tmpList[i].ToString());
                    storeList.Add(userTokenResultModel);
                }
                return true;
            }
        }

        public static bool GetUserTokenModel(string storeId, string mobile, out string token)
        {
            token = string.Empty;
            var query = from item in XCManaUserHelperTokenCache.UserTokenHTDic
                        where ((XCManaUserHelperTokenModel)(item.Value)).StoreId.Equals(storeId) && ((XCManaUserHelperTokenModel)(item.Value)).Mobile.Equals(mobile)
                        select item.Key.ToString();
            if (query.Count() == 0)
            {
                return false;
            }
            else
            {
                token = query.First();
                return true;
            }
        }


        public static void Init()
        {
            IUserTokenService userTokenService = BLLContainer.Resolve<IUserTokenService>();
            var list = userTokenService.GetModels(p => 1 == 1).ToList<t_usertoken>();
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    XCManaUserHelperTokenModel tokenModel = new XCManaUserHelperTokenModel(list[i].StoreId, list[i].StoreName, list[i].Mobile, Convert.ToInt32(list[i].UserId));
                    XCManaUserHelperTokenCache.AddToken(list[i].Token, tokenModel);
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
