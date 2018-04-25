using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Project_BindGameBusiness
	{        
        public static IData_Project_BindGameService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Project_BindGameService>();
            }                       
        }
        
        public static IData_Project_BindGameService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Project_BindGameService>(resolveNew: true);
            }                    
        }
	} 
}