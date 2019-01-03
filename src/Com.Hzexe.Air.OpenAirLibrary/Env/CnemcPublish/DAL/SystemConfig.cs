namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class SystemConfig : Entity
    {
        private string _key;
        private string[] _openAccessGenerated;
        private string _value;

        public override object GetIdentity()
        {
            return this._key;
        }

        [StringLength(50), ConcurrencyCheck, DataMember, Editable(false, AllowInitialValue=true), Key, RoundtripOriginal]
        public string Key
        {
            get
            {
                return this._key;
            }
            set
            {
                if (this._key != value)
                {
                    base.ValidateProperty("Key", value);
                    this._key = value;
                    base.RaisePropertyChanged("Key");
                }
            }
        }

        [Editable(false), RoundtripOriginal, DataMember, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), ReadOnly(true)]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (this._value != value)
                {
                    base.RaiseDataMemberChanging("Value");
                    base.ValidateProperty("Value", value);
                    this._value = value;
                    base.RaiseDataMemberChanged("Value");
                }
            }
        }
    }
}

