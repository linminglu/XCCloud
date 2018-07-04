﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.CacheService.XCGameMana;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using XCCloudWebBar.Model.XCGameManager;

namespace XCCloudWebBar.Business.XCGameMana
{
    public class StoreBusiness
    {
        public bool IsEffectiveStore(string storeId,ref StoreCacheModel storeModel, out string errMsg)
        {
            errMsg = string.Empty;
            var model = StoreCache.GetStoreModel(storeId);
            if (model == null)
            {
                errMsg = "门店信息不存在";
                return false;
            }
            else
            {
                storeModel = model;
                return true;
            }
        }

        public bool GetStoreName(string storeId, out string storeName, out string errMsg)
        {
            errMsg = string.Empty;
            storeName = string.Empty;
            var model = StoreCache.GetStoreModel(storeId);
            if (model == null)
            {
                errMsg = "门店信息不存在";
                return false;
            }
            else
            {
                storeName = model.StoreName;
                return true;
            }
        }

        public bool IsEffectiveStore(string storeId,out XCGameManaDeviceStoreType deviceStoreType,ref StoreCacheModel storeModel, out string errMsg)
        {
            errMsg = string.Empty;
            deviceStoreType = XCGameManaDeviceStoreType.Store;
            var model = StoreCache.GetStoreModel(storeId);
            if (model == null)
            {
                errMsg = "门店信息不存在";
                return false;
            }
            else
            {
                storeModel = model;
                deviceStoreType = (XCGameManaDeviceStoreType)(storeModel.StoreType);
                return true;
            }
        }

        public bool IsEffectiveStore(string storeId, out string errMsg)
        {
            errMsg = string.Empty;
            var model = StoreCache.GetStoreModel(storeId);
            if (model == null)
            {
                errMsg = "门店信息不存在";
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 验证是否有效门店
        /// </summary>
        /// <param name="storeId">门店ID</param>
        /// <param name="xcGameDBName">门店对应的数据库名</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns></returns>
        public bool IsEffectiveStore(string storeId,out string xcGameDBName,out string errMsg)
        {
            errMsg = string.Empty;
            xcGameDBName = string.Empty;
            var model = StoreCache.GetStoreModel(storeId);
            if (model == null)
            {
                errMsg = "门店信息不存在";
                return false;
            }
            else
            {
                xcGameDBName = model.StoreDBName;
                return true;
            }
        }


        public bool IsEffectiveStore(string storeId, out string xcGameDBName,out string password, out string errMsg)
        {
            errMsg = string.Empty;
            password = string.Empty;
            xcGameDBName = string.Empty;
            var model = StoreCache.GetStoreModel(storeId);
            if (model == null)
            {
                errMsg = "门店信息不存在";
                return false;
            }
            else
            {
                password = model.StorePassword;
                xcGameDBName = model.StoreDBName;
                return true;
            }
        }

        public bool IsExistDog(string storeId,string dogId)
        {
            return StoreDogCache.ExistDogId(storeId,dogId);
        }

        public static void StoreInit()
        {
            if (!RedisCacheHelper.KeyExists(StoreCache.storeCacheKey))
            {
                string errMsg = string.Empty;
                //IStoreService storeService = BLLContainer.Resolve<IStoreService>();
                //string sql = " select ID as StoreID,store_password as StorePassword,store_dbname as StoreDBName,companyname as StoreName,StoreType,StoreDBDeployType from t_store where state = 1";
                //System.Data.DataSet ds = XCGameManabll.ExecuteQuerySentence(sql, null);
                string sql = "select StoreID, Password as StorePassword, '' as StoreDBName, StoreName, 2 AS StoreType, 1 AS StoreDBDeployType from Base_StoreInfo where StoreState = 1";
                System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, null);
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    StoreCache.Clear();
                    var list = Utils.GetModelList<StoreCacheModel>(ds.Tables[0]).ToList();
                    StoreCache.Init(list);
                }
            }
        }

        public static void StoreDogInit()
        {
            string errMsg = string.Empty;
            IStoreService storeService = BLLContainer.Resolve<IStoreService>();
            string sql = " exec Selectstoredog";
            System.Data.DataSet ds = XCGameManabll.ExecuteQuerySentence(sql, null);
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count > 0)
            {
                StoreDogCache.Clear();
                var list = Utils.GetModelList<StoreDogCacheModel>(ds.Tables[0]).ToList();
                StoreDogCache.Add(list);
            }
        }
    }
}