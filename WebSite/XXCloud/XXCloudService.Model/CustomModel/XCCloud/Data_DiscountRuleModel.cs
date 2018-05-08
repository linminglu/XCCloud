using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_DiscountRuleModel
    {
        public int ID { get; set; }
        public string RuleName { get; set; }
        public int? ShareCount { get; set; }
        public int? RuleLevel { get; set; }        
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Note { get; set; }
    }
}
