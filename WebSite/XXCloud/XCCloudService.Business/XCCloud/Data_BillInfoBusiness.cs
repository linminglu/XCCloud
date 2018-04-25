using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_BillInfoBusiness
	{        
        public static IData_BillInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_BillInfoService>();
            }
            
                       
        }
        
        public static IData_BillInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_BillInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}