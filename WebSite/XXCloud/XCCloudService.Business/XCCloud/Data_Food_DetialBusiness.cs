using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Food_DetialBusiness
	{        
        public static IData_Food_DetialService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_DetialService>();
            }
            
                       
        }
        
        public static IData_Food_DetialService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_DetialService>(resolveNew: true);
            }
            
                       
        }
	} 
}