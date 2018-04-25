using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Member_Card_StoreBusiness
	{        
        public static IData_Member_Card_StoreService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Member_Card_StoreService>();
            }
            
                       
        }
        
        public static IData_Member_Card_StoreService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Member_Card_StoreService>(resolveNew: true);
            }
            
                       
        }
	} 
}