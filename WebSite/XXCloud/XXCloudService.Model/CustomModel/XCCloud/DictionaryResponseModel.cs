using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public partial class DictionaryResponseModel
    {
        [DataMember(Name = "name", Order = 1)]
        public string DictKey { get; set; }        

        [DataMember(Name = "children", Order = 2)]
        public List<DictionaryResponseModel> Children { get; set; }

        [DataMember(Name = "id", Order = 3)]
        public int ID { get; set; }

        [IgnoreDataMember]
        public Nullable<int> PID { get; set; }

        [DataMember(Name = "dictValue", Order = 4)]
        public string DictValue { get; set; }

        [DataMember(Name = "comment", Order = 4)]
        public string Comment { get; set; }

        [DataMember(Name = "orderId", Order = 4)]
        public Nullable<int> OrderID { get; set; }

        [DataMember(Name = "enabled", Order = 5)]
        public Nullable<int> Enabled { get; set; }

        [DataMember(Name = "merchId", Order = 6)]
        public string MerchID { get; set; }

        [DataMember(Name = "checked", Order = 7)]
        public bool Checked { get { return false; } set { } }        
        
        [IgnoreDataMember]
        public Nullable<int> DictLevel { get; set; }
    }

    [DataContract]
    public class GameListModel : DictionaryResponseModel
    {
        [DataMember(Name = "gameList", Order = 1)]
        public List<GameInfoModel> GameList { set; get; }

        [DataMember(Name = "children", Order = 2)]
        public List<GameListModel> Children { get; set; }
    } 

    [DataContract]
    public class GameInfoModel
    {
        [DataMember(Name = "id", Order = 1)]
        public int ID { set; get; }

        [DataMember(Name = "name", Order = 2)]
        public string GameName { set; get; }

        [DataMember(Name = "checked", Order = 3)]
        public bool Checked { get { return false; } set { } }
    } 
}
