using System;
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
        //private static object syncRoot = new Object();

        public static string SetUserToken(string logId, int logType, TokenDataModel dataModel = null)
        {
            //设置用户token      
            string newToken = System.Guid.NewGuid().ToString("N");
            //string token = string.Empty;
            //if (GetUserTokenModel(logId, logType, out token))
            //{
            //    XCCloudUserTokenCache.Remove(token);
            //}

            RemoveUserToken(logId);

            XCCloudUserTokenModel tokenModel = new XCCloudUserTokenModel(newToken, logId, Utils.ConvertDateTimeToLong(DateTime.Now), logType, dataModel);
            XCCloudUserTokenCache.AddToken(newToken, tokenModel);

            return newToken;
        }

        public static string SetUserToken(XCCloudUserTokenModel tokenModel)
        {
            //设置用户token      
            string newToken = System.Guid.NewGuid().ToString("N");

            RemoveUserToken(tokenModel.LogId);

            tokenModel.Token = newToken;
            tokenModel.EndTime = Utils.ConvertDateTimeToLong(DateTime.Now);
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
            userTokenModel = XCCloudUserTokenCache.GetModel(token);
            return userTokenModel;
        }

        public static void RemoveUserToken(string logId)
        {
            var query = XCCloudUserTokenCache.UserTokenList.Where(t => t.LogId.Equals(logId)).Select(o => o.Token).ToArray();
            foreach (var item in query)
            {
                XCCloudUserTokenCache.Remove(item);
            }
        }

        public static bool GetUserTokenModel(string logId, out string token)
        {
            token = string.Empty;
            var query = XCCloudUserTokenCache.UserTokenList.FirstOrDefault(t => t.LogId.Equals(logId));
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

        public static bool GetUserTokenModel(string logId, int logType, out string token)
        {
            token = string.Empty;
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

        /// <summary>
        /// 移除在同一工作站登录的门店用户
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="logType"></param>
        public static void RemoveStoreUserTokenByWorkStaion(string logId, string workStation)
        {
            var query = XCCloudUserTokenCache.UserTokenList
                .Where(t => t.LogId.Equals(logId) && t.DataModel != null && t.DataModel.WorkStation != null && t.DataModel.WorkStation.Equals(workStation))
                .Select(t => t.Token).ToArray();
            foreach (var item in query)
            {
                XCCloudUserTokenCache.Remove(item);
            }
        }

        /// <summary>
        /// 移除门店所有工作站用户
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="logType"></param>
        public static void RemoveWorkStationUserToken(string storeId)
        {
            var query = XCCloudUserTokenCache.UserTokenList
                .Where(t => t.DataModel != null && t.DataModel.WorkStation != null && t.DataModel.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Token).ToArray();
            foreach (var item in query)
            {
                XCCloudUserTokenCache.Remove(item);
            }
        }

    }
}
