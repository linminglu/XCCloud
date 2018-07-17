using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_Member_Card
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string ICCardID { get; set; }
public string ParentCard { get; set; }
public int? JoinChannel { get; set; }
public string CardPassword { get; set; }
public int? CardType { get; set; }
public int? CardShape { get; set; }
public string CardPhoto { get; set; }
public string CardName { get; set; }
public int? CardSex { get; set; }
public DateTime? CardBirthDay { get; set; }
public string FaceReadID { get; set; }
public int? CardLimit { get; set; }
public string MemberID { get; set; }
public int? MemberLevelID { get; set; }
public DateTime? CreateTime { get; set; }
public DateTime? EndDate { get; set; }
public string LastStore { get; set; }
public DateTime? UpdateTime { get; set; }
public int? Deposit { get; set; }
public int? RepeatCode { get; set; }
public string UID { get; set; }
public int? IsLock { get; set; }
public int? CardStatus { get; set; }
public string OrderID { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
