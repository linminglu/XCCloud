using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Food_Sale_Pay
{
public string ID { get; set; }
public string MerchID { get; set; }
public string FlwFoodID { get; set; }
public int? BalanceIndex { get; set; }
public decimal? OrginalPrice { get; set; }
public decimal? Discount { get; set; }
public decimal? PayCount { get; set; }
public decimal? Balance { get; set; }
public string Verifiction { get; set; }
}
}
