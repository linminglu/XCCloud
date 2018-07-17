using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Base_GoodsInfo
{
public int ID { get; set; }
public string StoreID { get; set; }
public string Barcode { get; set; }
public string MerchID { get; set; }
public string GoodName { get; set; }
public string GoodPhoteURL { get; set; }
public int? GoodType { get; set; }
public int? AllowStorage { get; set; }
public decimal? Price { get; set; }
public decimal? Tax { get; set; }
public int? ReturnFlag { get; set; }
public int? AllowCreatePoint { get; set; }
public int? ReturnTime { get; set; }
public int? TimeType { get; set; }
public decimal? ReturnFee { get; set; }
public int? FeeType { get; set; }
public int? Status { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
