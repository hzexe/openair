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

        public SystemConfig()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return this._key;
        }

        [Key, StringLength(50), ConcurrencyCheck, DataMember, Editable(false, AllowInitialValue=true), RoundtripOriginal]
        public string Key
        {
            get
            {
                return this._key;
            }
            set
            {
                if ((this._key != value) == null)
                {
                    goto Label_002C;
                }
                base.ValidateProperty("Key", value);
                this._key = value;
                base.RaisePropertyChanged("Key");
            Label_002C:
                return;
            }
        }

        [Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), DataMember, RoundtripOriginal, Editable(false), ReadOnly(true)]
        public string[] OpenAccessGenerated
        {
            get
            {
                return this._openAccessGenerated;
            }
            set
            {
                if (this._openAccessGenerated == value)
                {
                    goto Label_0027;
                }
                base.ValidateProperty("OpenAccessGenerated", value);
                this._openAccessGenerated = value;
                base.RaisePropertyChanged("OpenAccessGenerated");
            Label_0027:
                return;
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if ((this._value != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Value");
                base.ValidateProperty("Value", value);
                this._value = value;
                base.RaiseDataMemberChanged("Value");
            Label_0037:
                return;
            }
        }
    }
}

