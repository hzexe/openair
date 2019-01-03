namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class FuctionTable : Entity
    {
        private string _fuctionName;
        private int _id;
        private int _maxCount;
        private string[] _openAccessGenerated;

        public override object GetIdentity()
        {
            return this._id;
        }

        [StringLength(20), DataMember, RoundtripOriginal, ConcurrencyCheck]
        public string FuctionName
        {
            get
            {
                return this._fuctionName;
            }
            set
            {
                if (this._fuctionName != value)
                {
                    base.RaiseDataMemberChanging("FuctionName");
                    base.ValidateProperty("FuctionName", value);
                    this._fuctionName = value;
                    base.RaiseDataMemberChanged("FuctionName");
                }
            }
        }

        [RoundtripOriginal, Editable(false, AllowInitialValue=true), DataMember, ConcurrencyCheck, Key]
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

        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public int MaxCount
        {
            get
            {
                return this._maxCount;
            }
            set
            {
                if (this._maxCount != value)
                {
                    base.RaiseDataMemberChanging("MaxCount");
                    base.ValidateProperty("MaxCount", value);
                    this._maxCount = value;
                    base.RaiseDataMemberChanged("MaxCount");
                }
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
                if (this._openAccessGenerated != value)
                {
                    base.ValidateProperty("OpenAccessGenerated", value);
                    this._openAccessGenerated = value;
                    base.RaisePropertyChanged("OpenAccessGenerated");
                }
            }
        }
    }
}

