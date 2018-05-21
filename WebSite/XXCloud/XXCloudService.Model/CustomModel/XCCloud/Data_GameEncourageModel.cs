using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_GameEncourageList
    {
        public int ID { get; set; }
        public string GameName { get; set; }
        public string ValidDate { get; set; }
        private string Note { get; set; }           
    }
}
