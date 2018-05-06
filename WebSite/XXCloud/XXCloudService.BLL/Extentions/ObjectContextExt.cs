using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL;
using XCCloudService.DAL.Extensions;

namespace XCCloudService.BLL.Extentions
{
    public class XCCloudBLLExt
    {
        public static void ExecuteStoredProcedure(string storedProcedureName, SqlParameter[] paramArr)
        {
            var dbContext = DbContextFactory.CreateByContainerName("XCCloudDBEntities");
            dbContext.ExecuteStoredProcedure(storedProcedureName, paramArr);
        }
    }
}
