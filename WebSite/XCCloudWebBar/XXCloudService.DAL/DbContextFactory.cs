using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model;
using XCCloudWebBar.Model.XCCloud;
using XCCloudWebBar.Model.XCCloudRS232;
using XCCloudWebBar.Model.XCGame;
using XCCloudWebBar.Model.XCGameManager;
using XCCloudWebBar.Model.XCGameManagerLog;

namespace XCCloudWebBar.DAL
{
    public partial class DbContextFactory
    {
        /// <summary>
        /// 创建EF上下文对象,已存在就直接取,不存在就创建,保证线程内是唯一。
        /// </summary>
        public static DbContext CreateByModelNamespace(string modelNamespace)
        {
            string dbName = GetDBNameByModelNamespace(modelNamespace);
            DbContext dbContext = CallContext.GetData(dbName) as DbContext;
            if (dbContext == null)
            {
                dbContext = GetDbContextByModelNamespace(modelNamespace);
                CallContext.SetData(dbName, dbContext);
            }
            return dbContext;
        }

        public static DbContext CreateByContainerName(string containerName)
        {
            string dbName = containerName;
            DbContext dbContext = CallContext.GetData(dbName) as DbContext;
            if (dbContext == null)
            {
                dbContext = GetDbContextByXCGameDBName(dbName);
                CallContext.SetData(dbName, dbContext);
            }
            return dbContext;
        }        

        private static DbContext GetDbContextByModelNamespace(string modelNamespace)
        {
            switch (modelNamespace)
            {
                case "XCCloudWebBar.Model.XCCloud": return new XCCloudDBEntities();
                case "XCCloudWebBar.Model.XCGameManager": return new XCGameManagerDBEntities();
                case "XCCloudWebBar.Model.XCCloudRS232": return new XCCloudRS232Entities();
                case "XCCloudWebBar.Model.XCGameManagerLog": return new XCGameManagerLogEntities();
                default: return null;
            }
        }

        private static DbContext GetDbContextByXCGameDBName(string xcGameDBName)
        {
            DbContext dbContext = new XCGameDBEntities();
            dbContext.Database.Connection.ConnectionString = XCGameDB.GetConnString(xcGameDBName);
            return dbContext;
        }

        private static string GetDBNameByModelNamespace(string modelNamespace)
        {
            switch (modelNamespace)
            {
                case "XCCloudWebBar.Model.XCGame": return "XCGamedbEntities";
                case "XCCloudWebBar.Model.XCCloud": return "XCCloudDBEntities";
                case "XCCloudWebBar.Model.XCGameManager": return "XCGameManagerDBEntities";
                case "XCCloudWebBar.Model.XCCloudRS232": return "XCCloudRS232Entities";
                case "XCCloudWebBar.Model.XCGameManagerLog": return "XCGameManagerLogEntities";
                default:return "";
            }
        }
    }    
}
