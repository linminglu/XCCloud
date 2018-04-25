using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GoodStorageBusiness
	{        
        public static IData_GoodStorageService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodStorageService>();
            }
            
                       
        }
        
        public static IData_GoodStorageService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodStorageService>(resolveNew: true);
            }
            
                       
        }
	} 
}