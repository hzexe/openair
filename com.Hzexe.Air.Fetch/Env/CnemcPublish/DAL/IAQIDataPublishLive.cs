namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class IAQIDataPublishLive : Entity
    {
        private string _area;
        private string _ico;
        private string _ino2;
        private string _io3;
        private string _io3_8h;
        private string _ipm10;
        private string _ipm2_5;
        private string _iso2;
        private string _latitude;
        private string _longitude;
        private string[] _openAccessGenerated;
        private string _positionName;
        private string _stationCode;
        private DateTime _timePoint;

        public IAQIDataPublishLive()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return this._stationCode;
        }

        [RoundtripOriginal, DataMember, StringLength(10), ConcurrencyCheck]
        public string Area
        {
            get
            {
                return this._area;
            }
            set
            {
                if ((this._area != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Area");
                base.ValidateProperty("Area", value);
                this._area = value;
                base.RaiseDataMemberChanged("Area");
            Label_0037:
                return;
            }
        }

        [DataMember, StringLength(8), ConcurrencyCheck, RoundtripOriginal]
        public string ICO
        {
            get
            {
                return this._ico;
            }
            set
            {
                if ((this._ico != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("ICO");
                base.ValidateProperty("ICO", value);
                this._ico = value;
                base.RaiseDataMemberChanged("ICO");
            Label_0037:
                return;
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember, StringLength(5)]
        public string INO2
        {
            get
            {
                return this._ino2;
            }
            set
            {
                if ((this._ino2 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("INO2");
                base.ValidateProperty("INO2", value);
                this._ino2 = value;
                base.RaiseDataMemberChanged("INO2");
            Label_0037:
                return;
            }
        }

        [RoundtripOriginal, StringLength(5), ConcurrencyCheck, DataMember]
        public string IO3
        {
            get
            {
                return this._io3;
            }
            set
            {
                if ((this._io3 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("IO3");
                base.ValidateProperty("IO3", value);
                this._io3 = value;
                base.RaiseDataMemberChanged("IO3");
            Label_0037:
                return;
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, StringLength(5)]
        public string IO3_8h
        {
            get
            {
                return this._io3_8h;
            }
            set
            {
                if ((this._io3_8h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("IO3_8h");
                base.ValidateProperty("IO3_8h", value);
                this._io3_8h = value;
                base.RaiseDataMemberChanged("IO3_8h");
            Label_0037:
                return;
            }
        }

        [DataMember, StringLength(5), ConcurrencyCheck, RoundtripOriginal]
        public string IPM10
        {
            get
            {
                return this._ipm10;
            }
            set
            {
                if ((this._ipm10 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("IPM10");
                base.ValidateProperty("IPM10", value);
                this._ipm10 = value;
                base.RaiseDataMemberChanged("IPM10");
            Label_0037:
                return;
            }
        }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(5)]
        public string IPM2_5
        {
            get
            {
                return this._ipm2_5;
            }
            set
            {
                if ((this._ipm2_5 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("IPM2_5");
                base.ValidateProperty("IPM2_5", value);
                this._ipm2_5 = value;
                base.RaiseDataMemberChanged("IPM2_5");
            Label_0037:
                return;
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(5), DataMember]
        public string ISO2
        {
            get
            {
                return this._iso2;
            }
            set
            {
                if ((this._iso2 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("ISO2");
                base.ValidateProperty("ISO2", value);
                this._iso2 = value;
                base.RaiseDataMemberChanged("ISO2");
            Label_0037:
                return;
            }
        }

        [StringLength(15), ConcurrencyCheck, RoundtripOriginal, DataMember]
        public string Latitude
        {
            get
            {
                return this._latitude;
            }
            set
            {
                if ((this._latitude != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Latitude");
                base.ValidateProperty("Latitude", value);
                this._latitude = value;
                base.RaiseDataMemberChanged("Latitude");
            Label_0037:
                return;
            }
        }

        [RoundtripOriginal, StringLength(15), ConcurrencyCheck, DataMember]
        public string Longitude
        {
            get
            {
                return this._longitude;
            }
            set
            {
                if ((this._longitude != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Longitude");
                base.ValidateProperty("Longitude", value);
                this._longitude = value;
                base.RaiseDataMemberChanged("Longitude");
            Label_0037:
                return;
            }
        }

        [Editable(false), RoundtripOriginal, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), ReadOnly(true), DataMember]
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

        [DataMember, StringLength(40), ConcurrencyCheck, RoundtripOriginal]
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

        [StringLength(5), Editable(false, AllowInitialValue=true), ConcurrencyCheck, Key, RoundtripOriginal, DataMember]
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

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public DateTime TimePoint
        {
            get
            {
                return this._timePoint;
            }
            set
            {
                if ((this._timePoint != value) == null)
                {
                    goto Label_003C;
                }
                base.RaiseDataMemberChanging("TimePoint");
                base.ValidateProperty("TimePoint", (DateTime) value);
                this._timePoint = value;
                base.RaiseDataMemberChanged("TimePoint");
            Label_003C:
                return;
            }
        }
    }
}

