using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Common.Extensions;
using System.Transactions;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// FoodType 的摘要说明
    /// </summary>
    public class FoodType : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryFoodType(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
              
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@MerchId", merchId);

                string sql = @"select a.ID, a.DictKey, a.DictValue, a.Icon, a.Comment, a.OrderID, a.Enabled, (case a.Enabled when 1 then '允许' when 0 then '禁止' else '' end) as EnabledStr " +  
                    " from Dict_System a INNER JOIN Dict_System b on a.PID=b.ID" +
                    " where a.MerchID=@MerchId AND b.PID=0 AND b.DictKey='套餐类别' order by a.OrderID";

                var data_FoodType = Dict_SystemService.I.SqlQuery<Data_FoodTypeListModel>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_FoodType);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetFoodTypeDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var foodType = from a in Dict_SystemService.N.GetModels(p => p.Enabled == 1 && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                               join b in Dict_SystemService.N.GetModels(p => p.PID == 0 && p.DictKey.Equals("套餐类别", StringComparison.OrdinalIgnoreCase)) on a.PID equals b.ID
                               orderby a.OrderID
                               select new
                               {
                                   ID = a.ID,
                                   DictKey = a.DictKey
                               };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, foodType);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetFoodType(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if(!dicParas.Get("id").Validintnozero("套餐类别ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();

                var dict_SystemService = Dict_SystemService.I;
                if (!dict_SystemService.Any(p => p.ID == id))
                {
                    errMsg = "该套餐类别不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var dict_SystemModel = dict_SystemService.GetModels(p => p.ID == id).FirstOrDefault();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, dict_SystemModel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveFoodType(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                if (!dicParas.Get("dictKey").Nonempty("类别名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("enabled").Validint("是否启用状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint(0);
                var dictKey = dicParas.Get("dictKey");
                //var dictValue = dicParas.Get("dictValue");
                var icon = dicParas.Get("icon");
                var comment = dicParas.Get("comment");
                var enabled = dicParas.Get("enabled").Toint();
               
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var dict_SystemService = Dict_SystemService.I;

                        var pId = dict_SystemService.GetModels(p => p.DictKey.Equals("套餐类别") && p.PID == 0).Select(o => o.ID).FirstOrDefault();
                        if (pId == 0)
                        {
                            errMsg = "套餐类别根节点不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (dict_SystemService.Any(a => a.ID != id && a.PID == pId && a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && a.DictKey.Equals(dictKey, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "该套餐类别名称已存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var dict_SystemModel = dict_SystemService.GetModels(p => p.ID == id).FirstOrDefault() ?? new Dict_System();
                        if (id == 0)
                        {                            
                            //后续序号加1
                            var list = dict_SystemService.GetModels(p => p.PID == pId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).OrderBy(or => or.OrderID).ToList();
                            for (var i = 1; i <= list.Count; i++)
                            {
                                list[i - 1].OrderID = i + 1;
                                dict_SystemService.UpdateModel(list[i - 1], true, merchId, merchSecret);
                            }

                            if (!dict_SystemService.SaveChanges())
                            {
                                errMsg = "添加套餐类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //新增
                            dict_SystemModel.PID = pId;
                            dict_SystemModel.Enabled = enabled;
                            dict_SystemModel.DictKey = dictKey;                          
                            //dict_SystemModel.DictValue = dictValue;
                            dict_SystemModel.Icon = icon;
                            dict_SystemModel.Comment = comment;
                            dict_SystemModel.MerchID = merchId;
                            dict_SystemModel.OrderID = 1;
                            if (!dict_SystemService.Add(dict_SystemModel, true, merchId, merchSecret))
                            {
                                errMsg = "添加套餐类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //将ID设置为DictValue
                            dict_SystemModel.DictValue = dict_SystemModel.ID.ToString();
                            if (!dict_SystemService.Update(dict_SystemModel, true, merchId, merchSecret))
                            {
                                errMsg = "添加套餐类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (dict_SystemModel.ID == 0)
                            {
                                errMsg = "该套餐类别不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //修改
                            dict_SystemModel.DictKey = dictKey;
                            //dict_SystemModel.DictValue = dictValue;
                            dict_SystemModel.DictValue = dict_SystemModel.ID.ToString();
                            dict_SystemModel.Icon = icon;
                            dict_SystemModel.Enabled = enabled;
                            dict_SystemModel.Comment = comment;
                            if (!dict_SystemService.Update(dict_SystemModel, true, merchId, merchSecret))
                            {
                                errMsg = "修改套餐类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelFoodType(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("套餐类别ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if (!id.Validintnozero("套餐类别ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            var dict_SystemService = Dict_SystemService.I;
                            if (!dict_SystemService.Any(p => p.ID == (int)id))
                            {
                                errMsg = "该套餐类别不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var dict_System = dict_SystemService.GetModels(p => p.ID == (int)id).FirstOrDefault();
                            if (!dict_SystemService.Delete(dict_System, true, merchId, merchSecret))
                            {
                                errMsg = "删除套餐类别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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
        public object OrderFoodType(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Validintnozero("套餐类别ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("orderState").Nonempty("排序状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id").Toint();
                var orderState = dicParas.Get("orderState").Toint(0);                
                if (orderState != 1 && orderState != -1)
                {
                    errMsg = "排序状态值不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var dict_SystemService = Dict_SystemService.I;
                        if (!dict_SystemService.Any(a => a.ID == id))
                        {
                            errMsg = "该套餐类别不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var pId = dict_SystemService.GetModels(p => p.DictKey.Equals("套餐类别") && p.PID == 0).Select(o => o.ID).FirstOrDefault();
                        if (pId == 0)
                        {
                            errMsg = "套餐类别根节点不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //设置当前序号, 默认为1, 且不大于最大序号
                        var dict_System = dict_SystemService.GetModels(p => p.ID == id).FirstOrDefault();
                        var oldOrder = dict_System.OrderID ?? 1;
                        dict_System.OrderID = oldOrder + orderState;
                        if (dict_System.OrderID < 1)
                        {
                            dict_System.OrderID = 1;
                        }

                        var max = dict_SystemService.GetModels(p => p.PID == pId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).Max(m => m.OrderID);
                        if (dict_System.OrderID > max)
                        {
                            dict_System.OrderID = max;
                        }

                        dict_SystemService.UpdateModel(dict_System, true, merchId, merchSecret);

                        var newOrder = dict_System.OrderID ?? 1;
                        if (oldOrder != newOrder)
                        {
                            //与下一个序号交换位置
                            var nextModel = dict_SystemService.GetModels(p => p.PID == pId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != id && p.OrderID == newOrder).FirstOrDefault();
                            if (nextModel != null)
                            {
                                nextModel.OrderID = oldOrder;
                                dict_SystemService.UpdateModel(nextModel, true, merchId, merchSecret);
                            }                            
                        }

                        if (!dict_SystemService.SaveChanges())
                        {
                            errMsg = "排序套餐类别失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        errMsg = e.Message;
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

        /// <summary>
        /// 上传套餐类别背景图片
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UploadFoodTypeImage(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;

                List<string> imageUrls = null;
                if (!Utils.UploadImageFile("/XCCloud/FoodType/Image/", out imageUrls, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, new
                {
                    ImageURL = imageUrls
                });
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}