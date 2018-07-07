using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.XCCloud;
using XCCloudService.Common.Enum;

namespace XCCloudService.BLL.XCCloud
{
    public static class XCCloudBLLExt
    {
        public static void ExecuteStoredProcedure(string storedProcedureName, SqlParameter[] paramArr)
        {
            var dbContext = DbContextFactory.CreateByModelNamespace("XCCloudService.Model.XCCloud");
            dbContext.ExecuteStoredProcedure(storedProcedureName, paramArr);
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="t"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static bool CheckVerifiction(this object t, bool identity)
        {
            try
            {
                //检查该实体是否需要校验
                if (t.ContainProperty("Verifiction"))
                {
                    var dbContext = DbContextFactory.CreateByModelNamespace("XCCloudService.Model.XCCloud");

                    //获取校验码
                    var verifiction = Convert.ToString(t.GetPropertyValue("Verifiction"));

                    //获取校验密钥
                    var merchSecret = string.Empty;
                    if (t.ContainProperty("MerchID"))
                    {
                        var merchId = Convert.ToString(t.GetPropertyValue("MerchID"));
                        if (!merchId.IsNull())
                        {                            
                            merchSecret = dbContext.Set<Base_MerchantInfo>().Where(w => w.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                .Select(o => o.MerchSecret).FirstOrDefault() ?? string.Empty;
                        }
                    }

                    //获取表主键组
                    var pkList = dbContext.GetPrimaryKey(t.GetType());
                    
                    var str = string.Empty;
                    var md5 = string.Empty;
                    str = t.GetClearText(identity, pkList, merchSecret);
                    md5 = Utils.MD5(str);
                    if (!verifiction.Equals(md5, StringComparison.OrdinalIgnoreCase))
                    {
                        LogHelper.SaveLog(str);
                        LogHelper.SaveLog(md5);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        /// <summary>
        /// 更新库存
        /// </summary>
        public static bool UpdateGoodsStock(int? depotId, int? goodId, int? sourceType, int? sourceId, decimal? goodCost, int? stockFlag, int? stockCount, string merchId, string storeId, out string errMsg)
        {
            errMsg = string.Empty;

            //添加库存异动信息
            var data_GoodStock_Record = new Data_GoodStock_Record();
            data_GoodStock_Record.DepotID = depotId;
            data_GoodStock_Record.GoodID = goodId;
            data_GoodStock_Record.SourceType = sourceType;
            data_GoodStock_Record.SourceID = sourceId;
            data_GoodStock_Record.GoodCost = goodCost;
            data_GoodStock_Record.StockFlag = stockFlag;
            data_GoodStock_Record.StockCount = stockCount;
            data_GoodStock_Record.CreateTime = DateTime.Now;
            data_GoodStock_Record.MerchID = merchId;
            data_GoodStock_Record.StoreID = storeId;
            Data_GoodStock_RecordService.I.AddModel(data_GoodStock_Record);

            //更新当前库存
            var stockModel = Data_GoodsStockService.I.GetModels(p => p.StockType == (int)StockType.Depot && p.StockIndex == depotId && p.GoodID == goodId).OrderByDescending(or => or.InitialTime).FirstOrDefault();
            if (stockModel == null)
            {
                errMsg = "仓库未绑定该商品信息";
                return false;
            }

            stockModel.RemainCount = (stockModel.RemainCount ?? 0) + (stockFlag == (int)StockFlag.Out ? -stockCount : stockFlag == (int)StockFlag.In ? stockCount : 0) ?? 0;
            if (!Data_GoodsStockService.I.Update(stockModel))
            {
                errMsg = "更新当前库存信息失败";
                return false;
            }

            return true;
        }
    }
}
