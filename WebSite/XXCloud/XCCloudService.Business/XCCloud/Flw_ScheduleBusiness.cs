using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_ScheduleBusiness
	{        
        public static IFlw_ScheduleService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_ScheduleService>();
            }
            
                       
        }
        
        public static IFlw_ScheduleService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_ScheduleService>(resolveNew: true);
            }
            
                       
        }
	} 
}