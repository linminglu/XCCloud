using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.TableMemory
{
    public static class flw_ticket_exit
    {
        public class Table
        {
            public String ID { get; set; }
            public String MerchID { get; set; }
            public String StoreID { get; set; }
            public String Segment { get; set; }
            public String HeadAddress { get; set; }
            public Int32 DeviceID { get; set; }
            public Int32 Barcode { get; set; }
            public Int32 BalanceIndex { get; set; }
            public Int32 Coins { get; set; }
            public DateTime RealTime { get; set; }
            public Decimal CoinMoney { get; set; }
            public String UserID { get; set; }
            public String Schedule { get; set; }
            public String AuthorID { get; set; }
            public String Note { get; set; }
            public String MacAddress { get; set; }
            public String DiskID { get; set; }
            public String WorkStation { get; set; }
            public DateTime ChargeTime { get; set; }
            public UInt64 State { get; set; }
            public String PWD { get; set; }
            public UInt64 isNoAllow { get; set; }
            public String ICCardID { get; set; }
        }
        /// <summary>
        /// 创建条形码纪录
        /// </summary>
        /// <param name="t">表数据</param>
        /// <returns>返回条形码序号，失败则返回0</returns>
        public static UInt32 CreateBarCode(Table t)
        {
            UInt32 条码号 = 0;
            string sql = "";
            if (t.ICCardID == "")
            {
                sql = string.Format("insert into flw_ticket_exit (Segment,HeadAddress,Barcode,Coins,RealTime,State,PWD,isNoAllow) values ('{0}','{1}','','{2}','{3} {4}',0,'{5}',0);select @@identity as id;", t.Segment, t.HeadAddress, t.Coins, t.RealTime.ToShortDateString(), t.RealTime.ToLongTimeString(), t.PWD);
            }
            else
            {
                sql = string.Format("insert into flw_ticket_exit (Segment,HeadAddress,Barcode,Coins,RealTime,State,PWD,isNoAllow,ICCardID) values ('{0}','{1}','','{2}','{3} {4}',0,'{5}',0,'{6}');select @@identity as id;", t.Segment, t.HeadAddress, t.Coins, t.RealTime.ToShortDateString(), t.RealTime.ToLongTimeString(), t.PWD, t.ICCardID);
            }
            DataTable newcoin = DataAccess.ExecuteQuery(sql).Tables[0];
            if (newcoin.Rows.Count > 0)
            {
                条码号 = Convert.ToUInt32(newcoin.Rows[0]["id"].ToString());
                string 条码 = t.Segment + PubLib.UInt32ToHexString(条码号) + t.PWD;
                DataAccess.Execute(string.Format("update flw_ticket_exit set barcode='{0}' where id='{1}'", 条码, 条码号));
            }
            return 条码号;
        }

        public static void UpdatePrintDate(uint 条码号, DateTime 打印时间)
        {
            DataAccess.Execute(string.Format("update flw_ticket_exit set RealTime='{0} {1}' where id='{2}'", 打印时间.ToShortDateString(), 打印时间.ToLongTimeString(), 条码号));
        }
        public static void LockBarCode(UInt32 ID)
        {
            DataAccess.Execute(string.Format("update flw_ticket_exit set isNoAllow=1 where id='{0}'", ID));
        }
    }
}
