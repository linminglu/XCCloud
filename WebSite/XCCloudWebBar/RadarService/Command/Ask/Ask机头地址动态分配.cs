using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DSS;

namespace RadarService.Command.Ask
{
    public class Ask机头地址动态分配
    {
        public byte 机头地址 { get; set; }
        public UInt64 MCUID { get; set; }
        /// <summary>
        /// 是否为有效地址
        /// </summary>
        public bool isSuccess = false;
        public Ask机头地址动态分配(string rAddres, string longAddress, bool IsNew)
        {
            try
            {
                DataAccess ac = new DataAccess();
                MCUID = Convert.ToUInt64(longAddress, 16);
                DataTable dt = ac.ExecuteQuery(string.Format("select Segment,Address from Base_DeviceInfo where mcuid='{0}'", longAddress)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    if (row["Segment"].ToString() == rAddres)
                        机头地址 = Convert.ToByte(row["Address"].ToString(), 16);
                    else
                        机头地址 = 0xfe;                    
                }
                else
                    机头地址 = 0xfe;
                isSuccess = true;
            }
            catch
            {
                throw;
            }
        }
    }
}
