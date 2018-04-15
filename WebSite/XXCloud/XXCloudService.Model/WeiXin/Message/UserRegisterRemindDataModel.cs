﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.WeiXin.Message
{
    public class UserRegisterRemindDataModel
    {
        public UserRegisterRemindDataModel()
        { 
            
        }

        public UserRegisterRemindDataModel(string userName, string registerTime, string workId, string message)
        {
            this.UserName = userName;
            this.RegisterTime = registerTime;
            this.WorkId = workId;            
            this.Message = message;
        }

        public string UserName { set; get; }

        public string RegisterTime { set; get; }

        public string WorkId { set; get; }

        public string Message { set; get; }

    }
}
