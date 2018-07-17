using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_Food_Record
{
public string MerchID { get; set; }
public int? FoodID { get; set; }
public int? FoodLevelID { get; set; }
public DateTime? RecordDate { get; set; }
public int? day_sale_count { get; set; }
public int? member_day_sale_count { get; set; }
public string Verifiction { get; set; }
}
}
