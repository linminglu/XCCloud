using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_ProjectTime_BandPriceBusiness
	{        
        public static IData_ProjectTime_BandPriceService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectTime_BandPriceService>();
            }
            
                       
        }
        
        public static IData_ProjectTime_BandPriceService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectTime_BandPriceService>(resolveNew: true);
            }
            
                       
        }
	} 
}