namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class StationConfig : Entity
    {
        private string _area;
        private int? _cityCode;
        private int _id;
        private bool? _isContrast;
        private bool? _isPublish;
        private string _latitude;
        private string _longitude;
        private string[] _openAccessGenerated;
        private int _orderId;
        private string _positionName;
        private string _stationCode;
        private string _uniqueCode;

        public override object GetIdentity()
        {
            return this._id;
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember, StringLength(10)]
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
        public int? CityCode
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

        [Key, ConcurrencyCheck, DataMember, Editable(false, AllowInitialValue=true), RoundtripOriginal]
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

        [DataMember, ConcurrencyCheck, RoundtripOriginal]
        public bool? IsContrast
        {
            get
            {
                return this._isContrast;
            }
            set
            {
                if (this._isContrast != value)
                {
                    base.RaiseDataMemberChanging("IsContrast");
                    base.ValidateProperty("IsContrast", value);
                    this._isContrast = value;
                    base.RaiseDataMemberChanged("IsContrast");
                }
            }
        }

        [DataMember, ConcurrencyCheck, RoundtripOriginal]
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

        [StringLength(15), ConcurrencyCheck, DataMember, RoundtripOriginal]
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

        [Editable(false), Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), DataMember, ReadOnly(true), RoundtripOriginal]
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

        [ConcurrencyCheck, DataMember, RoundtripOriginal]
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

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(5)]
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

        [ConcurrencyCheck, StringLength(20), DataMember, RoundtripOriginal]
        public string UniqueCode
        {
            get
            {
                return this._uniqueCode;
            }
            set
            {
                if (this._uniqueCode != value)
                {
                    base.RaiseDataMemberChanging("UniqueCode");
                    base.ValidateProperty("UniqueCode", value);
                    this._uniqueCode = value;
                    base.RaiseDataMemberChanged("UniqueCode");
                }
            }
        }
    }
}

