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
	public class Flw_ScheduleService : BaseService<Flw_Schedule>, IFlw_ScheduleService
	{
        public override void SetDal()
        {
        	
        }
        
        public Flw_ScheduleService()
        	: this(false)
        {
            
        }
        
        public Flw_ScheduleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_ScheduleDAL>(resolveNew: resolveNew);
        }
	} 
}