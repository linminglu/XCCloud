using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace XXCloudService.Utility.Info
{
    public class RouterInfo
    {
        public RouterInfo()
        {
            this.Online = false;
            this.RemotePoint = string.Empty;
        }

        public int RouteId { get; set; }

        public string RouteToken { get; set; }

        public string RouteName { get; set; }

        public string Segment { get; set; }

        public string RemotePoint { get; set; }

        public bool Online { get; set; }

        public string UpdateTime { get; set; }
    }

    public class DeviceModel
    {
        public int RouterId { get; set; }

        public int DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string DeviceType { get; set; }

        public string SiteName { get; set; }

        public string MCUID { get; set; }

        public string Address { get; set; }

        public string State { get; set; }
    }
}