using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_MemberLevelBusiness
	{        
        public static IData_MemberLevelService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MemberLevelService>();
            }
            
                       
        }
        
        public static IData_MemberLevelService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MemberLevelService>(resolveNew: true);
            }
            
                       
        }
	} 
}