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

        public StationConfig()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
        }

        [DataMember, StringLength(10), ConcurrencyCheck, RoundtripOriginal]
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

        [RoundtripOriginal, DataMember, ConcurrencyCheck]
        public int? CityCode { get; set; }

        [Key, ConcurrencyCheck, RoundtripOriginal, DataMember, Editable(false, AllowInitialValue=true)]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
        public bool? IsContrast { get; set; }

        [DataMember, RoundtripOriginal, ConcurrencyCheck]
        public bool? IsPublish { get; set; }

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(15)]
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

        [DataMember, ConcurrencyCheck, RoundtripOriginal, StringLength(15)]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
        public int OrderId
        {
            get
            {
                return this._orderId;
            }
            set
            {
                if (this._orderId == value)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("OrderId");
                base.ValidateProperty("OrderId", (int) value);
                this._orderId = value;
                base.RaiseDataMemberChanged("OrderId");
            Label_0037:
                return;
            }
        }

        [DataMember, ConcurrencyCheck, RoundtripOriginal, StringLength(40)]
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

        [DataMember, StringLength(5), ConcurrencyCheck, RoundtripOriginal]
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

        [ConcurrencyCheck, DataMember, RoundtripOriginal, StringLength(20)]
        public string UniqueCode
        {
            get
            {
                return this._uniqueCode;
            }
            set
            {
                if ((this._uniqueCode != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("UniqueCode");
                base.ValidateProperty("UniqueCode", value);
                this._uniqueCode = value;
                base.RaiseDataMemberChanged("UniqueCode");
            Label_0037:
                return;
            }
        }
    }
}

