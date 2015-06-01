namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class CityDayAQIPublishLive : Entity
    {
        
        [RoundtripOriginal, DataMember, StringLength(5), ConcurrencyCheck]
        public string AQI { get; set; }

        [RoundtripOriginal, DataMember, StringLength(50), ConcurrencyCheck]
        public string Area { get; set; }

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public int CityCode { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(8), DataMember]
        public string CO_24h { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, DataMember, Editable(false, AllowInitialValue=true), Key]
        public int Id { get; set; }

        [RoundtripOriginal, DataMember, StringLength(0xff), ConcurrencyCheck]
        public string Measure { get; set; }

        [RoundtripOriginal, StringLength(5), ConcurrencyCheck, DataMember]
        public string NO2_24h { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(5), DataMember]
        public string O3_8h_24h { get; set; }

        

        [DataMember, RoundtripOriginal, StringLength(5), ConcurrencyCheck]
        public string PM10_24h { get; set; }

        [StringLength(5), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string PM2_5_24h { get; set; }

        [StringLength(0xff), RoundtripOriginal, ConcurrencyCheck, DataMember]
        public string PrimaryPollutant { get; set; }

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Quality { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(5), DataMember]
        public string SO2_24h { get; set; }

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public DateTime TimePoint { get; set; }

        [DataMember, StringLength(0xff), ConcurrencyCheck, RoundtripOriginal]
        public string Unheathful { get; set; }
    }
}

