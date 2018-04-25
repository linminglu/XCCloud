using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_CouponInfoBusiness
	{        
        public static IData_CouponInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CouponInfoService>();
            }
            
                       
        }
        
        public static IData_CouponInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CouponInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}