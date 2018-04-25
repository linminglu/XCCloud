using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Dict_AreaBusiness
	{        
        public static IDict_AreaService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IDict_AreaService>();
            }
            
                       
        }
        
        public static IDict_AreaService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IDict_AreaService>(resolveNew: true);
            }
            
                       
        }
	} 
}