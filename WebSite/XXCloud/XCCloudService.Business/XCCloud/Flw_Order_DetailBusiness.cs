using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_Order_DetailBusiness
	{        
        public static IFlw_Order_DetailService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Order_DetailService>();
            }
            
                       
        }
        
        public static IFlw_Order_DetailService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Order_DetailService>(resolveNew: true);
            }
            
                       
        }
	} 
}