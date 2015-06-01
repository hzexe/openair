namespace Env.CnemcPublish.DAL
{
    using System;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class AQIDataPublishHistory_2014 : ComplexObject
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

        public AQIDataPublishHistory_2014()
        {
           
            return;
        }

        [DataMember]
        public string AQI
        {
            get
            {
                return this._aqi;
            }
            set
            {
                if ((this._aqi != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("AQI");
                base.ValidateProperty("AQI", value);
                this._aqi = value;
                base.RaiseDataMemberChanged("AQI");
            Label_0037:
                return;
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

        [DataMember]
        public string CO
        {
            get
            {
                return this._co;
            }
            set
            {
                if ((this._co != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("CO");
                base.ValidateProperty("CO", value);
                this._co = value;
                base.RaiseDataMemberChanged("CO");
            Label_0037:
                return;
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
                if ((this._co_24h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("CO_24h");
                base.ValidateProperty("CO_24h", value);
                this._co_24h = value;
                base.RaiseDataMemberChanged("CO_24h");
            Label_0037:
                return;
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

        [DataMember]
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

        [DataMember]
        public string Measure
        {
            get
            {
                return this._measure;
            }
            set
            {
                if ((this._measure != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Measure");
                base.ValidateProperty("Measure", value);
                this._measure = value;
                base.RaiseDataMemberChanged("Measure");
            Label_0037:
                return;
            }
        }

        [DataMember]
        public string NO2
        {
            get
            {
                return this._no2;
            }
            set
            {
                if ((this._no2 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("NO2");
                base.ValidateProperty("NO2", value);
                this._no2 = value;
                base.RaiseDataMemberChanged("NO2");
            Label_0037:
                return;
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
                if ((this._no2_24h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("NO2_24h");
                base.ValidateProperty("NO2_24h", value);
                this._no2_24h = value;
                base.RaiseDataMemberChanged("NO2_24h");
            Label_0037:
                return;
            }
        }

        [DataMember]
        public string O3
        {
            get
            {
                return this._o3;
            }
            set
            {
                if ((this._o3 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("O3");
                base.ValidateProperty("O3", value);
                this._o3 = value;
                base.RaiseDataMemberChanged("O3");
            Label_0037:
                return;
            }
        }

        [DataMember]
        public string O3_24h
        {
            get
            {
                return this._o3_24h;
            }
            set
            {
                if ((this._o3_24h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("O3_24h");
                base.ValidateProperty("O3_24h", value);
                this._o3_24h = value;
                base.RaiseDataMemberChanged("O3_24h");
            Label_0037:
                return;
            }
        }

        [DataMember]
        public string O3_8h
        {
            get
            {
                return this._o3_8h;
            }
            set
            {
                if ((this._o3_8h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("O3_8h");
                base.ValidateProperty("O3_8h", value);
                this._o3_8h = value;
                base.RaiseDataMemberChanged("O3_8h");
            Label_0037:
                return;
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
                if ((this._o3_8h_24h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("O3_8h_24h");
                base.ValidateProperty("O3_8h_24h", value);
                this._o3_8h_24h = value;
                base.RaiseDataMemberChanged("O3_8h_24h");
            Label_0037:
                return;
            }
        }

        [DataMember]
        public string PM10
        {
            get
            {
                return this._pm10;
            }
            set
            {
                if ((this._pm10 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("PM10");
                base.ValidateProperty("PM10", value);
                this._pm10 = value;
                base.RaiseDataMemberChanged("PM10");
            Label_0037:
                return;
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
                if ((this._pm10_24h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("PM10_24h");
                base.ValidateProperty("PM10_24h", value);
                this._pm10_24h = value;
                base.RaiseDataMemberChanged("PM10_24h");
            Label_0037:
                return;
            }
        }

        [DataMember]
        public string PM2_5
        {
            get
            {
                return this._pm2_5;
            }
            set
            {
                if ((this._pm2_5 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("PM2_5");
                base.ValidateProperty("PM2_5", value);
                this._pm2_5 = value;
                base.RaiseDataMemberChanged("PM2_5");
            Label_0037:
                return;
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
                if ((this._pm2_5_24h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("PM2_5_24h");
                base.ValidateProperty("PM2_5_24h", value);
                this._pm2_5_24h = value;
                base.RaiseDataMemberChanged("PM2_5_24h");
            Label_0037:
                return;
            }
        }

        [DataMember]
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

        [DataMember]
        public string PrimaryPollutant
        {
            get
            {
                return this._primaryPollutant;
            }
            set
            {
                if ((this._primaryPollutant != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("PrimaryPollutant");
                base.ValidateProperty("PrimaryPollutant", value);
                this._primaryPollutant = value;
                base.RaiseDataMemberChanged("PrimaryPollutant");
            Label_0037:
                return;
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
                if ((this._quality != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Quality");
                base.ValidateProperty("Quality", value);
                this._quality = value;
                base.RaiseDataMemberChanged("Quality");
            Label_0037:
                return;
            }
        }

        [DataMember]
        public string SO2
        {
            get
            {
                return this._so2;
            }
            set
            {
                if ((this._so2 != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("SO2");
                base.ValidateProperty("SO2", value);
                this._so2 = value;
                base.RaiseDataMemberChanged("SO2");
            Label_0037:
                return;
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
                if ((this._so2_24h != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("SO2_24h");
                base.ValidateProperty("SO2_24h", value);
                this._so2_24h = value;
                base.RaiseDataMemberChanged("SO2_24h");
            Label_0037:
                return;
            }
        }

        [DataMember]
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
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("StationCode");
                base.ValidateProperty("StationCode", value);
                this._stationCode = value;
                base.RaiseDataMemberChanged("StationCode");
            Label_0037:
                return;
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

        [DataMember]
        public string Unheathful
        {
            get
            {
                return this._unheathful;
            }
            set
            {
                if ((this._unheathful != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Unheathful");
                base.ValidateProperty("Unheathful", value);
                this._unheathful = value;
                base.RaiseDataMemberChanged("Unheathful");
            Label_0037:
                return;
            }
        }
    }
}

