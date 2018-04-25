﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GivebackRuleBusiness
	{        
        public static IData_GivebackRuleService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GivebackRuleService>();
            }
            
                       
        }
        
        public static IData_GivebackRuleService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GivebackRuleService>(resolveNew: true);
            }
            
                       
        }
	} 
}