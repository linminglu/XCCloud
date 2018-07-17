using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Food_Sale
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? FlowType { get; set; }
public int? SingleType { get; set; }
public string FoodID { get; set; }
public int? SaleCount { get; set; }
public int? Point { get; set; }
public int? PointBalance { get; set; }
public int? MemberLevelID { get; set; }
public decimal? Deposit { get; set; }
public decimal? OpenFee { get; set; }
public decimal? RenewFee { get; set; }
public decimal? ChangeFee { get; set; }
public decimal? ReissueFee { get; set; }
public decimal? TotalMoney { get; set; }
public int? BuyFoodType { get; set; }
public decimal? TaxFee { get; set; }
public decimal? TaxTotal { get; set; }
public string Note { get; set; }
public int? SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
