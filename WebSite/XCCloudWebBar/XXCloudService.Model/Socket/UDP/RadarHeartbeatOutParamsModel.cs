﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.Socket.UDP
{
    public class RadarHeartbeatOutParamsModel
    {
        public RadarHeartbeatOutParamsModel(byte[] responsePackages, object responseModel, string responseJson)
        {
            this.ResponsePackages = responsePackages;
            this.ResponseModel = responseModel;
            this.ResponseJson = responseJson;
        }

        /// <summary>
        /// 响应数据包
        /// </summary>
        public byte[] ResponsePackages { set; get; }

        public object ResponseModel { set; get; }

        public string ResponseJson { set; get; }
    }
}
