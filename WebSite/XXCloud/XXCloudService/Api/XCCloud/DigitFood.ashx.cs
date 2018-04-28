using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.Business.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.XCCloud;
using System.Transactions;
using XCCloudService.Common.Enum;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// DigitFood 的摘要说明
    /// </summary>
    public class DigitFood : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryDigitFood(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var linq = from a in Data_DigitCoinFoodBiz.NI.GetModels(p=>p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                           join b in Dict_BalanceTypeBiz.NI.GetModels(p => p.State == 1) on a.BalanceIndex equals b.ID into b1
                           from b in b1.DefaultIfEmpty()
                           select new
                           {
                               ID = a.ID,
                               FoodName = a.FoodName,
                               BalanceIndex = a.BalanceIndex,
                               BalanceIndexStr = b != null ? b.TypeName : string.Empty,
                               Coins = a.Coins,
                               AuthorFlag = a.AuthorFlag == 0 ? "否" : a.AuthorFlag == 1 ? "是" : string.Empty,                               
                               Note = a.Note
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDigitFoodInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                if (id == 0)
                {
                    errMsg = "数字币套餐ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DigitFoodInfo = Data_DigitCoinFoodBiz.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (data_DigitFoodInfo == null)
                {
                    errMsg = "该数字币套餐不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_DigitFoodInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDigitFoodDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
               
                var linq = from a in Data_DigitCoinFoodBiz.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))                
                           select new {
                               ID = a.ID,
                               FoodName = a.FoodName
                           };
                
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveDigitFoodInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                string foodName = dicParas.Get("foodName");
                var balanceIndex = dicParas.Get("balanceIndex").Toint();
                var coins = dicParas.Get("coins").Toint();
                var authorFlag = dicParas.Get("authorFlag").Toint();
                string note = dicParas.Get("note");                
                
                #region 验证参数

                if(!foodName.Nonempty("数字币套餐名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                if (!coins.Illegalint("数量", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                if (!authorFlag.Illegalint("授权标志", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                if (!balanceIndex.Illegalint("活动定金", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                

                #endregion

                if(Data_DigitCoinFoodBiz.I.Any(a=>a.ID != id && a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && a.FoodName.Equals(foodName, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "数字币套餐名称不能重复";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DigitCoinFood = Data_DigitCoinFoodBiz.I.GetModels(p => p.ID == id).FirstOrDefault() ?? new Data_DigitCoinFood();
                data_DigitCoinFood.ID = id;
                data_DigitCoinFood.FoodName = foodName;
                data_DigitCoinFood.BalanceIndex = balanceIndex;
                data_DigitCoinFood.Coins = coins;
                data_DigitCoinFood.AuthorFlag = authorFlag;
                data_DigitCoinFood.Note = note;
                data_DigitCoinFood.MerchID = merchId;
                if (id == 0)
                {
                    if (!Data_DigitCoinFoodBiz.I.Add(data_DigitCoinFood))
                    {
                        errMsg = "添加数字币套餐失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    if (!Data_DigitCoinFoodBiz.I.Any(a => a.ID == id))
                    {
                        errMsg = "该数字币套餐不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Data_DigitCoinFoodBiz.I.Update(data_DigitCoinFood))
                    {
                        errMsg = "更新数字币套餐失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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
        public object DelDigitFoodInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                if (id == 0)
                {
                    errMsg = "数字币套餐ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_DigitCoinFoodBiz.I.Any(a => a.ID == id))
                        {
                            errMsg = "该数字币套餐不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_DigitCoinFood = Data_DigitCoinFoodBiz.I.GetModels(p => p.ID == id).FirstOrDefault();
                        if (!Data_DigitCoinFoodBiz.I.Delete(data_DigitCoinFood))
                        {
                            errMsg = "删除数字币套餐失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        foreach (var model in Data_Food_DetialBiz.I.GetModels(p => p.FoodType == (int)FoodDetailType.Digit && p.Status == 1))
                        {
                            model.Status = 0;
                            Data_Food_DetialBiz.I.UpdateModel(model);
                        }

                        if (!Data_Food_DetialBiz.I.SaveChanges())
                        {
                            errMsg = "删除套餐内容关联信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}