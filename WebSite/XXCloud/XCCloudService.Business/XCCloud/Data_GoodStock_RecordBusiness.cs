using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GoodStock_RecordBusiness
	{        
        public static IData_GoodStock_RecordService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodStock_RecordService>();
            }
            
                       
        }
        
        public static IData_GoodStock_RecordService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodStock_RecordService>(resolveNew: true);
            }
            
                       
        }
	} 
}