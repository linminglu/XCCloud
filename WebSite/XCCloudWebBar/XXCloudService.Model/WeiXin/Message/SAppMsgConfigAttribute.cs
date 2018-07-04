﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.WeiXin.Message
{
    /// <summary>
    /// 微信消息配置模式属性
    /// </summary>
    public class SAppMsgConfigAttribute : Attribute
    {
        public string MsgType { set; get; }
    }
}
