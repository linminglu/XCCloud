using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Food_StoreListBusiness
	{        
        public static IData_Food_StoreListService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_StoreListService>();
            }
            
                       
        }
        
        public static IData_Food_StoreListService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_StoreListService>(resolveNew: true);
            }
            
                       
        }
	} 
}