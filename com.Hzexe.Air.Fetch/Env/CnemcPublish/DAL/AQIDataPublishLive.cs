namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class AQIDataPublishLive : Entity, IEntityDatahash
    {

        [RoundtripOriginal, StringLength(5), ConcurrencyCheck, DataMember]
        public string AQI { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(10), DataMember]
        public string Area { get; set; }

        [DataMember]
        public int CityCode { get; set; }

        [StringLength(8), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string CO { get; set; }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(8)]
        public string CO_24h { get; set; }

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public bool? IsPublish { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(15), DataMember]
        public string Latitude { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(15), DataMember]
        public string Longitude { get; set; }

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Measure { get; set; }

        [RoundtripOriginal, DataMember, StringLength(5), ConcurrencyCheck]
        public string NO2 { get; set; }

        [ConcurrencyCheck, StringLength(5), RoundtripOriginal, DataMember]
        public string NO2_24h { get; set; }

        [StringLength(5), DataMember, ConcurrencyCheck, RoundtripOriginal]
        public string O3 { get; set; }

        [DataMember, ConcurrencyCheck, RoundtripOriginal, StringLength(5)]
        public string O3_24h { get; set; }

        [RoundtripOriginal, ConcurrencyCheck, StringLength(5), DataMember]
        public string O3_8h { get; set; }

        [StringLength(5), DataMember, ConcurrencyCheck, RoundtripOriginal]
        public string O3_8h_24h { get; set; }

       

        [RoundtripOriginal, DataMember, ConcurrencyCheck]
        public int OrderId { get; set; }
        [StringLength(5), ConcurrencyCheck, RoundtripOriginal, DataMember]
        public string PM10 { get; set; }

        [StringLength(5), DataMember, RoundtripOriginal, ConcurrencyCheck]
        public string PM10_24h { get; set; }

        [StringLength(5), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string PM2_5 { get; set; }

        [RoundtripOriginal, ConcurrencyCheck, StringLength(5), DataMember]
        public string PM2_5_24h { get; set; }

        [DataMember, StringLength(40), ConcurrencyCheck, RoundtripOriginal]
        public string PositionName { get; set; }

        [ConcurrencyCheck, StringLength(0xff), RoundtripOriginal, DataMember]
        public string PrimaryPollutant { get; set; }

        [DataMember]
        public int ProvinceId { get; set; }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(0xff)]
        public string Quality { get; set; }

        [StringLength(5), DataMember, ConcurrencyCheck, RoundtripOriginal]
        public string SO2 { get; set; }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, StringLength(5)]
        public string SO2_24h { get; set; }

        [Key,StringLength(5), DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string StationCode { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public DateTime TimePoint { get; set; }

        [RoundtripOriginal, StringLength(0xff), ConcurrencyCheck, DataMember]
        public string Unheathful { get; set; }
       [DataMember]
        public int datahash { get; set; }


        public new void  RaiseDataMemberChanging(string x)
        {
            base.RaiseDataMemberChanging(x);
            base.RaiseDataMemberChanged(x);
        }



       
    }
}

