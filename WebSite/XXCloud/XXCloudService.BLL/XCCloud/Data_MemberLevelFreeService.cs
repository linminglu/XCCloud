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
	public class Data_MemberLevelFreeService : BaseService<Data_MemberLevelFree>, IData_MemberLevelFreeService
	{
        public override void SetDal()
        {
        	
        }
        
        public Data_MemberLevelFreeService()
        	: this(false)
        {
            
        }
        
        public Data_MemberLevelFreeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MemberLevelFreeDAL>(resolveNew: resolveNew);
        }
	} 
}