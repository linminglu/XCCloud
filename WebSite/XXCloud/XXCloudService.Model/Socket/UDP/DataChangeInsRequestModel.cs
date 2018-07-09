using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class DataChangeInsRequestModel
    {
        public DataChangeInsRequestModel(string sn, string dataIndex, string tableName, string action, string storeId)
        {
            this.SN = sn;
            this.DataIndex = dataIndex;
            this.TableName = tableName;
            this.Action = action;
            this.StoreId = storeId;
            this.SignKey = "";
        }

        [DataMember(Name = "sn", Order = 1)]
        public string SN { set; get; }

        [DataMember(Name = "dataindex", Order = 2)]
        public string DataIndex { set; get; }

        [DataMember(Name = "tablename", Order = 3)]
        public string TableName { set; get; }

        [DataMember(Name = "action", Order = 4)]
        public string Action { set; get; }

        [DataMember(Name = "storeid", Order = 5)]
        public string StoreId { set; get; }

        [DataMember(Name = "signkey", Order = 6)]
        public string SignKey { set; get; }
    }
}
