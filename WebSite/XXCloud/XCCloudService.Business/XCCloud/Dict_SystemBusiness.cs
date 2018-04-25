using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Dict_SystemBusiness
	{        
        public static IDict_SystemService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IDict_SystemService>();
            }
            
                       
        }
        
        public static IDict_SystemService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
            }
            
                       
        }
	} 
}