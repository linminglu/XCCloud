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
    public class Data_ProjectTimeInfoService : BaseService<Data_ProjectTimeInfo>, IData_ProjectTimeInfoService
	{
        public override void SetDal()
        {

        }

        public Data_ProjectTimeInfoService()
            : this(false)
        {
        }

        public Data_ProjectTimeInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ProjectTimeInfoDAL>(resolveNew: resolveNew);
        }
	} 
}