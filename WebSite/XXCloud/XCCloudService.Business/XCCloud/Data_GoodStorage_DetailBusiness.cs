using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GoodStorage_DetailBusiness
	{        
        public static IData_GoodStorage_DetailService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodStorage_DetailService>();
            }
            
                       
        }
        
        public static IData_GoodStorage_DetailService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodStorage_DetailService>(resolveNew: true);
            }
            
                       
        }
	} 
}