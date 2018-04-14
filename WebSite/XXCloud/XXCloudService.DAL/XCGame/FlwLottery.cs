﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.Base;
using XCCloudService.DAL.IDAL;
using XCCloudService.DAL.XCGame.IDAL;
using XCCloudService.Model;
using XCCloudService.Model.XCGame;

namespace XCCloudService.DAL.XCGame
{

    public partial class FlwLotteryDAL : BaseDAL<flw_lottery>, IFlwLotteryDAL
    {
        public FlwLotteryDAL(string containerName)
            : base(containerName)
        { 
            
        }
    }
}
