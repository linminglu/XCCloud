using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Log_OperationBusiness
	{        
        public static ILog_OperationService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<ILog_OperationService>();
            }
            
                       
        }
        
        public static ILog_OperationService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<ILog_OperationService>(resolveNew: true);
            }
            
                       
        }
	} 
}