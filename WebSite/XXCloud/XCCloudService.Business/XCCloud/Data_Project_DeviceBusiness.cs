using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Project_DeviceBusiness
	{        
        public static IData_Project_DeviceService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Project_DeviceService>();
            }
            
                       
        }
        
        public static IData_Project_DeviceService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Project_DeviceService>(resolveNew: true);
            }
            
                       
        }
	} 
}