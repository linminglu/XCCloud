using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_MerchantInfoBusiness
	{        
        public static IBase_MerchantInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MerchantInfoService>();
            }
            
                       
        }
        
        public static IBase_MerchantInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MerchantInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}