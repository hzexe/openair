namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class WhiteList : Entity
    {
        private int _functionId;
        private int _id;
        private string _ipAddress;
        private string[] _openAccessGenerated;
        private DateTime _timePoint;

        public override object GetIdentity()
        {
            return this._id;
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
        public int FunctionId
        {
            get
            {
                return this._functionId;
            }
            set
            {
                if (this._functionId != value)
                {
                    base.RaiseDataMemberChanging("FunctionId");
                    base.ValidateProperty("FunctionId", value);
                    this._functionId = value;
                    base.RaiseDataMemberChanged("FunctionId");
                }
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, Editable(false, AllowInitialValue=true), Key]
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

        [DataMember, ConcurrencyCheck, StringLength(20), RoundtripOriginal]
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

        [RoundtripOriginal, DataMember, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), Editable(false), ReadOnly(true)]
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

        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public DateTime TimePoint
        {
            get
            {
                return this._timePoint;
            }
            set
            {
                if (this._timePoint != value)
                {
                    base.RaiseDataMemberChanging("TimePoint");
                    base.ValidateProperty("TimePoint", value);
                    this._timePoint = value;
                    base.RaiseDataMemberChanged("TimePoint");
                }
            }
        }
    }
}

