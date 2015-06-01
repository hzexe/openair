namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class CityDayAQIPublishHistory : Entity, IEntityDatahash
    {
        private string _aqi;
        private string _area;
        private int _cityCode;
        private string _co_24h;
        private int _id;
        private string _measure;
        private string _no2_24h;
        private string _o3_8h_24h;
        private string[] _openAccessGenerated;
        private string _pm10_24h;
        private string _pm2_5_24h;
        private string _primaryPollutant;
        private string _quality;
        private string _so2_24h;
        private DateTime _timePoint;
        private string _unheathful;

        public CityDayAQIPublishHistory()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
        }

        [StringLength(5), DataMember, RoundtripOriginal]
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

        [DataMember, RoundtripOriginal, StringLength(50)]
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

        [RoundtripOriginal, DataMember]
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

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember, Key, RoundtripOriginal, Editable(false, AllowInitialValue = true)]
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

        [DataMember, RoundtripOriginal, StringLength(0xff)]
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

        [DataMember, RoundtripOriginal, StringLength(5)]
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

        [DataMember, StringLength(5), RoundtripOriginal]
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

        [StringLength(5), RoundtripOriginal, DataMember]
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

        [DataMember, StringLength(0xff), RoundtripOriginal]
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

        [StringLength(5), DataMember, RoundtripOriginal]
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

        [RoundtripOriginal, DataMember, ConcurrencyCheck]
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

        [StringLength(0xff), DataMember, RoundtripOriginal]
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
         [DataMember]
        public int datahash { get; set; }

         public new void RaiseDataMemberChanging(string x)
         {
             base.RaiseDataMemberChanging(x);
             base.RaiseDataMemberChanged(x);
         }
    }
}

