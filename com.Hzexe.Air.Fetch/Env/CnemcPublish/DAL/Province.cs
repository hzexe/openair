namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class Province : Entity, IEntityDatahash
    {

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember, Key]
        public int Id { get; set; }

        [DataMember]
        public int? ProvinceCode { get; set; }

        [StringLength(50), DataMember]
        public string ProvinceJC { get; set; }

        [StringLength(50), DataMember]
       
        public string ProvinceName { get; set; }
        [DataMember]
        public int datahash { get; set; }

        public new void RaiseDataMemberChanging(string x)
        {
            base.RaiseDataMemberChanging(x);
            base.RaiseDataMemberChanged(x);
        }

    }



}

