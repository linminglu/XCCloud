using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PalletService.Model.WorkStation
{
    [DataContract]
    public class WorkStationRegisterModel
    {
        [DataMember(Name = "dogId", Order = 1)]
        public string DogId { set; get; }

        [DataMember(Name = "workStation", Order = 2)]
        public string WorkStation { set; get; }
    }
}
