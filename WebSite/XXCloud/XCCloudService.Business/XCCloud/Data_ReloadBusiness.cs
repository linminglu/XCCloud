using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_ReloadBusiness
	{        
        public static IData_ReloadService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ReloadService>();
            }
            
                       
        }
        
        public static IData_ReloadService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ReloadService>(resolveNew: true);
            }
            
                       
        }
	} 
}