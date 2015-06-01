namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class City : Entity, IEntityDatahash
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [RoundtripOriginal, DataMember, ConcurrencyCheck, Key]
        public int CityCode { get; set; }

        [DataMember, StringLength(10), ConcurrencyCheck, RoundtripOriginal]
        public string CityJC { get; set; }

        [RoundtripOriginal, StringLength(50), DataMember, ConcurrencyCheck]
        public string CityName { get; set; }

        [Required]
        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public int Id { get; set; }

        [RoundtripOriginal, DataMember, ConcurrencyCheck]
        public int ProvinceId { get; set; }
        [DataMember]
        public int datahash { get; set; }

        public new void RaiseDataMemberChanging(string x)
        {
            base.RaiseDataMemberChanging(x);
            base.RaiseDataMemberChanged(x);
        }
    }
}

