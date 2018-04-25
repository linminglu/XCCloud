using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_DepotInfoBusiness
	{        
        public static IBase_DepotInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_DepotInfoService>();
            }
            
                       
        }
        
        public static IBase_DepotInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_DepotInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}