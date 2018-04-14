﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.WeiXin.Message
{
    public class XcUserResetPasswordDataModel
    {
        public XcUserResetPasswordDataModel()
        { 
            
        }

        public XcUserResetPasswordDataModel(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }

        public string UserName { set; get; }

        public string Password { set; get; }

    }
}
