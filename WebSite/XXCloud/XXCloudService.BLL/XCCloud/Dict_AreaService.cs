﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XCCloudService.BLL.XCCloud
{
    public partial class Dict_AreaService : BaseService<Dict_Area>, IDict_AreaService
    {
        public override void SetDal()
        {

        }

        public Dict_AreaService()
            : this(false)
        {

        }

        public Dict_AreaService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IDict_AreaDAL>(resolveNew: resolveNew);
        }
    }
}
