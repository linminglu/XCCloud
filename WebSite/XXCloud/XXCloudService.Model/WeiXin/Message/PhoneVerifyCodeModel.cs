using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XCCloudService.Model.WeiXin.Message
{
    public class PhoneVerifyCodeModel
    {
        public PhoneVerifyCodeModel()
        {

        }
        public PhoneVerifyCodeModel(string _keyword1, string _keyword2, string _remark)
        {
            this.keyword1 = _keyword1;
            this.keyword2 = _keyword2;
            this.remark = _remark;
        }
        public string keyword1 { get; set; }
        public string keyword2 { get; set; }
        public string remark { get; set; }
    }

    [WeixinMsgConfigAttribute(MsgType = "PhoneVerifyCode")]
    [XmlRoot(ElementName = "msgtemplate")]
    public class PhoneVerifyCodeConfigModel
    {
        [XmlElement(ElementName = "title")]
        public string Title { set; get; }

        [XmlElement(ElementName = "templateid")]
        public string TemplateId { set; get; }

        [XmlElement(ElementName = "detailsurl")]
        public string DetailsUrl { set; get; }

        [XmlElement(ElementName = "sapppagepath")]
        public string SAppPagePath { set; get; }

        [XmlElement(ElementName = "remark")]
        public string Remark { set; get; }

        [XmlElement(ElementName = "firstcolor")]
        public string FirstColor { set; get; }

        [XmlElement(ElementName = "keynote1color")]
        public string Keynote1Color { set; get; }

        [XmlElement(ElementName = "keynote2color")]
        public string Keynote2Color { set; get; }

        [XmlElement(ElementName = "keynote3color")]
        public string Keynote3Color { set; get; }

        [XmlElement(ElementName = "remarkcolor")]
        public string RemarkColor { set; get; }
    }
}
