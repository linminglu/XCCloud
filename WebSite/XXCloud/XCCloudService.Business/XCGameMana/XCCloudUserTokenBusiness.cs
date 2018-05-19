﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XCCloudService.Business.XCGameMana
{
    public class XCCloudUserTokenBusiness
    {
        private static object syncRoot = new Object();

        public static string SetUserToken(string logId, int logType, TokenDataModel dataModel = null)
        {
            //设置用户token      
            string newToken = System.Guid.NewGuid().ToString("N");
            //string token = string.Empty;
            //if (GetUserTokenModel(logId, logType, out token))
            //{
            //    XCCloudUserTokenCache.Remove(token);
            //}

            RemoveUserToken(logId, logType);

            XCCloudUserTokenModel tokenModel = new XCCloudUserTokenModel(newToken, logId, Utils.ConvertDateTimeToLong(DateTime.Now), logType, dataModel);
            XCCloudUserTokenCache.AddToken(newToken, tokenModel);

            return newToken;
        }

        public static void RemoveToken(string token)
        {
            XCCloudUserTokenCache.Remove(token);
        }

        public static XCCloudUserTokenModel GetUserTokenModel(string token)
        {
            XCCloudUserTokenModel userTokenModel = null;
            if (XCCloudUserTokenCache.ExistToken(token))
            {
                userTokenModel = XCCloudUserTokenCache.GetModel(token);
            }

            return userTokenModel;
        }

        public static void RemoveUserToken(string logId, int logType)
        {
            lock (syncRoot)
            {
                var query = XCCloudUserTokenCache.UserTokenList.Where(t => t.LogId.Equals(logId) && t.LogType == logType).Select(o=>o.Token).ToArray();
                foreach (var item in query)
                {
                    XCCloudUserTokenCache.Remove(item);
                }
            }
        }

        public static bool GetUserTokenModel(string logId, int logType, out string token)
        {
            token = string.Empty;
            lock (syncRoot)
            {
                var query = XCCloudUserTokenCache.UserTokenList.FirstOrDefault(t => t.LogId.Equals(logId) && t.LogType == logType);
                if (query == null)
                {
                    return false;
                }
                else
                {
                    token = query.Token;
                    return true;
                }
            }
        }

        /// <summary>
        /// 移除在同一工作站登录的门店用户
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="logType"></param>
        public static void RemoveStoreUserTokenByWorkStaion(string logId, int logType, string workStation)
        {
            var query = XCCloudUserTokenCache.UserTokenList.Where(t => t.LogId.Equals(logId) && t.LogType == logType && t.DataModel.WorkStation.Equals(workStation)).Select(t => t.Token).ToArray();
            foreach (var item in query)
            {
                XCCloudUserTokenCache.Remove(item);
            }
        }

    }
}
