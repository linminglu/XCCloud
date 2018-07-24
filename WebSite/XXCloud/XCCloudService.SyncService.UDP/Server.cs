using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.Common.Enum;

namespace XCCloudService.SyncService.UDP
{
    public class Server
    {
        public static DSS.Server.Server server = null;

        public static void Init(int Port)
        {
            try
            {
                //DSS.DataAccess.SQLConnectString = "Data Source = 192.168.1.119;Initial Catalog=XCCloudWEB;User Id = sa;Password = xinchen;Connection Timeout=30;";
                DSS.DataAccess.SQLConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["XCCloudDB"].ToString();
                server = new DSS.Server.Server();
                server.Init(Port);
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog("SyncServer初始化失败:" + ex.Message);
            }
        }

        public static void CloudDataAsync(string merchID, string secret, string tableName, string idValue, int action, bool writeBuf = true)
        {
            if (server != null)
                server.CloudDataAsync(merchID, secret, tableName, idValue, action, (r) => { if (!r) LogHelper.SaveLog("SyncServer数据同步失败"); }, writeBuf);
        }

        public static void CloudDataSync(string merchID, string secret, string tableName, string idValue, int action, bool writeBuf = true)
        {
            if (server != null)
                server.CloudDataSync(merchID, secret, tableName, idValue, action, writeBuf);
        }
    }
}
