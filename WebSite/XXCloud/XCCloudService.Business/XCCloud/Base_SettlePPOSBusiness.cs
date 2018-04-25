using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_SettlePPOSBusiness
	{        
        public static IBase_SettlePPOSService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_SettlePPOSService>();
            }
            
                       
        }
        
        public static IBase_SettlePPOSService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_SettlePPOSService>(resolveNew: true);
            }
            
                       
        }
	} 
}