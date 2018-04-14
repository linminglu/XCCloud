﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class LotteryOperateResultNotifyRequestModel
    {
        public LotteryOperateResultNotifyRequestModel()
        {

        }

        [DataMember(Name = "result_code", Order = 1)]
        public string Result_Code { set; get; }

        [DataMember(Name = "result_msg", Order = 2)]
        public string Result_Msg { set; get; }

        [DataMember(Name = "signkey", Order = 3)]
        public string SignKey { set; get; }

        [DataMember(Name = "result_data", Order = 3)]
        public string Result_Data { set; get; }

        [DataMember(Name = "sn", Order = 4)]
        public string SN { set; get; }

        public string StoreId { set; get; }
    }


}
