using System;
using System.Diagnostics;
using OpenRiaServices.DomainServices;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

#if SERVERFX
namespace OpenRiaServices.DomainServices.Server
#else
namespace OpenRiaServices.DomainServices.Client
#endif
{
    /// <summary>
    /// This exception indicates that an unrecoverable error has occurred 
    /// during the execution of a domain operation.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class DomainException : Exception
    {
#if !SERVERFX
        private string stackTrace;
#endif

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DomainException()
        {
        }

        /// <summary>
        /// Constructor that accepts a localized exception message.
        /// </summary>
        /// <param name="message">The localized exception message.</param>
        public DomainException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor that accepts a localized exception message and a custom error code.
        /// </summary>
        /// <param name="message">The localized exception message.</param>
        /// <param name="errorCode">The custom error code for the exception.</param>
        public DomainException(string message, int errorCode)
            : this(message)
        {
            this.ErrorCode = errorCode;
        }

#if !SERVERFX
        /// <summary>
        /// Constructor that accepts a localized exception message, a custom error code and a stack trace.
        /// </summary>
        /// <param name="message">The localized exception message.</param>
        /// <param name="errorCode">The custom error code for the exception.</param>
        /// <param name="stackTrace">The exception's stack trace.</param>
        public DomainException(string message, int errorCode, string stackTrace)
            : this(message, errorCode)
        {
            this.stackTrace = stackTrace;
        }
#endif

        /// <summary>
        /// Constructor that accepts a localized exception message and an inner exception.
        /// </summary>
        /// <param name="message">The localized exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
            Debug.Assert(!innerException.IsFatal(), "Fatal exception passed in as InnerException");
        }

        /// <summary>
        /// Constructor that accepts a localized exception message and an inner exception
        /// </summary>
        /// <param name="message">The localized exception message</param>
        /// <param name="errorCode">The custom error code for the exception</param>
        /// <param name="innerException">The inner exception</param>
        public DomainException(string message, int errorCode, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Constructor that takes serialization info
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context used for serialization</param>
        private DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ErrorCode = info.GetInt32("ErrorCode");
        }
#endif //!SILVERLIGHT

#if !SERVERFX
        /// <summary>
        /// Gets the exception's stack trace.
        /// </summary>
        public override string StackTrace
        {
            get
            {
                return this.stackTrace ?? base.StackTrace;
            }
        }
#endif

        /// <summary>
        /// Gets or sets the custom error code for this exception. Can be any user
        /// defined value.
        /// </summary>
        public int ErrorCode
        {
            get;
            set;
        }
    }
}
