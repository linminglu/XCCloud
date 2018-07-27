using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_Food_Level
{
public int? ID { get; set; }
public string MerchID { get; set; }
public int? FoodID { get; set; }
public int? MemberLevelID { get; set; }
public int? TimeType { get; set; }
public string Week { get; set; }
public TimeSpan? StartTime { get; set; }
public TimeSpan? EndTime { get; set; }
public decimal? VIPPrice { get; set; }
public decimal? ClientPrice { get; set; }
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public int? AllFreqType { get; set; }
public int? AllCount { get; set; }
public int? MemberFreqType { get; set; }
public int? MemberCount { get; set; }
public int? UpdateLevelID { get; set; }
public int? PriorityLevel { get; set; }
public string Verifiction { get; set; }
}
}
