using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Game_StockInfoBusiness
	{        
        public static IData_Game_StockInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Game_StockInfoService>();
            }
            
                       
        }
        
        public static IData_Game_StockInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Game_StockInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}