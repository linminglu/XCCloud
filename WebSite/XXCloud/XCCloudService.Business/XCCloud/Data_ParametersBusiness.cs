using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_ParametersBusiness
	{        
        public static IData_ParametersService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ParametersService>();
            }
            
                       
        }
        
        public static IData_ParametersService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ParametersService>(resolveNew: true);
            }
            
                       
        }
	} 
}