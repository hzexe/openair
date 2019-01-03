namespace Env.CnemcPublish.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using OpenRiaServices.DomainServices.Client;

    [DataContract(Namespace="http://schemas.datacontract.org/2004/07/Env.CnemcPublish.DAL")]
    public sealed class PublishLog : Entity
    {
        private DateTime _date;
        private string _exception;
        private int _id;
        private string _level;
        private string _logger;
        private string _message;
        private string[] _openAccessGenerated;
        private string _thread;

        public override object GetIdentity()
        {
            return this._id;
        }

        [ConcurrencyCheck, RoundtripOriginal, DataMember]
        public DateTime Date
        {
            get
            {
                return this._date;
            }
            set
            {
                if (this._date != value)
                {
                    base.RaiseDataMemberChanging("Date");
                    base.ValidateProperty("Date", value);
                    this._date = value;
                    base.RaiseDataMemberChanged("Date");
                }
            }
        }

        [ConcurrencyCheck, StringLength(0x7d0), DataMember, RoundtripOriginal]
        public string Exception
        {
            get
            {
                return this._exception;
            }
            set
            {
                if (this._exception != value)
                {
                    base.RaiseDataMemberChanging("Exception");
                    base.ValidateProperty("Exception", value);
                    this._exception = value;
                    base.RaiseDataMemberChanged("Exception");
                }
            }
        }

        [ConcurrencyCheck, Key, DataMember, Editable(false, AllowInitialValue=true), RoundtripOriginal]
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

        [StringLength(50), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Level
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

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Logger
        {
            get
            {
                return this._logger;
            }
            set
            {
                if (this._logger != value)
                {
                    base.RaiseDataMemberChanging("Logger");
                    base.ValidateProperty("Logger", value);
                    this._logger = value;
                    base.RaiseDataMemberChanged("Logger");
                }
            }
        }

        [StringLength(0xfa0), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Message
        {
            get
            {
                return this._message;
            }
            set
            {
                if (this._message != value)
                {
                    base.RaiseDataMemberChanging("Message");
                    base.ValidateProperty("Message", value);
                    this._message = value;
                    base.RaiseDataMemberChanged("Message");
                }
            }
        }

        [Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), DataMember, RoundtripOriginal, Editable(false), ReadOnly(true)]
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

        [StringLength(0xff), ConcurrencyCheck, DataMember, RoundtripOriginal]
        public string Thread
        {
            get
            {
                return this._thread;
            }
            set
            {
                if (this._thread != value)
                {
                    base.RaiseDataMemberChanging("Thread");
                    base.ValidateProperty("Thread", value);
                    this._thread = value;
                    base.RaiseDataMemberChanged("Thread");
                }
            }
        }
    }
}

