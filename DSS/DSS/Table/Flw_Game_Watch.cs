using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Game_Watch
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int GameIndex { get; set; }
public int HeadIndex { get; set; }
public string MediaURL1 { get; set; }
public string MediaURL2 { get; set; }
public string MediaURL3 { get; set; }
public DateTime CreateTime { get; set; }
public int UserID { get; set; }
public int InCoin { get; set; }
public int InCoinError { get; set; }
public int InCoin2 { get; set; }
public int InCoinError2 { get; set; }
public int PrizeCount { get; set; }
public int PrizeError { get; set; }
public decimal GoodPrice { get; set; }
public int OutCoin { get; set; }
public int OutCoinError { get; set; }
public int OutLottery { get; set; }
public int OutLotteryError { get; set; }
public int Winner { get; set; }
public int WinnerError { get; set; }
public string Verifiction { get; set; }
}
}
