namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class Province : Entity
    {
        private int _id;
        private string[] _openAccessGenerated;
        private int? _provinceCode;
        private string _provinceJC;
        private string _provinceName;

        public override object GetIdentity()
        {
            return this._id;
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember, Editable(false, AllowInitialValue=true), Key]
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

        [DataMember, ReadOnly(true), RoundtripOriginal, Editable(false), Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-")]
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

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
        public int? ProvinceCode
        {
            get
            {
                return this._provinceCode;
            }
            set
            {
                if (this._provinceCode != value)
                {
                    base.RaiseDataMemberChanging("ProvinceCode");
                    base.ValidateProperty("ProvinceCode", value);
                    this._provinceCode = value;
                    base.RaiseDataMemberChanged("ProvinceCode");
                }
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, StringLength(50)]
        public string ProvinceJC
        {
            get
            {
                return this._provinceJC;
            }
            set
            {
                if (this._provinceJC != value)
                {
                    base.RaiseDataMemberChanging("ProvinceJC");
                    base.ValidateProperty("ProvinceJC", value);
                    this._provinceJC = value;
                    base.RaiseDataMemberChanged("ProvinceJC");
                }
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember, StringLength(50)]
        public string ProvinceName
        {
            get
            {
                return this._provinceName;
            }
            set
            {
                if (this._provinceName != value)
                {
                    base.RaiseDataMemberChanging("ProvinceName");
                    base.ValidateProperty("ProvinceName", value);
                    this._provinceName = value;
                    base.RaiseDataMemberChanged("ProvinceName");
                }
            }
        }
    }
}

