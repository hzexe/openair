namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class CityAQIModel : Entity
    {
        private int _aqiLevel;
        private string _area;
        private int _cityCode;
        private int _id;
        private string _latitude;
        private string _longitude;
        private string _measure;
        private string[] _openAccessGenerated;
        private string _primaryPollutant;
        private int _provinceId;
        private string _quality;
        private DateTime _timePoint;
        private string _unheathful;

        public override object GetIdentity()
        {
            return this._id;
        }

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public int AqiLevel
        {
            get
            {
                return this._aqiLevel;
            }
            set
            {
                if (this._aqiLevel != value)
                {
                    base.RaiseDataMemberChanging("AqiLevel");
                    base.ValidateProperty("AqiLevel", value);
                    this._aqiLevel = value;
                    base.RaiseDataMemberChanged("AqiLevel");
                }
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, StringLength(0xff), DataMember]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
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

        [RoundtripOriginal, DataMember, Key, ConcurrencyCheck, Editable(false, AllowInitialValue=true)]
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

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(0xff)]
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

        [StringLength(0xff), RoundtripOriginal, ConcurrencyCheck, DataMember]
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

        [DataMember, RoundtripOriginal, StringLength(0xff), ConcurrencyCheck]
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

        [RoundtripOriginal, Editable(false), ReadOnly(true), DataMember, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-")]
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

        [ConcurrencyCheck, StringLength(0xff), RoundtripOriginal, DataMember]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
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

        [RoundtripOriginal, DataMember, ConcurrencyCheck, StringLength(0xff)]
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

        [ConcurrencyCheck, RoundtripOriginal, DataMember, StringLength(0xff)]
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

