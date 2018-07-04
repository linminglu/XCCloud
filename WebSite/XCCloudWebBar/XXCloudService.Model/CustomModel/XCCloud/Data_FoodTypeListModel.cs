using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class Data_FoodTypeListModel
    {
        public int ID { get; set; }
        public string DictKey { get; set; }
        public string DictValue { get; set; }
        public string Comment { get; set; }
        public int? OrderID { get; set; }
        public int? Enabled { get; set; }
        public string EnabledStr { get; set; }        
    }
}
