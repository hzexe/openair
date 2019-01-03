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

        [Display(AutoGenerateField=false), DataMember]
        public List<Change24PointModel> DataList
        {
            get
            {
                return this._dataList;
            }
            set
            {
                if (this._dataList != value)
                {
                    base.RaiseDataMemberChanging("DataList");
                    base.ValidateProperty("DataList", value);
                    this._dataList = value;
                    base.RaiseDataMemberChanged("DataList");
                }
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
                if (this._endTime != value)
                {
                    base.RaiseDataMemberChanging("EndTime");
                    base.ValidateProperty("EndTime", value);
                    this._endTime = value;
                    base.RaiseDataMemberChanged("EndTime");
                }
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
                if (this._starTime != value)
                {
                    base.RaiseDataMemberChanging("StarTime");
                    base.ValidateProperty("StarTime", value);
                    this._starTime = value;
                    base.RaiseDataMemberChanged("StarTime");
                }
            }
        }
    }
}

