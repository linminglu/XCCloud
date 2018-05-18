using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class RuleOverlyingModel
    {
        [DataMember(Name = "ruleId", Order = 1)]
        public int ID { get; set; }

        [DataMember(Name = "pId", Order = 2)]
        public Nullable<int> PID { get; set; }

        [DataMember(Name = "name", Order = 3)]
        public string RuleName { get; set; }

        [DataMember(Name = "ruleType", Order = 4)]
        public int RuleType { get; set; }

        [DataMember(Name = "typeName", Order = 5)]
        public string TypeName { get; set; }

        [DataMember(Name = "children", Order = 6)]
        public List<RuleOverlyingModel> Children { get; set; }
    }

    [DataContract]
    public class QueryRuleOverlyingData
    {
        [DataMember(Name = "treeNodes", Order = 1)]
        public List<RuleOverlyingModel> TreeNodes { get; set; }

        [DataMember(Name = "list", Order = 2)]
        public List<RuleOverlyingModel> List { get; set; }
    }
}
