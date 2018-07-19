using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_BillInfo
{
public int? ID { get; set; }
public string StoreName { get; set; }
public string StoreID { get; set; }
public DateTime? Time { get; set; }
public DateTime? ReleaseTime { get; set; }
public string PicturePath { get; set; }
public string Title { get; set; }
public string PagePath { get; set; }
public string Publisher { get; set; }
public int? State { get; set; }
public int? PublishType { get; set; }
public int? PromotionType { get; set; }
public string Verifiction { get; set; }
}
}
