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

        public CityAQIModel()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public int AqiLevel
        {
            get
            {
                return this._aqiLevel;
            }
            set
            {
                if (this._aqiLevel == value)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("AqiLevel");
                base.ValidateProperty("AqiLevel", (int) value);
                this._aqiLevel = value;
                base.RaiseDataMemberChanged("AqiLevel");
            Label_0037:
                return;
            }
        }

        [ConcurrencyCheck, StringLength(0xff), DataMember, RoundtripOriginal]
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

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public int CityCode
        {
            get
            {
                return this._cityCode;
            }
            set
            {
                if (this._cityCode == value)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("CityCode");
                base.ValidateProperty("CityCode", (int) value);
                this._cityCode = value;
                base.RaiseDataMemberChanged("CityCode");
            Label_0037:
                return;
            }
        }

        [ConcurrencyCheck, DataMember, Editable(false, AllowInitialValue=true), Key, RoundtripOriginal]
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (this._id == value)
                {
                    goto Label_002C;
                }
                base.ValidateProperty("Id", (int) value);
                this._id = value;
                base.RaisePropertyChanged("Id");
            Label_002C:
                return;
            }
        }

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
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

        [StringLength(0xff), DataMember, RoundtripOriginal, ConcurrencyCheck]
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

        [RoundtripOriginal, StringLength(0xff), ConcurrencyCheck, DataMember]
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

        [DataMember, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), Editable(false), ReadOnly(true), RoundtripOriginal]
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

        [RoundtripOriginal, DataMember, StringLength(0xff), ConcurrencyCheck]
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

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public int ProvinceId
        {
            get
            {
                return this._provinceId;
            }
            set
            {
                if (this._provinceId == value)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("ProvinceId");
                base.ValidateProperty("ProvinceId", (int) value);
                this._provinceId = value;
                base.RaiseDataMemberChanged("ProvinceId");
            Label_0037:
                return;
            }
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember, StringLength(0xff)]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
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

        [ConcurrencyCheck, StringLength(0xff), RoundtripOriginal, DataMember]
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

