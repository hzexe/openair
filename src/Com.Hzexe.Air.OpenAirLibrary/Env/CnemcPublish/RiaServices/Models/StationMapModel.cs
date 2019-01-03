namespace Env.CnemcPublish.RiaServices.Models
{
    using System;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.RiaServices.Models")]
    public sealed class StationMapModel : ComplexObject
    {
        private int _aqi;
        private double _avgHour;
        private string _cityName;
        private int _co;
        private int _id;
        private double _lastHour;
        private string _latitude;
        private string _longitude;
        private int _no2;
        private int _o3_1h;
        private int _o3_8h;
        private int _pm10;
        private int _pm2_5;
        private string _positionName;
        private int _so2;
        private string _stationCode;

        [DataMember]
        public int AQI
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
        public double AvgHour
        {
            get
            {
                return this._avgHour;
            }
            set
            {
                if (this._avgHour != value)
                {
                    base.RaiseDataMemberChanging("AvgHour");
                    base.ValidateProperty("AvgHour", value);
                    this._avgHour = value;
                    base.RaiseDataMemberChanged("AvgHour");
                }
            }
        }

        [DataMember]
        public string CityName
        {
            get
            {
                return this._cityName;
            }
            set
            {
                if (this._cityName != value)
                {
                    base.RaiseDataMemberChanging("CityName");
                    base.ValidateProperty("CityName", value);
                    this._cityName = value;
                    base.RaiseDataMemberChanged("CityName");
                }
            }
        }

        [DataMember]
        public int CO
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

        [DataMember]
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
                    base.RaiseDataMemberChanging("Id");
                    base.ValidateProperty("Id", value);
                    this._id = value;
                    base.RaiseDataMemberChanged("Id");
                }
            }
        }

        [DataMember]
        public double LastHour
        {
            get
            {
                return this._lastHour;
            }
            set
            {
                if (this._lastHour != value)
                {
                    base.RaiseDataMemberChanging("LastHour");
                    base.ValidateProperty("LastHour", value);
                    this._lastHour = value;
                    base.RaiseDataMemberChanged("LastHour");
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
        public int NO2
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

        [DataMember]
        public int O3_1h
        {
            get
            {
                return this._o3_1h;
            }
            set
            {
                if (this._o3_1h != value)
                {
                    base.RaiseDataMemberChanging("O3_1h");
                    base.ValidateProperty("O3_1h", value);
                    this._o3_1h = value;
                    base.RaiseDataMemberChanged("O3_1h");
                }
            }
        }

        [DataMember]
        public int O3_8h
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

        [DataMember]
        public int PM10
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

        [DataMember]
        public int PM2_5
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

        [DataMember]
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

        [DataMember]
        public int SO2
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

        [DataMember]
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
                    base.RaiseDataMemberChanging("StationCode");
                    base.ValidateProperty("StationCode", value);
                    this._stationCode = value;
                    base.RaiseDataMemberChanged("StationCode");
                }
            }
        }
    }
}

