using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;
using System.Data;

namespace RadarService.Info
{
    public class TicketInfo
    {
        /// <summary>
        /// 需要签出，且未签出的门票
        /// </summary>
        public static List<TicketItem> UnCheckTicket = new List<TicketItem>();

        public class TicketItem
        {
            public string PlayID = "";
            public string ICCardID = "";
            public string BuyID = "";
            public string Barcode = "";
            public string ProjectName = "";
            public string ProjectType = "";
            public int RemainCount = 0;
            public int PulseCount = 0;
            public string Checkout = "";
            public bool isCheckIn = false;
        }
        public TicketItem CheckTicket(string ICCardID, string Segment, string HeadAddress)
        {
            string sql = string.Format("select l.BuyID,l.Barcode,h.Segment,h.HeadAddress,p.ProjectName,l.ProjectType,p.Checkout,p.PulseCount,l.RemainCount,l.StartTime,l.EndTime from flw_project_buy b,flw_project_buy_codelist l,t_game_project g,t_head h,t_project p where b.ID=l.BuyID and l.ProjectID=p.id and g.GameID=h.GameID and g.ProjectID=p.id and b.ICCardID='{0}' and h.Segment='{1}' and h.HeadAddress='{2}' and l.EndTime>=GETDATE() and l.RemainCount>0 and l.State=0", ICCardID, Segment, HeadAddress);
            DataTable dt = DataAccess.ExecuteQueryReturnTable(sql);
            if (dt.Rows.Count > 0)
            {
                TicketItem t = new TicketItem();
                t.BuyID = dt.Rows[0]["BuyID"].ToString();
                t.Barcode = dt.Rows[0]["Barcode"].ToString();
                t.ProjectName = dt.Rows[0]["ProjectName"].ToString();
                t.ProjectType = dt.Rows[0]["ProjectType"].ToString();
                t.RemainCount = Convert.ToInt32(dt.Rows[0]["RemainCount"].ToString());
                t.PulseCount = Convert.ToInt32(dt.Rows[0]["PulseCount"].ToString());
                t.Checkout = dt.Rows[0]["Checkout"].ToString();

                return t;
            }
            return null;
        }

        public bool UseTicket(TicketItem ticket, HeadInfo.机头信息 head)
        {
            string sql = "";
            if (ticket.isCheckIn)
            {
                sql = string.Format("update flw_project_buy_codelist set UseTime=GETDATE(),HeadID='" + head.常规.机头编号 + "',RemainCount='" + ticket.RemainCount + "' where Barcode='" + ticket.Barcode + "';");
                sql += string.Format("insert into flw_project_play (BuyID,BarCode,ICCardID,CheckType,InTime,InSegment,InHeadAddress) values ('{0}','{1}','{2}','{3}',GETDATE(),'{4}','{5}')",
                    ticket.BuyID, ticket.Barcode, ticket.ICCardID, ticket.Checkout, head.常规.路由器编号, head.常规.机头地址);
                if (ticket.Checkout == "1")
                    UnCheckTicket.Add(ticket);
            }
            else
            {
                sql += string.Format("update flw_project_play set OutTime=GETDATE(),OutSegment='{0}',OutHeadAddress='{1}' where ID='{2}'",
                    head.常规.路由器编号, head.常规.机头地址, ticket.PlayID);
            }
            return DataAccess.Execute(sql) > 0;
        }
    }
}
