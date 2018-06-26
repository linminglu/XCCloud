using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.WeiXin.Message
{
    public class DoScheduleDataModel
    {
        public DoScheduleDataModel()
        { 
            
        }

        public DoScheduleDataModel(string scheduleId, int userId)
        {
            this.ScheduleID = scheduleId;
            this.UserID = userId;
        }

        public string ScheduleID { set; get; }

        public int UserID { set; get; }

    }
}
