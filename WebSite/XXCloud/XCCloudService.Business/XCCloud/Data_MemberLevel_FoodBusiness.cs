using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_MemberLevel_FoodBusiness
	{        
        public static IData_MemberLevel_FoodService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MemberLevel_FoodService>();
            }
            
                       
        }
        
        public static IData_MemberLevel_FoodService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MemberLevel_FoodService>(resolveNew: true);
            }
            
                       
        }
	} 
}