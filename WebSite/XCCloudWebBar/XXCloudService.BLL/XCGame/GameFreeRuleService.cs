﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCGame;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL;
using XCCloudWebBar.DAL.XCGame.IDAL;
using XCCloudWebBar.Model;
using XCCloudWebBar.Model.XCGame;

namespace XCCloudWebBar.BLL.XCGame
{
    public partial class GameFreeRuleService : BaseService<t_game_free_rule>, IGameFreeRuleService
    {
        private IGameFreeRuleDAL StaffDAL;

        public GameFreeRuleService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IGameFreeRuleDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
}
