﻿using System;
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
    public class XCManaDogTokenBusiness
    {
        public static string SetToken(string dogId, string storeId)
        {
            //设置会员token
            string newToken = System.Guid.NewGuid().ToString("N");
            string token = string.Empty;
            if (GetManaDogTokenModel(dogId, storeId, out token))
            {
                XCManaDogTokenCache.Remove(token);
            }

            XCManaDogTokenModel tokenModel = new XCManaDogTokenModel(dogId, storeId, newToken);
            XCManaDogTokenCache.AddToken(newToken, tokenModel);

            return newToken;
        }


        public static XCManaDogTokenModel GetManaDogTokenModel(string token)
        {
            if (XCManaDogTokenCache.ExistToken(token))
            {
                XCManaDogTokenModel tokenModel = XCManaDogTokenCache.GetModel(token);
                return tokenModel;
            }
            else
            {
                return null;
            }
        }

        public static bool GetManaDogTokenModel(string dogId, string storeId, out string token)
        {
            token = string.Empty;

            var model = XCManaDogTokenCache.DogTokenList.FirstOrDefault(t => t.StoreId.Equals(storeId) && t.DogId.Equals(dogId));
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
            
        }


        public static void Clear()
        {
            XCManaDogTokenCache.Clear();
        }        

    }
}
