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
    public partial class UDPRadarRegisterLogService : BaseService<t_UDPRadarRegisterLog>, IUDPRadarRegisterLogService
    {
        private IUDPRadarRegisterLogDAL deviceDAL = DALContainer.Resolve<IUDPRadarRegisterLogDAL>();
        public override void SetDal()
        {
            Dal = deviceDAL;
        }
    }
}
