using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GoodsStockBusiness
	{        
        public static IData_GoodsStockService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodsStockService>();
            }
            
                       
        }
        
        public static IData_GoodsStockService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodsStockService>(resolveNew: true);
            }
            
                       
        }
	} 
}