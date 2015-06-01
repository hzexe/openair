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

        public BlackList()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
        }

        [ConcurrencyCheck, RoundtripOriginal, Editable(false, AllowInitialValue=true), DataMember, Key]
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (this._id == value)
                {
                    goto Label_002C;
                }
                base.ValidateProperty("Id", (int) value);
                this._id = value;
                base.RaisePropertyChanged("Id");
            Label_002C:
                return;
            }
        }

        [RoundtripOriginal, DataMember, StringLength(20), ConcurrencyCheck]
        public string IpAddress
        {
            get
            {
                return this._ipAddress;
            }
            set
            {
                if ((this._ipAddress != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("IpAddress");
                base.ValidateProperty("IpAddress", value);
                this._ipAddress = value;
                base.RaiseDataMemberChanged("IpAddress");
            Label_0037:
                return;
            }
        }

        [DataMember, RoundtripOriginal, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), Editable(false), ReadOnly(true)]
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
    }
}

