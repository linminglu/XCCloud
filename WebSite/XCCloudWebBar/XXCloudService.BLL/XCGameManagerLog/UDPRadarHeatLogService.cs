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
    public partial class UDPRadarHeatLogService : BaseService<t_UDPRadarHeatLog>, IUDPRadarHeatLogService
    {
        private IUDPRadarHeatLogDAL deviceDAL = DALContainer.Resolve<IUDPRadarHeatLogDAL>();
        public override void SetDal()
        {
            Dal = deviceDAL;
        }
    }
}
