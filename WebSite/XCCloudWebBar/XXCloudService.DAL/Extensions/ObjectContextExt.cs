using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.DAL
{
    public static class ObjectContextExt
    {
        public static void ExecuteStoredProcedure(this DbContext dbContext, string storedProcName, params object[] parameters)
        {
            //var context = new ObjectContext("name=XCCloudDBEntities"); 
            ObjectContext context = ((IObjectContextAdapter)dbContext).ObjectContext;
            string command = "EXEC " + storedProcName + " ";
            if (parameters != null && parameters.Length > 0)
            {
                command = command + String.Join(",", parameters.Cast<SqlParameter>().Select(pi => String.Format("{0} {1}", pi.ParameterName, pi.Direction == ParameterDirection.Input ? String.Empty : "out")));
            }

            context.ExecuteStoreCommand(command, parameters);
        }
    }
}
