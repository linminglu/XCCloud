using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.WeiXin.Message
{
    public class OrderAuditDataModel
    {
        public string AuditId {set;get;}

        public string SendTime {set;get;}

        public string SendUserId {set;get;}

        public string SendUserName { set; get; }

        public string OrderId {set;get;}

        public string Remark {set;get;}
    }
}
