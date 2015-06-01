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

        public PublishLog()
        {
           
            return;
        }

        public override object GetIdentity()
        {
            return (int) this._id;
        }

        [RoundtripOriginal, ConcurrencyCheck, DataMember]
        public DateTime Date
        {
            get
            {
                return this._date;
            }
            set
            {
                if ((this._date != value) == null)
                {
                    goto Label_003C;
                }
                base.RaiseDataMemberChanging("Date");
                base.ValidateProperty("Date", (DateTime) value);
                this._date = value;
                base.RaiseDataMemberChanged("Date");
            Label_003C:
                return;
            }
        }

        [DataMember, StringLength(0x7d0), RoundtripOriginal, ConcurrencyCheck]
        public string Exception
        {
            get
            {
                return this._exception;
            }
            set
            {
                if ((this._exception != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Exception");
                base.ValidateProperty("Exception", value);
                this._exception = value;
                base.RaiseDataMemberChanged("Exception");
            Label_0037:
                return;
            }
        }

        [Key, DataMember, Editable(false, AllowInitialValue=true), ConcurrencyCheck, RoundtripOriginal]
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

        [ConcurrencyCheck, RoundtripOriginal, StringLength(50), DataMember]
        public string Level
        {
            get
            {
                return this._level;
            }
            set
            {
                if ((this._level != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Level");
                base.ValidateProperty("Level", value);
                this._level = value;
                base.RaiseDataMemberChanged("Level");
            Label_0037:
                return;
            }
        }

        [ConcurrencyCheck, DataMember, StringLength(0xff), RoundtripOriginal]
        public string Logger
        {
            get
            {
                return this._logger;
            }
            set
            {
                if ((this._logger != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Logger");
                base.ValidateProperty("Logger", value);
                this._logger = value;
                base.RaiseDataMemberChanged("Logger");
            Label_0037:
                return;
            }
        }

        [RoundtripOriginal, ConcurrencyCheck, StringLength(0xfa0), DataMember]
        public string Message
        {
            get
            {
                return this._message;
            }
            set
            {
                if ((this._message != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Message");
                base.ValidateProperty("Message", value);
                this._message = value;
                base.RaiseDataMemberChanged("Message");
            Label_0037:
                return;
            }
        }

        [Editable(false), RoundtripOriginal, Display(AutoGenerateField=false, AutoGenerateFilter=false, Description="OpenAccess Key", Name="-ID-"), DataMember, ReadOnly(true)]
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

        [ConcurrencyCheck, RoundtripOriginal, StringLength(0xff), DataMember]
        public string Thread
        {
            get
            {
                return this._thread;
            }
            set
            {
                if ((this._thread != value) == null)
                {
                    goto Label_0037;
                }
                base.RaiseDataMemberChanging("Thread");
                base.ValidateProperty("Thread", value);
                this._thread = value;
                base.RaiseDataMemberChanged("Thread");
            Label_0037:
                return;
            }
        }
    }
}

