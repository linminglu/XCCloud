using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Coins 的摘要说明
    /// </summary>
    public class Coins : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddCoinStorage(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                int userId = Convert.ToInt32(userTokenKeyModel.LogId);
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("storageCount").Validint("入库数量", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var storageCount = dicParas.Get("storageCount").Toint();
                var note = dicParas.Get("note");         

                var data_CoinStorage = new Data_CoinStorage();
                data_CoinStorage.DestroyTime = DateTime.Now;
                data_CoinStorage.Note = note;
                data_CoinStorage.StorageCount = storageCount;
                data_CoinStorage.UserID = userId;
                data_CoinStorage.StoreID = storeId;
                if (!Data_CoinStorageService.I.Add(data_CoinStorage))
                {
                    errMsg = "更新数据库失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                                

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCoinStorage(Dictionary<string, object> dicParas)
        {
            try
            {                
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                if(!dicParas.Get("destroyTime").Validdate("入库时间", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var destroyTime = dicParas.Get("destroyTime").Todatetime();

                var query = Data_CoinStorageService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                if (destroyTime != null)
                {
                    query = query.Where(w => DbFunctions.DiffDays(w.DestroyTime, destroyTime) == 0);
                }

                var result = from a in query.AsEnumerable()
                             join b in Base_UserInfoService.N.GetModels(p => p.UserType == (int)UserType.Store) on a.UserID equals b.UserID into b1
                             from b in b1.DefaultIfEmpty()
                             select new
                             {
                                 ID = a.ID,
                                 StoreID = a.StoreID,
                                 StorageCount = a.StorageCount,
                                 DestroyTime = Utils.ConvertFromDatetime(a.DestroyTime),
                                 UserID = a.UserID,
                                 Note = a.Note,
                                 LogName = b != null ? b.LogName : string.Empty,
                                 RealName = b != null ? b.RealName : string.Empty
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddCoinDestory(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                int userId = Convert.ToInt32(userTokenKeyModel.LogId);
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("storageCount").Validint("销毁数量", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var storageCount = dicParas.Get("storageCount").Toint();
                var note = dicParas.Get("note");                               

                var data_CoinDestory = new Data_CoinDestory();
                data_CoinDestory.DestroyTime = DateTime.Now;
                data_CoinDestory.Note = note;
                data_CoinDestory.StorageCount = storageCount;
                data_CoinDestory.UserID = userId;
                data_CoinDestory.StoreID = storeId;
                if (!Data_CoinDestoryService.I.Add(data_CoinDestory))
                {
                    errMsg = "更新数据库失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCoinDestory(Dictionary<string, object> dicParas)
        {
            try
            {                
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("destroyTime").Validdate("销毁时间", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var destroyTime = dicParas.Get("destroyTime").Todatetime();

                var query = Data_CoinDestoryService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                if (destroyTime != null)
                {
                    query = query.Where(w => DbFunctions.DiffDays(w.DestroyTime, destroyTime) == 0);
                }

                var result = from a in query.AsEnumerable()
                             join b in Base_UserInfoService.N.GetModels(p => p.UserType == (int)UserType.Store) on a.UserID equals b.UserID into b1
                             from b in b1.DefaultIfEmpty()
                             select new
                             {
                                 ID = a.ID,
                                 StoreID = a.StoreID,
                                 StorageCount = a.StorageCount,
                                 DestroyTime = Utils.ConvertFromDatetime(a.DestroyTime),
                                 UserID = a.UserID,
                                 Note = a.Note,
                                 LogName = b != null ? b.LogName : string.Empty,
                                 RealName = b != null ? b.RealName : string.Empty
                             };
                    
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddDigitCoin(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                if(!dicParas.Get("digitLevelID").Validint("数字币级别编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("iCardID").Nonempty("数字币编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("iCardID").Length > 16)
                {
                    errMsg = "数字币编号长度不能超过16位";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var digitLevelID = dicParas.Get("digitLevelID").Toint();
                var iCardID = dicParas.Get("iCardID");                
                
                if (Data_DigitCoinService.I.Any(a => a.ICardID.Equals(iCardID, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该数字币已存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DigitCoin = new Data_DigitCoin();
                data_DigitCoin.CreateTime = DateTime.Now;
                data_DigitCoin.ICardID = iCardID;
                data_DigitCoin.DigitLevelID = digitLevelID;
                data_DigitCoin.StoreID = storeId;
                data_DigitCoin.Status = (int)DigitStatus.Inuse;
                if (!Data_DigitCoinService.I.Add(data_DigitCoin))
                {
                    errMsg = "更新数据库失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDigitCoin(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                var data_DigitCoin = Data_DigitCoinService.I.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.Status != (int)DigitStatus.Cancel)
                    .OrderBy(or => or.ICardID).Select(o => o.ICardID).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_DigitCoin);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddDigitDestroy(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                int userId = Convert.ToInt32(userTokenKeyModel.LogId);

                string errMsg = string.Empty;
                string iCardID = dicParas.Get("iCardID");

                if (!iCardID.Nonempty("数字币编号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                
                if (iCardID.Length > 16)
                {
                    errMsg = "数字币编号长度不能超过16位";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_DigitCoinService.I.Any(a => a.ICardID.Equals(iCardID, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "该数字币不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_DigitCoin = Data_DigitCoinService.I.GetModels(p => p.ICardID.Equals(iCardID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        data_DigitCoin.Status = (int)DigitStatus.Cancel;
                        if (!Data_DigitCoinService.I.Update(data_DigitCoin))
                        {
                            errMsg = "更新数字币档案失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_DigitCoinDestroy = new Data_DigitCoinDestroy();
                        data_DigitCoinDestroy.DestroyTime = DateTime.Now;
                        data_DigitCoinDestroy.ICCardID = iCardID;
                        data_DigitCoinDestroy.StoreID = storeId;
                        data_DigitCoinDestroy.UserID = userId;
                        if (!Data_DigitCoinDestroyService.I.Add(data_DigitCoinDestroy))
                        {
                            errMsg = "更新数据库失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
                    }
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDigitDestroy(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("destroyTime").Validdate("销毁时间", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var destroyTime = dicParas.Get("destroyTime").Todatetime();
                var iCardID = dicParas.Get("iCardID");
                
                var query = Data_DigitCoinDestroyService.N.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                if (destroyTime != null)
                {
                    query = query.Where(w => DbFunctions.DiffDays(w.DestroyTime, destroyTime) == 0);
                }

                if (!string.IsNullOrEmpty(iCardID))
                {
                    query = query.Where(w => w.ICCardID.Contains(iCardID));
                }

                var result = from a in query.AsEnumerable()
                             join b in Base_UserInfoService.N.GetModels(p => p.UserType == (int)UserType.Store) on a.UserID equals b.UserID into b1
                             from b in b1.DefaultIfEmpty()
                             select new
                             {
                                 ID = a.ID,
                                 StoreID = a.StoreID,
                                 ICardID = a.ICCardID,
                                 DestroyTime = Utils.ConvertFromDatetime(a.DestroyTime),
                                 UserID = a.UserID,
                                 Note = a.Note,
                                 LogName = b != null ? b.LogName : string.Empty,
                                 RealName = b != null ? b.RealName : string.Empty
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}