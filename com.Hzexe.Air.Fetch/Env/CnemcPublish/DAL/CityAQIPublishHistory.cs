namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class CityAQIPublishHistory : Entity
    {
        private string _aqi;
        private string _area;
        private int _cityCode;
        private string _co;
        private int _id;
        private string _measure;
        private string _no2;
        private string _o3;
        private string[] _openAccessGenerated;
        private string _pm10;
        private string _pm2_5;
        private string _primaryPollutant;
        private string _quality;
        private string _so2;
        private DateTime _timePoint;
        private string _unheathful;

        public CityAQIPublishHistory()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
        }

        [DataMember, RoundtripOriginal, StringLength(5)]
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

        [StringLength(50), DataMember, RoundtripOriginal]
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

        [DataMember, RoundtripOriginal]
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

        [DataMember, RoundtripOriginal, StringLength(8)]
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

        [DataMember, Key, RoundtripOriginal, Editable(false, AllowInitialValue=true)]
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

        [DataMember, StringLength(0xff), RoundtripOriginal]
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

        [StringLength(5), DataMember, RoundtripOriginal]
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

        [DataMember, RoundtripOriginal, StringLength(5)]
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

        [Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), ReadOnly(true), RoundtripOriginal, DataMember, Editable(false)]
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

        [DataMember, RoundtripOriginal, StringLength(5)]
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

        [RoundtripOriginal, StringLength(5), DataMember]
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

        [StringLength(0xff), DataMember, RoundtripOriginal]
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

        [RoundtripOriginal, StringLength(0xff), DataMember]
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

        [DataMember, StringLength(5), RoundtripOriginal]
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

        [ConcurrencyCheck, DataMember, RoundtripOriginal]
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

        [DataMember, StringLength(0xff), RoundtripOriginal]
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

