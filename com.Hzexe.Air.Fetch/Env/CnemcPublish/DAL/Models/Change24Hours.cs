namespace Env.CnemcPublish.DAL.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL.Models")]
    public sealed class Change24Hours : ComplexObject
    {
        private List<Change24PointModel> _dataList;
        private DateTime _endTime;
        private DateTime _starTime;

        public Change24Hours()
        {
           
            return;
        }

        [Display(AutoGenerateField=false), DataMember]
        public List<Change24PointModel> DataList
        {
            get
            {
                return this._dataList;
            }
            set
            {
                if (this._dataList == value)
                {
                    goto Label_0032;
                }
                base.RaiseDataMemberChanging("DataList");
                base.ValidateProperty("DataList", value);
                this._dataList = value;
                base.RaiseDataMemberChanged("DataList");
            Label_0032:
                return;
            }
        }

        [DataMember]
        public DateTime EndTime
        {
            get
            {
                return this._endTime;
            }
            set
            {
                if ((this._endTime != value) == null)
                {
                    goto Label_003C;
                }
                base.RaiseDataMemberChanging("EndTime");
                base.ValidateProperty("EndTime", (DateTime) value);
                this._endTime = value;
                base.RaiseDataMemberChanged("EndTime");
            Label_003C:
                return;
            }
        }

        [DataMember]
        public DateTime StarTime
        {
            get
            {
                return this._starTime;
            }
            set
            {
                if ((this._starTime != value) == null)
                {
                    goto Label_003C;
                }
                base.RaiseDataMemberChanging("StarTime");
                base.ValidateProperty("StarTime", (DateTime) value);
                this._starTime = value;
                base.RaiseDataMemberChanged("StarTime");
            Label_003C:
                return;
            }
        }
    }
}

