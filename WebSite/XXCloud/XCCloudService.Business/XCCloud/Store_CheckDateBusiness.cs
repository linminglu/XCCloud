using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Store_CheckDateBusiness
	{        
        public static IStore_CheckDateService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IStore_CheckDateService>();
            }
            
                       
        }
        
        public static IStore_CheckDateService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IStore_CheckDateService>(resolveNew: true);
            }
            
                       
        }
	} 
}