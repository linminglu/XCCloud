using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCGame;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCGame;
using XCCloudWebBar.Model.XCGame;

namespace XCCloudWebBar.BLL.XCGame
{
    public partial class ScheduleService : BaseService<flw_schedule>, IScheduleService
    {
        private IScheduleDAL StaffDAL;

        public ScheduleService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IScheduleDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
    
}
