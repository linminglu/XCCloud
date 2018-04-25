using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Coupon_StoreListBusiness
	{        
        public static IData_Coupon_StoreListService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Coupon_StoreListService>();
            }
            
                       
        }
        
        public static IData_Coupon_StoreListService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Coupon_StoreListService>(resolveNew: true);
            }
            
                       
        }
	} 
}