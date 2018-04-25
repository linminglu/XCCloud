using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_CouponUseBusiness
	{        
        public static IFlw_CouponUseService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_CouponUseService>();
            }
            
                       
        }
        
        public static IFlw_CouponUseService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_CouponUseService>(resolveNew: true);
            }
            
                       
        }
	} 
}