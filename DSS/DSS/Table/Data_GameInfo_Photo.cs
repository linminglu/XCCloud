using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GameInfo_Photo
{
public int ID { get; set; }
public int? GameID { get; set; }
public string PhotoURL { get; set; }
public DateTime? UploadTime { get; set; }
}
}
