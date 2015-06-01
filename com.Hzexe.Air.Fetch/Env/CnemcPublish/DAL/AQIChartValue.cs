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

        [DataMember, RoundtripOriginal, StringLength(0xff), ConcurrencyCheck]
        public string IsLimits
        {
            get
            {
                return this._isLimits;
            }
            set
            {
                if ((this._isLimits != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("IsLimits");
                base.ValidateProperty("IsLimits", value);
                this._isLimits = value;
                base.RaiseDataMemberChanged("IsLimits");
            Label_0037:
                return;
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
                if ((this._monValue != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("MonValue");
                base.ValidateProperty("MonValue", value);
                this._monValue = value;
                base.RaiseDataMemberChanged("MonValue");
            Label_0037:
                return;
            }
        }

        [RoundtripOriginal, StringLength(0xff), ConcurrencyCheck, DataMember]
        public string MonValue_24
        {
            get
            {
                return this._monValue_24;
            }
            set
            {
                if ((this._monValue_24 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("MonValue_24");
                base.ValidateProperty("MonValue_24", value);
                this._monValue_24 = value;
                base.RaiseDataMemberChanged("MonValue_24");
            Label_0037:
                return;
            }
        }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [RoundtripOriginal, Editable(false), Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), DataMember, ReadOnly(true)]
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

        [RoundtripOriginal, ConcurrencyCheck, StringLength(0xff), DataMember]
        public string PositionName
        {
            get
            {
                return this._positionName;
            }
            set
            {
                if ((this._positionName != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("PositionName");
                base.ValidateProperty("PositionName", value);
                this._positionName = value;
                base.RaiseDataMemberChanged("PositionName");
            Label_0037:
                return;
            }
        }

        [Key, StringLength(0xff), DataMember, RoundtripOriginal, ConcurrencyCheck]
        public string StationCode
        {
            get
            {
                return this._stationCode;
            }
            set
            {
                if ((this._stationCode != value) == null)
                {
                    goto Label_002C;
                }
                base.ValidateProperty("StationCode", value);
                this._stationCode = value;
                base.RaisePropertyChanged("StationCode");
            Label_002C:
                return;
            }
        }

        [DataMember, ConcurrencyCheck, RoundtripOriginal]
        public DateTime TimePoint{get;set;}
    
    }
}

