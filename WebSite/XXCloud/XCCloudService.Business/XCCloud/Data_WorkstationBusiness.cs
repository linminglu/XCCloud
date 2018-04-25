using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_WorkstationBusiness
	{        
        public static IData_WorkstationService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_WorkstationService>();
            }
            
                       
        }
        
        public static IData_WorkstationService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_WorkstationService>(resolveNew: true);
            }
            
                       
        }
	} 
}