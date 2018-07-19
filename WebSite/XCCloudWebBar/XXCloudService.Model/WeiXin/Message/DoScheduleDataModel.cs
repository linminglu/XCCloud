using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.WeiXin.Message
{
    public class DoScheduleDataModel
    {
        public DoScheduleDataModel()
        {

        }

        public DoScheduleDataModel(string scheduleId, int userId, string workStation, string scheduleName, DateTime openTime, DateTime shiftTime, decimal payCount, decimal realPay, decimal freePay)
        {
            this.ScheduleID = scheduleId;
            this.WorkStation = workStation;
            this.ScheduleName = scheduleName;
            this.UserID = userId;
            this.OpenTime = openTime;
            this.ShiftTime = shiftTime;
            this.PayCount = payCount;
            this.RealPay = realPay;
            this.FreePay = freePay;
        }

        public string ScheduleID { set; get; }

        public int UserID { set; get; }
        public string WorkStation { set; get; }
        public string ScheduleName { set; get; }
        public DateTime OpenTime { set; get; }
        public DateTime ShiftTime { set; get; }
        public decimal PayCount { set; get; }
        public decimal RealPay { set; get; }
        public decimal FreePay { set; get; }

    }
}
