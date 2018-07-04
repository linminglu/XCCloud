using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class Data_GameEncourageList
    {
        public int ID { get; set; }
        public int? State { get; set; }
        public string GameName { get; set; }
        public string ValidDate { get; set; }
        private string Note { get; set; }           
    }
}
