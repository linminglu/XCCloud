using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GameFreeRule_ListBusiness
	{        
        public static IData_GameFreeRule_ListService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameFreeRule_ListService>();
            }
            
                       
        }
        
        public static IData_GameFreeRule_ListService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameFreeRule_ListService>(resolveNew: true);
            }
            
                       
        }
	} 
}