﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class Data_GoodStorageList
    {
        public int ID { get; set; }
        public string StorageOrderID { get; set; }
        public string StoreName { get; set; }
        public string RealTime { get; set; }
        public string Supplier { get; set; }       
        public decimal? TotalPrice { get; set; }
        public string LogName { get; set; }
        public int? DepotID { get; set; }
        public string DepotName { get; set; }
        public int? AuthorFlag { get; set; }
        public string AuthorFlagStr { get { return ((GoodOutInState?)AuthorFlag).GetDescription(); } set { } }        
    }
}