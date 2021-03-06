﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XCCloudWebBar.Model.WeiXin.Message;

namespace XCCloudWebBar.Model.WeiXin.SAppMessage
{
    [SAppMsgConfigAttribute(MsgType = "MemberCoinsNotify")]
    [XmlRoot(ElementName = "msgtemplate")]
    public class MemberCoinsConfigModel
    {
        [XmlElement(ElementName="title")]
        public string Title { set;get; }

        [XmlElement(ElementName = "templateid")]
        public string TemplateId { set; get; }

        [XmlElement(ElementName = "detailsurl")]
        public string DetailsUrl { set; get; }
    }
}
