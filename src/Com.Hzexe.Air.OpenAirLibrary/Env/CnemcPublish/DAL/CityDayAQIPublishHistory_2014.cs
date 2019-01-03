namespace Env.CnemcPublish.DAL
{
    using System;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class CityDayAQIPublishHistory_2014 : ComplexObject
    {
        private string _aqi;
        private string _area;
        private int _cityCode;
        private string _co_24h;
        private string _latitude;
        private string _longitude;
        private string _measure;
        private string _no2_24h;
        private string _o3_8h_24h;
        private string _pm10_24h;
        private string _pm2_5_24h;
        private string _primaryPollutant;
        private string _quality;
        private string _so2_24h;
        private DateTime _timePoint;
        private string _unheathful;

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

