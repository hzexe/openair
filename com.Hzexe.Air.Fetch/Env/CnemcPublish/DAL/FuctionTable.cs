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

        public FuctionTable()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
        }

        [StringLength(20), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string FuctionName
        {
            get
            {
                return this._fuctionName;
            }
            set
            {
                if ((this._fuctionName != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("FuctionName");
                base.ValidateProperty("FuctionName", value);
                this._fuctionName = value;
                base.RaiseDataMemberChanged("FuctionName");
            Label_0037:
                return;
            }
        }

        [Editable(false, AllowInitialValue=true), RoundtripOriginal, ConcurrencyCheck, DataMember, Key]
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

        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public int MaxCount
        {
            get
            {
                return this._maxCount;
            }
            set
            {
                if (this._maxCount == value)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("MaxCount");
                base.ValidateProperty("MaxCount", (int) value);
                this._maxCount = value;
                base.RaiseDataMemberChanged("MaxCount");
            Label_0037:
                return;
            }
        }

        [RoundtripOriginal, DataMember, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), Editable(false), ReadOnly(true)]
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
    }
}

