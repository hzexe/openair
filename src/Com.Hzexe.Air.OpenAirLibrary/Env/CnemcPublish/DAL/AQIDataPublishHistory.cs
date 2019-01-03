namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class AQIDataPublishHistory : Entity
    {
        private string _aqi;
        private string _area;
        private string _co;
        private string _co_24h;
        private string _latitude;
        private string _longitude;
        private string _measure;
        private string _no2;
        private string _no2_24h;
        private string _o3;
        private string _o3_24h;
        private string _o3_8h;
        private string _o3_8h_24h;
        private string[] _openAccessGenerated;
        private string _pm10;
        private string _pm10_24h;
        private string _pm2_5;
        private string _pm2_5_24h;
        private string _positionName;
        private string _primaryPollutant;
        private string _quality;
        private string _so2;
        private string _so2_24h;
        private string _stationCode;
        private DateTime _timePoint;
        private string _unheathful;

        public override object GetIdentity()
        {
            if (this._stationCode == null)
            {
                return null;
            }
            return EntityKey.Create<string, DateTime>(this._stationCode, this._timePoint);
        }

        [StringLength(5), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string AQI
        {
            get
            {
                return this._aqi;
            }
            set
            {
                if (this._aqi != value)
                {
                    base.RaiseDataMemberChanging("AQI");
                    base.ValidateProperty("AQI", value);
                    this._aqi = value;
                    base.RaiseDataMemberChanged("AQI");
                }
            }
        }

        [ConcurrencyCheck, StringLength(10), DataMember, RoundtripOriginal]
        public string Area
        {
            get
            {
                return this._area;
            }
            set
            {
                if (this._area != value)
                {
                    base.RaiseDataMemberChanging("Area");
                    base.ValidateProperty("Area", value);
                    this._area = value;
                    base.RaiseDataMemberChanged("Area");
                }
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember, StringLength(8)]
        public string CO
        {
            get
            {
                return this._co;
            }
            set
            {
                if (this._co != value)
                {
                    base.RaiseDataMemberChanging("CO");
                    base.ValidateProperty("CO", value);
                    this._co = value;
                    base.RaiseDataMemberChanged("CO");
                }
            }
        }

        [StringLength(8), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string CO_24h
        {
            get
            {
                return this._co_24h;
            }
            set
            {
                if (this._co_24h != value)
                {
                    base.RaiseDataMemberChanging("CO_24h");
                    base.ValidateProperty("CO_24h", value);
                    this._co_24h = value;
                    base.RaiseDataMemberChanged("CO_24h");
                }
            }
        }

        [StringLength(15), DataMember, ConcurrencyCheck, RoundtripOriginal]
        public string Latitude
        {
            get
            {
                return this._latitude;
            }
            set
            {
                if (this._latitude != value)
                {
                    base.RaiseDataMemberChanging("Latitude");
                    base.ValidateProperty("Latitude", value);
                    this._latitude = value;
                    base.RaiseDataMemberChanged("Latitude");
                }
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, StringLength(15)]
        public string Longitude
        {
            get
            {
                return this._longitude;
            }
            set
            {
                if (this._longitude != value)
                {
                    base.RaiseDataMemberChanging("Longitude");
                    base.ValidateProperty("Longitude", value);
                    this._longitude = value;
                    base.RaiseDataMemberChanged("Longitude");
                }
            }
        }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(0xff)]
        public string Measure
        {
            get
            {
                return this._measure;
            }
            set
            {
                if (this._measure != value)
                {
                    base.RaiseDataMemberChanging("Measure");
                    base.ValidateProperty("Measure", value);
                    this._measure = value;
                    base.RaiseDataMemberChanged("Measure");
                }
            }
        }

        [DataMember, ConcurrencyCheck, RoundtripOriginal, StringLength(5)]
        public string NO2
        {
            get
            {
                return this._no2;
            }
            set
            {
                if (this._no2 != value)
                {
                    base.RaiseDataMemberChanging("NO2");
                    base.ValidateProperty("NO2", value);
                    this._no2 = value;
                    base.RaiseDataMemberChanged("NO2");
                }
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, StringLength(5)]
        public string NO2_24h
        {
            get
            {
                return this._no2_24h;
            }
            set
            {
                if (this._no2_24h != value)
                {
                    base.RaiseDataMemberChanging("NO2_24h");
                    base.ValidateProperty("NO2_24h", value);
                    this._no2_24h = value;
                    base.RaiseDataMemberChanged("NO2_24h");
                }
            }
        }

        [DataMember, StringLength(5), RoundtripOriginal, ConcurrencyCheck]
        public string O3
        {
            get
            {
                return this._o3;
            }
            set
            {
                if (this._o3 != value)
                {
                    base.RaiseDataMemberChanging("O3");
                    base.ValidateProperty("O3", value);
                    this._o3 = value;
                    base.RaiseDataMemberChanged("O3");
                }
            }
        }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(5)]
        public string O3_24h
        {
            get
            {
                return this._o3_24h;
            }
            set
            {
                if (this._o3_24h != value)
                {
                    base.RaiseDataMemberChanging("O3_24h");
                    base.ValidateProperty("O3_24h", value);
                    this._o3_24h = value;
                    base.RaiseDataMemberChanged("O3_24h");
                }
            }
        }

        [RoundtripOriginal, DataMember, ConcurrencyCheck, StringLength(5)]
        public string O3_8h
        {
            get
            {
                return this._o3_8h;
            }
            set
            {
                if (this._o3_8h != value)
                {
                    base.RaiseDataMemberChanging("O3_8h");
                    base.ValidateProperty("O3_8h", value);
                    this._o3_8h = value;
                    base.RaiseDataMemberChanged("O3_8h");
                }
            }
        }

        [RoundtripOriginal, StringLength(5), ConcurrencyCheck, DataMember]
        public string O3_8h_24h
        {
            get
            {
                return this._o3_8h_24h;
            }
            set
            {
                if (this._o3_8h_24h != value)
                {
                    base.RaiseDataMemberChanging("O3_8h_24h");
                    base.ValidateProperty("O3_8h_24h", value);
                    this._o3_8h_24h = value;
                    base.RaiseDataMemberChanged("O3_8h_24h");
                }
            }
        }

        [RoundtripOriginal, ReadOnly(true), DataMember, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), Editable(false)]
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

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(5)]
        public string PM10
        {
            get
            {
                return this._pm10;
            }
            set
            {
                if (this._pm10 != value)
                {
                    base.RaiseDataMemberChanging("PM10");
                    base.ValidateProperty("PM10", value);
                    this._pm10 = value;
                    base.RaiseDataMemberChanged("PM10");
                }
            }
        }

        [StringLength(5), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string PM10_24h
        {
            get
            {
                return this._pm10_24h;
            }
            set
            {
                if (this._pm10_24h != value)
                {
                    base.RaiseDataMemberChanging("PM10_24h");
                    base.ValidateProperty("PM10_24h", value);
                    this._pm10_24h = value;
                    base.RaiseDataMemberChanged("PM10_24h");
                }
            }
        }

        [StringLength(5), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string PM2_5
        {
            get
            {
                return this._pm2_5;
            }
            set
            {
                if (this._pm2_5 != value)
                {
                    base.RaiseDataMemberChanging("PM2_5");
                    base.ValidateProperty("PM2_5", value);
                    this._pm2_5 = value;
                    base.RaiseDataMemberChanged("PM2_5");
                }
            }
        }

        [StringLength(5), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string PM2_5_24h
        {
            get
            {
                return this._pm2_5_24h;
            }
            set
            {
                if (this._pm2_5_24h != value)
                {
                    base.RaiseDataMemberChanging("PM2_5_24h");
                    base.ValidateProperty("PM2_5_24h", value);
                    this._pm2_5_24h = value;
                    base.RaiseDataMemberChanged("PM2_5_24h");
                }
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, StringLength(40)]
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

        [DataMember, RoundtripOriginal, ConcurrencyCheck, StringLength(0xff)]
        public string PrimaryPollutant
        {
            get
            {
                return this._primaryPollutant;
            }
            set
            {
                if (this._primaryPollutant != value)
                {
                    base.RaiseDataMemberChanging("PrimaryPollutant");
                    base.ValidateProperty("PrimaryPollutant", value);
                    this._primaryPollutant = value;
                    base.RaiseDataMemberChanged("PrimaryPollutant");
                }
            }
        }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(0xff)]
        public string Quality
        {
            get
            {
                return this._quality;
            }
            set
            {
                if (this._quality != value)
                {
                    base.RaiseDataMemberChanging("Quality");
                    base.ValidateProperty("Quality", value);
                    this._quality = value;
                    base.RaiseDataMemberChanged("Quality");
                }
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember, StringLength(5)]
        public string SO2
        {
            get
            {
                return this._so2;
            }
            set
            {
                if (this._so2 != value)
                {
                    base.RaiseDataMemberChanging("SO2");
                    base.ValidateProperty("SO2", value);
                    this._so2 = value;
                    base.RaiseDataMemberChanged("SO2");
                }
            }
        }

        [DataMember, RoundtripOriginal, StringLength(5), ConcurrencyCheck]
        public string SO2_24h
        {
            get
            {
                return this._so2_24h;
            }
            set
            {
                if (this._so2_24h != value)
                {
                    base.RaiseDataMemberChanging("SO2_24h");
                    base.ValidateProperty("SO2_24h", value);
                    this._so2_24h = value;
                    base.RaiseDataMemberChanged("SO2_24h");
                }
            }
        }

        [DataMember, RoundtripOriginal, StringLength(5), Key, Editable(false, AllowInitialValue=true), ConcurrencyCheck]
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

        [DataMember, Key, Editable(false, AllowInitialValue=true), RoundtripOriginal, ConcurrencyCheck]
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
                    base.ValidateProperty("TimePoint", value);
                    this._timePoint = value;
                    base.RaisePropertyChanged("TimePoint");
                }
            }
        }

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Unheathful
        {
            get
            {
                return this._unheathful;
            }
            set
            {
                if (this._unheathful != value)
                {
                    base.RaiseDataMemberChanging("Unheathful");
                    base.ValidateProperty("Unheathful", value);
                    this._unheathful = value;
                    base.RaiseDataMemberChanged("Unheathful");
                }
            }
        }
    }
}

