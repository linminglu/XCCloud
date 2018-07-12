using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalletService.Model.WorkStation
{
    public class DogTokenModel
    {

        public DogTokenModel(string dogId, string workStation, string merchId, string storeId, string token)
        {
            this.DogId = dogId;
            this.WorkStation = workStation;
            this.MerchId = merchId;
            this.StoreId = storeId;
            this.Token = token;
        }

        public string DogId { set; get; }

        public string WorkStation { set; get; }

        public string MerchId { set; get; }

        public string StoreId { set; get; }

        public string Token { set; get; }
    }
}
