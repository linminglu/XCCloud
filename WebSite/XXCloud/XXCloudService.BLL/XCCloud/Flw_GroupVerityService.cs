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
    public class Flw_GroupVerityService : BaseService<Flw_GroupVerity>, IFlw_GroupVerityService
	{
		public override void SetDal()
        {

        }

        public Flw_GroupVerityService()
            : this(false)
        {

        }

        public Flw_GroupVerityService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_GroupVerityDAL>(resolveNew: resolveNew);
        }
	} 
}