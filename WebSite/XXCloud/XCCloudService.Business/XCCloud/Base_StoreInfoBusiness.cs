using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_StoreInfoBusiness
	{        
        public static IBase_StoreInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_StoreInfoService>();
            }
            
                       
        }
        
        public static IBase_StoreInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}