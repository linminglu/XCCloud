using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_FoodInfoBusiness
	{        
        public static IData_FoodInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_FoodInfoService>();
            }
            
                       
        }
        
        public static IData_FoodInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_FoodInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}