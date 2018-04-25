using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace XCCloudService.OrderPayCallback.Common
{
    public static class PayList
    {
        static List<PayItem> clientPayList = new List<PayItem>();

        public static void Init()
        {
            DataAccess ac = new DataAccess();
            string sql = "SELECT * FROM Data_Order WHERE PayStatus = 1 AND DATEADD(HOUR, 2, PayTime) > GETDATE()";
            DataTable dt = ac.GetTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                AddNewItem(row["OrderID"].ToString(), row["Price"].ToString());
            }
        }

        /// <summary>
        /// 收到新的支付成功订单
        /// </summary>
        /// <param name="tradeNum">订单编号</param>
        public static void AddNewItem(string tradeNum, string money)
        {
            PayItem item = new PayItem()
            {
                tradeNum = tradeNum,
                postTime = DateTime.Now,
                PayAmount = money
            };
            clientPayList.Add(item);

            ClearTimeout();
        }

        static void ClearTimeout()
        {
            lock (clientPayList)
            {
                DateTime d = DateTime.Now;
            continueline:
                foreach (PayItem item in clientPayList)
                {
                    if (item.postTime.AddHours(2) < d)
                    {
                        clientPayList.Remove(item);
                        goto continueline;
                    }
                }
            }
        }

        public static bool GetItem(string tradeNum, out string Amount)
        {
            Amount = "";
            lock (clientPayList)
            {
                var list = clientPayList.Where(p => p.tradeNum == tradeNum);
                if (list.Count() > 0)
                {
                    Amount = list.FirstOrDefault().PayAmount;
                    clientPayList.Remove(list.FirstOrDefault());
                    return true;
                }
                else
                    return false;
            }
        }
    }

    /// <summary>
    ///付款队列元素
    /// </summary>
    public class PayItem
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string tradeNum = "";
        /// <summary>
        /// 交易成功时间戳
        /// </summary>
        public DateTime postTime;
        /// <summary>
        /// 支付金额
        /// </summary>
        public string PayAmount = "";
    }
}