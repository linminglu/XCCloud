﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCCloud;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Model.XCCloud;
namespace XCCloudService.BLL.XCCloud
{
	public class Base_DepotInfoService : BaseService<Base_DepotInfo>, IBase_DepotInfoService
	{
        public override void SetDal()
        {
        	
        }
        
        public Base_DepotInfoService()
        	: this(false)
        {
            
        }
        
        public Base_DepotInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_DepotInfoDAL>(resolveNew: resolveNew);
        }
	} 
}