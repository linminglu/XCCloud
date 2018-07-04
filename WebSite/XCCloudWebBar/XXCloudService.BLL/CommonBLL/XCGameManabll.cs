using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.DAL.CommonDAL;

namespace XCCloudWebBar.BLL.CommonBLL
{
    public class XCGameManabll
    {
        public static DataSet ExecuteQuerySentence(string strSQL, SqlParameter[] paramArr)
        {
            return new DataAccess(DataAccessDB.XCGameManaDB).ExecuteQuerySentence(strSQL, paramArr);
        }
    }
}
