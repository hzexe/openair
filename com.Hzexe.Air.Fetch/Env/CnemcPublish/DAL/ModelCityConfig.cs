namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class ModelCityConfig : Entity
    {
        private int _cityCode;
        private string _cityName;
        private int _id;
        private bool _isPublish;
        private string[] _openAccessGenerated;
        private int? _orderId;
        private int _provinceId;

        public ModelCityConfig()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
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

        [DataMember, RoundtripOriginal, StringLength(50), ConcurrencyCheck]
        public string CityName
        {
            get
            {
                return this._cityName;
            }
            set
            {
                if ((this._cityName != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("CityName");
                base.ValidateProperty("CityName", value);
                this._cityName = value;
                base.RaiseDataMemberChanged("CityName");
            Label_0037:
                return;
            }
        }

        [Editable(false, AllowInitialValue=true), DataMember, ConcurrencyCheck, Key, RoundtripOriginal]
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

        [RoundtripOriginal, DataMember, ConcurrencyCheck]
        public bool IsPublish
        {
            get
            {
                return this._isPublish;
            }
            set
            {
                if (this._isPublish == value)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("IsPublish");
                base.ValidateProperty("IsPublish", (bool) value);
                this._isPublish = value;
                base.RaiseDataMemberChanged("IsPublish");
            Label_0037:
                return;
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

        [RoundtripOriginal, DataMember, ConcurrencyCheck]
        public int? OrderId { get; set; }

        [RoundtripOriginal, DataMember, ConcurrencyCheck]
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
    }
}

