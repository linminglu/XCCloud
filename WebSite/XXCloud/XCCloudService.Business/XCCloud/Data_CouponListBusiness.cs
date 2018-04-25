using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_CouponListBusiness
	{        
        public static IData_CouponListService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CouponListService>();
            }
            
                       
        }
        
        public static IData_CouponListService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CouponListService>(resolveNew: true);
            }
            
                       
        }
	} 
}