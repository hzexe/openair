namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class AQIChartValue : Entity
    {
        private string _isLimits;
        private string _monValue;
        private string _monValue_24;
        private string[] _openAccessGenerated;
        private string _positionName;
        private string _stationCode;
        private DateTime _timePoint;

        public override object GetIdentity()
        {
            return this._stationCode;
        }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(0xff), DataMember]
        public string IsLimits
        {
            get
            {
                return this._isLimits;
            }
            set
            {
                if (this._isLimits != value)
                {
                    base.RaiseDataMemberChanging("IsLimits");
                    base.ValidateProperty("IsLimits", value);
                    this._isLimits = value;
                    base.RaiseDataMemberChanged("IsLimits");
                }
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(0xff), DataMember]
        public string MonValue
        {
            get
            {
                return this._monValue;
            }
            set
            {
                if (this._monValue != value)
                {
                    base.RaiseDataMemberChanging("MonValue");
                    base.ValidateProperty("MonValue", value);
                    this._monValue = value;
                    base.RaiseDataMemberChanged("MonValue");
                }
            }
        }

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string MonValue_24
        {
            get
            {
                return this._monValue_24;
            }
            set
            {
                if (this._monValue_24 != value)
                {
                    base.RaiseDataMemberChanging("MonValue_24");
                    base.ValidateProperty("MonValue_24", value);
                    this._monValue_24 = value;
                    base.RaiseDataMemberChanged("MonValue_24");
                }
            }
        }

        [Editable(false), RoundtripOriginal, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), DataMember, ReadOnly(true)]
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

        [ConcurrencyCheck, RoundtripOriginal, StringLength(0xff), DataMember]
        public string PositionName
        {
            get
            {
                return this._positionName;
            }
            set
            {
                if (this._positionName != value)
                {
                    base.RaiseDataMemberChanging("PositionName");
                    base.ValidateProperty("PositionName", value);
                    this._positionName = value;
                    base.RaiseDataMemberChanged("PositionName");
                }
            }
        }

        [DataMember, Key, Editable(false, AllowInitialValue=true), RoundtripOriginal, StringLength(0xff), ConcurrencyCheck]
        public string StationCode
        {
            get
            {
                return this._stationCode;
            }
            set
            {
                if (this._stationCode != value)
                {
                    base.ValidateProperty("StationCode", value);
                    this._stationCode = value;
                    base.RaisePropertyChanged("StationCode");
                }
            }
        }

        [ConcurrencyCheck, DataMember, RoundtripOriginal]
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

