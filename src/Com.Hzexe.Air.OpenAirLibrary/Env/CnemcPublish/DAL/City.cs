namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class City : Entity
    {
        private int _cityCode;
        private string _cityJC;
        private string _cityName;
        private int _id;
        private string[] _openAccessGenerated;
        private int _provinceId;

        public override object GetIdentity()
        {
            return this._id;
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

        [StringLength(10), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string CityJC
        {
            get
            {
                return this._cityJC;
            }
            set
            {
                if (this._cityJC != value)
                {
                    base.RaiseDataMemberChanging("CityJC");
                    base.ValidateProperty("CityJC", value);
                    this._cityJC = value;
                    base.RaiseDataMemberChanged("CityJC");
                }
            }
        }

        [DataMember, ConcurrencyCheck, RoundtripOriginal, StringLength(50)]
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

        [Key, DataMember, RoundtripOriginal, Editable(false, AllowInitialValue=true), ConcurrencyCheck]
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

        [ReadOnly(true), Editable(false), RoundtripOriginal, DataMember, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-")]
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

        [DataMember, ConcurrencyCheck, RoundtripOriginal]
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
    }
}

