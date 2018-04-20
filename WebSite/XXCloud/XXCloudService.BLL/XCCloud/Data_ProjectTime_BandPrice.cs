using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCCloud;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Model.XCCloud;
namespace XCCloudService.BLL.XCCloud
{
	public class Data_ProjectTime_BandPriceService : BaseService<Data_ProjectTime_BandPrice>, IData_ProjectTime_BandPriceService
	{
        public override void SetDal()
        {

        }

        public Data_ProjectTime_BandPriceService()
            : this(false)
        {
        }

        public Data_ProjectTime_BandPriceService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ProjectTime_BandPriceDAL>(resolveNew: resolveNew);
        }
	} 
}