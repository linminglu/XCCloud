using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Log_GameAlarmBusiness
	{        
        public static ILog_GameAlarmService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<ILog_GameAlarmService>();
            }
            
                       
        }
        
        public static ILog_GameAlarmService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<ILog_GameAlarmService>(resolveNew: true);
            }
            
                       
        }
	} 
}