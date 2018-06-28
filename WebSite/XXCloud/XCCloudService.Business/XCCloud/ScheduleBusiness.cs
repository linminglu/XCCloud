using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCGame;
using XCCloudService.CacheService;

namespace XCCloudService.Business.XCCloud
{
    public class ScheduleBusiness
    {
        //开通班次
        public static bool OpenSchedule(string merchId,string storeId, int userId, string scheduleName, string workStation,out int workStationId, out string currentSchedule,out string openTime, out string errMsg)
        {
            errMsg = string.Empty;
            string checkDate = System.DateTime.Now.ToString("yyyy-MM-dd");
            string flwSeedId = RedisCacheHelper.CreateCloudSerialNo(storeId, true);
            string sql = "OpenSchedule";
            SqlParameter[] parameters = new SqlParameter[15];

            parameters[0] = new SqlParameter("@FlwSeedId", flwSeedId);

            parameters[1] = new SqlParameter("@CheckDate", checkDate);

            parameters[2] = new SqlParameter("@MerchId", merchId);

            parameters[3] = new SqlParameter("@StoreId", storeId);

            parameters[4] = new SqlParameter("@UserId", userId);

            parameters[5] = new SqlParameter("@ScheduleName", scheduleName);

            parameters[6] = new SqlParameter("@workStation", workStation);


            parameters[7] = new SqlParameter("@RealCash", System.Data.SqlDbType.Decimal);
            parameters[7].Value = 0;

            parameters[8] = new SqlParameter("@RealCredit", System.Data.SqlDbType.Decimal);
            parameters[8].Value = 0;

            parameters[9] = new SqlParameter("@AuthorID", System.Data.SqlDbType.Int);
            parameters[9].Value = 0;

            parameters[10] = new SqlParameter("@WorkStationId", 0);
            parameters[10].Direction = System.Data.ParameterDirection.Output;

            parameters[11] = new SqlParameter("@CurrentSchedule", System.Data.SqlDbType.VarChar,32);
            parameters[11].Direction = System.Data.ParameterDirection.Output;

            parameters[12] = new SqlParameter("@OpenTime",System.Data.SqlDbType.VarChar,20);
            parameters[12].Direction = System.Data.ParameterDirection.Output;

            parameters[13] = new SqlParameter("@ErrMsg", System.Data.SqlDbType.VarChar, 50);
            parameters[13].Direction = System.Data.ParameterDirection.Output;

            parameters[14] = new SqlParameter("@ReturnValue", 0);
            parameters[14].Direction = System.Data.ParameterDirection.ReturnValue;

            XCCloudBLL.ExecuteStoredProcedureSentence(sql,parameters);

            int result = Convert.ToInt32(parameters[14].Value);
            if (result >= 1)
            {
                currentSchedule = parameters[11].Value.ToString();
                openTime = parameters[12].Value.ToString();
                workStationId = Convert.ToInt32(parameters[10].Value.ToString());
                return true;
            }
            else
            {
                errMsg = parameters[13].Value.ToString();
                currentSchedule = string.Empty;
                openTime = string.Empty;
                workStationId = 0;
                return false;
            }  
        }

        public static bool GetCurrentSchedule(string merchId, string storeId, int userId, string workStation, out string scheduleName, out string currentSchedule, out string openTime, out string checkDate, out string errMsg)
        {
            errMsg = string.Empty;
            string flwSeedId = RedisCacheHelper.CreateCloudSerialNo(storeId, true);
            string sql = "GetCurrentSchedule";
            string seedId = RedisCacheHelper.CreateCloudSerialNo(storeId, true);

            SqlParameter[] parameters = new SqlParameter[11];
            parameters[0] = new SqlParameter("@MerchId", merchId);
            parameters[1] = new SqlParameter("@StoreId", storeId);
            parameters[2] = new SqlParameter("@SeedId", seedId);
            parameters[3] = new SqlParameter("@UserId", userId);
            parameters[4] = new SqlParameter("@WorkStation", workStation);

            parameters[5] = new SqlParameter("@CurrentSchedule", System.Data.SqlDbType.VarChar,32);
            parameters[5].Direction = System.Data.ParameterDirection.Output;

            parameters[6] = new SqlParameter("@OpenTime", System.Data.SqlDbType.VarChar,20);
            parameters[6].Direction = System.Data.ParameterDirection.Output;

            parameters[7] = new SqlParameter("@ScheduleName", System.Data.SqlDbType.VarChar,50);
            parameters[7].Direction = System.Data.ParameterDirection.Output;

            parameters[8] = new SqlParameter("@CheckDate", System.Data.SqlDbType.VarChar, 10);
            parameters[8].Direction = System.Data.ParameterDirection.Output;

            parameters[9] = new SqlParameter("@ErrMsg", System.Data.SqlDbType.VarChar, 200);
            parameters[9].Direction = System.Data.ParameterDirection.Output;

            parameters[10] = new SqlParameter("@ReturnValue", 0);
            parameters[10].Direction = System.Data.ParameterDirection.ReturnValue;

            XCCloudBLL.ExecuteStoredProcedureSentence(sql, parameters);
            int result = Convert.ToInt32(parameters[10].Value);

            if (result >= 1)
            {
                currentSchedule = parameters[5].Value.ToString();
                openTime = parameters[6].Value.ToString();
                scheduleName = parameters[7].Value.ToString();
                checkDate = parameters[8].Value.ToString();
                return true;
            }
            else
            {
                errMsg = parameters[9].Value.ToString();
                currentSchedule = string.Empty;
                openTime = string.Empty;
                scheduleName = string.Empty;
                checkDate = string.Empty;
                return false;
            }
        }
    
    }
}
