﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCGame;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCGame;
using XCCloudService.Model.XCGame;

namespace XCCloudService.BLL.XCGame
{
    public partial class MemberlevelService : BaseService<t_memberlevel>, IMemberlevelService
    {
        private IMemberlevelDAL StaffDAL;

        public MemberlevelService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IMemberlevelDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
   
}
