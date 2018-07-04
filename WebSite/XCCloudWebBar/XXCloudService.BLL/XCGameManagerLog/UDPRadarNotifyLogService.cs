using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCGameManagerLog;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCGameManagerLog;
using XCCloudWebBar.Model.XCGameManagerLog;


namespace XCCloudWebBar.BLL.XCGameManagerLog
{
    public partial class UDPRadarNotifyLogService : BaseService<t_UDPRadarNotifyLog>, IUDPRadarNotifyLogService
    {
        private IUDPRadarNotifyLogDAL deviceDAL = DALContainer.Resolve<IUDPRadarNotifyLogDAL>();
        public override void SetDal()
        {
            Dal = deviceDAL;
        }
    }
}
