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

        public Change24PointModel()
        {
           
            return;
        }

        [DataMember]
        public DateTime DataDateTime
        {
            get
            {
                return this._dataDateTime;
            }
            set
            {
                if ((this._dataDateTime != value) == null)
                {
                    goto Label_003C;
                }
                base.RaiseDataMemberChanging("DataDateTime");
                base.ValidateProperty("DataDateTime", (DateTime) value);
                this._dataDateTime = value;
                base.RaiseDataMemberChanged("DataDateTime");
            Label_003C:
                return;
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

        [DataMember]
        public int Level
        {
            get
            {
                return this._level;
            }
            set
            {
                if (this._level == value)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Level");
                base.ValidateProperty("Level", (int) value);
                this._level = value;
                base.RaiseDataMemberChanged("Level");
            Label_0037:
                return;
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
    }
}

