using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCCloudRS232;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCCloudRS232;
using XCCloudWebBar.Model.XCCloudRS232;

namespace XCCloudWebBar.BLL.XCCloudRS232
{
    public partial class DataGameInfoService : BaseService<Data_GameInfo>, IDataGameInfoService
    {
        private IDataGameInfoDAL deviceDAL = DALContainer.Resolve<IDataGameInfoDAL>();
        public override void SetDal()
        {
            Dal = deviceDAL;
        }
    }
    
}
