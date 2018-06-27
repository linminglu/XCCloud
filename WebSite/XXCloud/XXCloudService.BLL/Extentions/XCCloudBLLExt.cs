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
                    //获取校验码
                    var verifiction = Convert.ToString(t.GetPropertyValue("Verifiction"));

                    //获取校验密钥
                    var merchSecret = string.Empty;
                    if (t.ContainProperty("MerchID"))
                    {
                        var merchId = Convert.ToString(t.GetPropertyValue("MerchID"));
                        if (!merchId.IsNull())
                        {
                            var dbContext = DbContextFactory.CreateByModelNamespace("XCCloudService.Model.XCCloud");
                            merchSecret = dbContext.Set<Base_MerchantInfo>().Where(w => w.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                .Select(o => o.MerchSecret).FirstOrDefault() ?? string.Empty;
                        }
                    }
                    
                    var str = string.Empty;
                    var md5 = string.Empty;
                    str = t.GetClearText(identity, merchSecret);
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
    }
}
