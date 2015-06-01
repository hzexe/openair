namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class CityAQIPublishLive : Entity, IEntityDatahash
    {
        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(0xff)]
        public string AQI { get; set; }

        [DataMember, RoundtripOriginal, StringLength(0xff), ConcurrencyCheck]
        public string Area { get; set; }

        [RoundtripOriginal, DataMember, ConcurrencyCheck,Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CityCode { get; set; }

        [DataMember, RoundtripOriginal, ConcurrencyCheck, StringLength(0xff)]
        public string CO
        { get; set; }

        [ConcurrencyCheck, Editable(false, AllowInitialValue=true), RoundtripOriginal, DataMember]
        public int Id { get; set; }

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Measure { get; set; }

        [RoundtripOriginal, ConcurrencyCheck, StringLength(0xff), DataMember]
        public string NO2 { get; set; }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(0xff), DataMember]
        public string O3 { get; set; }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(0xff)]
        public string PM10 { get; set; }

        [StringLength(0xff), RoundtripOriginal, ConcurrencyCheck, DataMember]
        public string PM2_5 { get; set; }

        [DataMember, StringLength(0xff), ConcurrencyCheck, RoundtripOriginal]
        public string PrimaryPollutant { get; set; }

        [DataMember, StringLength(0xff), ConcurrencyCheck, RoundtripOriginal]
        public string Quality { get; set; }

        [RoundtripOriginal, ConcurrencyCheck, StringLength(0xff), DataMember]
        public string SO2 { get; set; }

        [ConcurrencyCheck, DataMember, RoundtripOriginal]
        public DateTime TimePoint { get; set; }

        [DataMember, ConcurrencyCheck, RoundtripOriginal, StringLength(0xff)]
        public string Unheathful { get; set; }
        [DataMember]
        public int datahash { get; set; }

        public new void RaiseDataMemberChanging(string x)
        {
            base.RaiseDataMemberChanging(x);
            base.RaiseDataMemberChanged(x);
        }
    }
}

