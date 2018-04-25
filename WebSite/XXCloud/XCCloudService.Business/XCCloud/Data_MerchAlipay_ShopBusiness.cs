using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_MerchAlipay_ShopBusiness
	{        
        public static IData_MerchAlipay_ShopService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MerchAlipay_ShopService>();
            }
            
                       
        }
        
        public static IData_MerchAlipay_ShopService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MerchAlipay_ShopService>(resolveNew: true);
            }
            
                       
        }
	} 
}