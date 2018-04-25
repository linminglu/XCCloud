using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_MerchAlipayBusiness
	{        
        public static IBase_MerchAlipayService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MerchAlipayService>();
            }
            
                       
        }
        
        public static IBase_MerchAlipayService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MerchAlipayService>(resolveNew: true);
            }
            
                       
        }
	} 
}