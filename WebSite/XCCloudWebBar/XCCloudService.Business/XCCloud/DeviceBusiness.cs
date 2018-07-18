using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Model.XCCloud;

namespace XCCloudWebBar.Business.XCCloud
{
    public class DeviceBusiness
    {
        public bool GetDeviceInfo(string merchId, string storeId, int type, string workStation, out string mcuId, out int status, out string statusName)
        {
            string storedProcedure = "GetDeviceInfo";
            SqlParameter[] sqlParameter = new SqlParameter[9];
            sqlParameter[0] = new SqlParameter("@MerchId", SqlDbType.VarChar);
            sqlParameter[0].Value = merchId;

            sqlParameter[1] = new SqlParameter("@StoreId", SqlDbType.VarChar);
            sqlParameter[1].Value = storeId;

            sqlParameter[2] = new SqlParameter("@Type", SqlDbType.Int);
            sqlParameter[2].Value = type;

            sqlParameter[3] = new SqlParameter("@WorkStation", SqlDbType.VarChar);
            sqlParameter[3].Value = workStation;

            sqlParameter[4] = new SqlParameter("@Result", SqlDbType.Int);
            sqlParameter[4].Direction = ParameterDirection.Output;

            sqlParameter[5] = new SqlParameter("@MCUId", SqlDbType.VarChar, 20);
            sqlParameter[5].Direction = ParameterDirection.Output;

            sqlParameter[6] = new SqlParameter("@Status", SqlDbType.Int);
            sqlParameter[6].Direction = ParameterDirection.Output;

            sqlParameter[7] = new SqlParameter("@StatusName", SqlDbType.VarChar, 50);
            sqlParameter[7].Direction = ParameterDirection.Output;

            sqlParameter[8] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
            sqlParameter[8].Direction = ParameterDirection.Output;

            XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, sqlParameter);

            if (sqlParameter[4].Value.ToString() == "1")
            {
                mcuId = sqlParameter[5].Value.ToString();
                status = int.Parse(sqlParameter[6].Value.ToString());
                statusName = sqlParameter[7].Value.ToString();
                return true;
            }
            else
            {
                mcuId = string.Empty;
                status = 0;
                statusName = string.Empty;
                return false;
            }
        }

        public static IQueryable<Base_DeviceInfo> GetDeviceList(string merchId, string storeId)
        {
            return Base_DeviceInfoService.I.GetModels(t => t.MerchID == merchId && t.StoreID == storeId && t.DeviceStatus != 0);
        }
    }
}
