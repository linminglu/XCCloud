using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_ProjectTime_StoreListBusiness
	{        
        public static IData_ProjectTime_StoreListService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectTime_StoreListService>();
            }
            
                       
        }
        
        public static IData_ProjectTime_StoreListService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectTime_StoreListService>(resolveNew: true);
            }
            
                       
        }
	} 
}