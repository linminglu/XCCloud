﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.DAL.Base;
using XCCloudWebBar.DAL.IDAL;
using XCCloudWebBar.DAL.XCGame.IDAL;
using XCCloudWebBar.Model;
using XCCloudWebBar.Model.XCGame;

namespace XCCloudWebBar.DAL.XCGame
{

    public partial class HeadDAL : BaseDAL<t_head>, IHeadDAL
    {
        public HeadDAL(string containerName)
            : base(containerName)
        { 
            
        }
    }
}