﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_GroupVerityBusiness
	{        
        public static IFlw_GroupVerityService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_GroupVerityService>();
            }
            
                       
        }
        
        public static IFlw_GroupVerityService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_GroupVerityService>(resolveNew: true);
            }
            
                       
        }
	} 
}