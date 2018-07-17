using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Store_CheckDate
{
public int ID { get; set; }
public DateTime? CheckDate { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public DateTime? CreateTime { get; set; }
public int? AuthorID { get; set; }
public DateTime? AuthorTime { get; set; }
public decimal? CashTotle { get; set; }
public decimal? AliPay { get; set; }
public decimal? AliPayFee { get; set; }
public decimal? Wechat { get; set; }
public decimal? WechatFee { get; set; }
public decimal? GroupTotle { get; set; }
public decimal? GroupFee { get; set; }
public int? MemberCount { get; set; }
public int? MemberCardCount { get; set; }
public decimal? DepositTotle { get; set; }
}
}
