namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class BlackList : Entity
    {
        private int _id;
        private string _ipAddress;
        private string[] _openAccessGenerated;

        public override object GetIdentity()
        {
            return this._id;
        }

        [Key, Editable(false, AllowInitialValue=true), RoundtripOriginal, ConcurrencyCheck, DataMember]
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (this._id != value)
                {
                    base.ValidateProperty("Id", value);
                    this._id = value;
                    base.RaisePropertyChanged("Id");
                }
            }
        }

        [DataMember, RoundtripOriginal, StringLength(20), ConcurrencyCheck]
        public string IpAddress
        {
            get
            {
                return this._ipAddress;
            }
            set
            {
                if (this._ipAddress != value)
                {
                    base.RaiseDataMemberChanging("IpAddress");
                    base.ValidateProperty("IpAddress", value);
                    this._ipAddress = value;
                    base.RaiseDataMemberChanged("IpAddress");
                }
            }
        }

        [ReadOnly(true), Editable(false), DataMember, RoundtripOriginal, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-")]
        public string[] OpenAccessGenerated
        {
            get
            {
                return this._openAccessGenerated;
            }
            set
            {
                if (this._openAccessGenerated != value)
                {
                    base.ValidateProperty("OpenAccessGenerated", value);
                    this._openAccessGenerated = value;
                    base.RaisePropertyChanged("OpenAccessGenerated");
                }
            }
        }
    }
}

