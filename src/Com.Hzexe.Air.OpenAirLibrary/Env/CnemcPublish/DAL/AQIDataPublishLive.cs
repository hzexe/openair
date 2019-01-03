namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class AQIDataPublishLive : Entity
    {
        private string _aqi;
        private string _area;
        private int _cityCode;
        private string _co;
        private string _co_24h;
        private bool? _isPublish;
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
        private int _orderId;
        private string _pm10;
        private string _pm10_24h;
        private string _pm2_5;
        private string _pm2_5_24h;
        private string _positionName;
        private string _primaryPollutant;
        private int _provinceId;
        private string _quality;
        private string _so2;
        private string _so2_24h;
        private string _stationCode;
        private DateTime _timePoint;
        private string _unheathful;

        public override object GetIdentity()
        {
            return this._stationCode;
        }

        [RoundtripOriginal, StringLength(5), ConcurrencyCheck, DataMember]
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

        [ConcurrencyCheck, RoundtripOriginal, StringLength(10), DataMember]
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

        [DataMember]
        public int CityCode
        {
            get
            {
                return this._cityCode;
            }
            set
            {
                if (this._cityCode != value)
                {
                    base.RaiseDataMemberChanging("CityCode");
                    base.ValidateProperty("CityCode", value);
                    this._cityCode = value;
                    base.RaiseDataMemberChanged("CityCode");
                }
            }
        }

        [ConcurrencyCheck, StringLength(8), DataMember, RoundtripOriginal]
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

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public bool? IsPublish
        {
            get
            {
                return this._isPublish;
            }
            set
            {
                if (this._isPublish != value)
                {
                    base.RaiseDataMemberChanging("IsPublish");
                    base.ValidateProperty("IsPublish", value);
                    this._isPublish = value;
                    base.RaiseDataMemberChanged("IsPublish");
                }
            }
        }

        [StringLength(15), ConcurrencyCheck, DataMember, RoundtripOriginal]
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

        [RoundtripOriginal, StringLength(15), ConcurrencyCheck, DataMember]
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

        [RoundtripOriginal, DataMember, StringLength(0xff), ConcurrencyCheck]
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

        [DataMember, StringLength(5), ConcurrencyCheck, RoundtripOriginal]
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

        [RoundtripOriginal, StringLength(5), ConcurrencyCheck, DataMember]
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

        [DataMember, StringLength(5), ConcurrencyCheck, RoundtripOriginal]
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

        [ConcurrencyCheck, RoundtripOriginal, StringLength(5), DataMember]
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

        [ConcurrencyCheck, RoundtripOriginal, StringLength(5), DataMember]
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

        [StringLength(5), DataMember, RoundtripOriginal, ConcurrencyCheck]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
        public int OrderId
        {
            get
            {
                return this._orderId;
            }
            set
            {
                if (this._orderId != value)
                {
                    base.RaiseDataMemberChanging("OrderId");
                    base.ValidateProperty("OrderId", value);
                    this._orderId = value;
                    base.RaiseDataMemberChanged("OrderId");
                }
            }
        }

        [StringLength(5), RoundtripOriginal, ConcurrencyCheck, DataMember]
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

        [StringLength(5), DataMember, RoundtripOriginal, ConcurrencyCheck]
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

        [StringLength(5), DataMember, RoundtripOriginal, ConcurrencyCheck]
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

        [ConcurrencyCheck, StringLength(0xff), DataMember, RoundtripOriginal]
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

        [DataMember]
        public int ProvinceId
        {
            get
            {
                return this._provinceId;
            }
            set
            {
                if (this._provinceId != value)
                {
                    base.RaiseDataMemberChanging("ProvinceId");
                    base.ValidateProperty("ProvinceId", value);
                    this._provinceId = value;
                    base.RaiseDataMemberChanged("ProvinceId");
                }
            }
        }

        [DataMember, ConcurrencyCheck, RoundtripOriginal, StringLength(0xff)]
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

        [RoundtripOriginal, DataMember, ConcurrencyCheck, StringLength(5)]
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

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(5)]
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

        [StringLength(5), DataMember, ConcurrencyCheck, Editable(false, AllowInitialValue=true), Key, RoundtripOriginal]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
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

        [DataMember, RoundtripOriginal, StringLength(0xff), ConcurrencyCheck]
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

