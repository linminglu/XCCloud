using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_Discount_Detail
{
public int ID { get; set; }
public string MerchID { get; set; }
public int? DiscountRuleID { get; set; }
public decimal? LimitCount { get; set; }
public int? LimitType { get; set; }
public decimal? ConsumeCount { get; set; }
public decimal? DiscountCount { get; set; }
public string Verifiction { get; set; }
}
}
