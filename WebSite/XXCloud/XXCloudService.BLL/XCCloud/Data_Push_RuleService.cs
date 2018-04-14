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
	public class Data_Push_RuleService : BaseService<Data_Push_Rule>, IData_Push_RuleService
	{
		public override void SetDal()
        {

        }

        public Data_Push_RuleService()
            : this(false)
        {

        }

        public Data_Push_RuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Push_RuleDAL>(resolveNew: resolveNew);
        }
	} 
}