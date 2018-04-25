using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Storage_RecordBusiness
	{        
        public static IData_Storage_RecordService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Storage_RecordService>();
            }
            
                       
        }
        
        public static IData_Storage_RecordService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Storage_RecordService>(resolveNew: true);
            }
            
                       
        }
	} 
}