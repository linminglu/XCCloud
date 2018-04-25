using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_Order_SerialNumberBusiness
	{        
        public static IFlw_Order_SerialNumberService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Order_SerialNumberService>();
            }
            
                       
        }
        
        public static IFlw_Order_SerialNumberService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Order_SerialNumberService>(resolveNew: true);
            }
            
                       
        }
	} 
}