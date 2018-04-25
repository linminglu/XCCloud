using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Member_CardBusiness
	{        
        public static IData_Member_CardService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Member_CardService>();
            }
            
                       
        }
        
        public static IData_Member_CardService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Member_CardService>(resolveNew: true);
            }
            
                       
        }
	} 
}