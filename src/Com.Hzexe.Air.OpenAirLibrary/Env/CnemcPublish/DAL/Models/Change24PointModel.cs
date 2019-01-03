namespace Env.CnemcPublish.DAL.Models
{
    using System;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL.Models")]
    public sealed class Change24PointModel : ComplexObject
    {
        private DateTime _dataDateTime;
        private string _latitude;
        private int _level;
        private string _longitude;

        [DataMember]
        public DateTime DataDateTime
        {
            get
            {
                return this._dataDateTime;
            }
            set
            {
                if (this._dataDateTime != value)
                {
                    base.RaiseDataMemberChanging("DataDateTime");
                    base.ValidateProperty("DataDateTime", value);
                    this._dataDateTime = value;
                    base.RaiseDataMemberChanged("DataDateTime");
                }
            }
        }

        [DataMember]
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

        [DataMember]
        public int Level
        {
            get
            {
                return this._level;
            }
            set
            {
                if (this._level != value)
                {
                    base.RaiseDataMemberChanging("Level");
                    base.ValidateProperty("Level", value);
                    this._level = value;
                    base.RaiseDataMemberChanged("Level");
                }
            }
        }

        [DataMember]
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
    }
}

