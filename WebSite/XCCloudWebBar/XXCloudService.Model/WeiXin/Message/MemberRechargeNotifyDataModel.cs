using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.WeiXin.Message
{
    public class MemberRechargeNotifyDataModel
    {
        public MemberRechargeNotifyDataModel()
        {

        }

        public MemberRechargeNotifyDataModel(string accountType, string account, string amount, string status, string remark)
        {
            this.AccountType = AccountType;
            this.Account = account;
            this.Amount = amount;
            this.Status = status;
            this.Remark = remark;
        }

        public string AccountType { get; set; }

        public string Account { get; set; }

        public string Amount { set; get; }

        public string Status { set; get; }

        public string Remark { set; get; }

    }
}
