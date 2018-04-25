using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Jackpot_MatrixBusiness
	{        
        public static IData_Jackpot_MatrixService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Jackpot_MatrixService>();
            }
            
                       
        }
        
        public static IData_Jackpot_MatrixService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Jackpot_MatrixService>(resolveNew: true);
            }
            
                       
        }
	} 
}