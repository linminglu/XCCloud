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

        [DataMember(Name = "name", Order = 2)]
        public string RuleName { get; set; }

        [DataMember(Name = "ruleType", Order = 3)]
        public int RuleType { get; set; }

        [DataMember(Name = "typeName", Order = 4)]
        public string TypeName { get; set; }

        [DataMember(Name = "couponType", Order = 5)]
        public int? CouponType { get; set; }
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
