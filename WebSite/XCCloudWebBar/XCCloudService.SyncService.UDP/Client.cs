using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using XCCloudWebBar.Common;
//using XCCloudWebBar.Common.Enum;

namespace XCCloudService.SyncService.UDP
{
    public class Client
    {
        public static DSS.Client.Client client = null;

        public static void Init(string serverIP, int serverPort, string storeID, string merchID, string secret)
        {
            try
            {
                client = new DSS.Client.Client(serverIP, serverPort, storeID, merchID, secret);
                client.Init();
            }
            catch (Exception ex)
            {
                //LogHelper.SaveLog("SyncClient初始化失败:" + ex.Message);
            }
        }

        public static void StoreDataSync(string tableName, string idValue, int action, bool writeBuf = true, string sn = "")
        {
            if (client != null)
                client.StoreDataSync(tableName, idValue, action, writeBuf, sn);
        }
    }
}
