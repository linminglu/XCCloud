using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class Data_WorkstationList
    {
        public int ID { get; set; }
        public int? DepotID { get; set; }
        public string DepotName { get; set; }
        private int State { get; set; }        
        public string StateStr
        {
            get
            {
                return State == 0 ? "禁用" : State == 1 ? "启用" : string.Empty;
            }
            set { }
        }       
    }
}
