using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.ObjectModel;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// This exception indicates that at least one error has occurred
    /// during the processing of domain operations on the server.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class DomainOperationException : Exception
    {
#if !SILVERLIGHT
        // The internal safe serialization state object must not be implicitly serialized.
        // [NonSerialized] -- TODO: uncomment when CLR fixes 851783
#endif
        private DomainOperationExceptionData _data = new DomainOperationExceptionData();

#if !SILVERLIGHT
        // The new safe serialization model in CLR 4.0 requires an object of this type.
        // We keep our state information here for simplicity.
        // [Serializable] -- TODO: uncomment when CLR fixes 851783
#endif
        private struct DomainOperationExceptionData
#if !SILVERLIGHT
            // : ISafeSerializationData -- TODO: uncomment when CLR fixes 851783
#endif
        {
            public OperationErrorStatus Status;
            public string StackTrace;
            public int ErrorCode;
            public IEnumerable<ValidationResult> ValidationResults;
#if !SILVERLIGHT

            // TODO: uncomment when CLR fixes 851783
            ///// <summary>
            ///// Called by the deserialization logic after it the exception
            ///// has been created but not yet initialized.
            ///// </summary>
            ///// <param name="obj">The new uninitialized exception instance.</param>
            //void ISafeSerializationData.CompleteDeserialization(object obj)
            //{
            //    DomainOperationException exception = obj as DomainOperationException;
            //    exception._data = this;
            //}
#endif
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DomainOperationException()
            : this(/*message*/ null, /*innerException*/ null, /*status*/ OperationErrorStatus.ServerError, /*errorCode*/ 0, /*stackTrace*/ null, /*validationErrors*/ null)
        {
        }

        /// <summary>
        /// Constructor that accepts only a localized exception message
        /// </summary>
        /// <param name="message">The localized exception message</param>
        public DomainOperationException(string message)
            : this(message, /*innerException*/ null, /*status*/ OperationErrorStatus.ServerError, /*errorCode*/ 0, /*stackTrace*/ null, /*validationErrors*/ null)
        {
        }

        /// <summary>
        /// Constructor that accepts a localized exception message and status
        /// </summary>
        /// <param name="message">The localized exception message</param>
        /// <param name="status">The status of the exception</param>
        public DomainOperationException(string message, OperationErrorStatus status)
            : this(message, /*innerException*/ null, status, /*errorCode*/ 0, /*stackTrace*/ null, /*validationErrors*/ null)
        {
        }

        /// <summary>
        /// Constructor that accepts a localized exception message and validation errors
        /// </summary>
        /// <param name="message">The localized exception message</param>
        /// <param name="validationErrors">The validation errors </param>
        public DomainOperationException(string message, IEnumerable<ValidationResult> validationErrors)
            : this(message, /*innerException*/ null, OperationErrorStatus.ValidationFailed, /*errorCode*/ 0, /*stackTrace*/ null, /*validationErrors:*/ validationErrors)
        {
        }

        /// <summary>
        /// Constructor that accepts a localized exception message, status and custom error code
        /// </summary>
        /// <param name="message">The localized exception message</param>
        /// <param name="status">The status of the exception</param>
        /// <param name="errorCode">The custom error code</param>
        public DomainOperationException(string message, OperationErrorStatus status, int errorCode)
            : this(message, /*innerException*/ null, status, errorCode, /*stackTrace*/ null, /*validationErrors*/ null)
        {
        }

        /// <summary>
        /// Constructor that accepts a localized exception message and an inner exception
        /// </summary>
        /// <param name="message">The localized exception message</param>
        /// <param name="innerException">The inner exception</param>
        public DomainOperationException(string message, Exception innerException)
            : this(message, innerException, /*status*/ OperationErrorStatus.ServerError, /*errorCode*/ 0, /*stackTrace*/ null, /*validationErrors*/ null)
        {
        }

        /// <summary>
        /// Constructor accepting optional localized message, status, 
        /// custom error code and stack trace of the exception. 
        /// </summary>
        /// <param name="message">The localized error message</param>
        /// <param name="status">status of the exception</param>
        /// <param name="errorCode">custom error code</param>
        /// <param name="stackTrace">stack trace of the exception</param>
        public DomainOperationException(string message, OperationErrorStatus status, int errorCode, string stackTrace)
            : this(message, /*innerException*/ null, status, errorCode, stackTrace, /*validationErrors*/ null)
        {
        }

        /// <summary>
        /// Constructor accepting optional localized message, status, 
        /// custom error code, stack trace of the exception and validation errors. 
        /// </summary>
        /// <param name="message">The localized error message</param>
        /// <param name="status">status of the exception</param>
        /// <param name="errorCode">custom error code</param>
        /// <param name="stackTrace">stack trace of the exception</param>
        /// <param name="validationErrors">validation errror of the exception</param>
        public DomainOperationException(string message, OperationErrorStatus status, int errorCode, string stackTrace, IEnumerable<ValidationResult> validationErrors)
            : this(message, /*innerException*/ null, status, errorCode, stackTrace, /*validationErrors*/ validationErrors)
        {
        }

        /// <summary>
        /// Internal copy constructor.
        /// </summary>
        /// <param name="message">The new error message to use</param>
        /// <param name="exception">The exception to copy</param>
        internal DomainOperationException(string message, DomainOperationException exception)
            : this(message, exception.InnerException, exception.Status, exception.ErrorCode, exception.StackTrace, exception.ValidationErrors)
        {
        }
        
        /// <summary>
        /// Private constructor used by all public constructors.
        /// </summary>
        /// <param name="message">The localized error message</param>
        /// <param name="innerException">Optional inner exception.</param>
        /// <param name="status">status of the exception</param>
        /// <param name="errorCode">custom error code</param>
        /// <param name="stackTrace">stack trace of the exception</param>
        /// <param name="validationErrors">validation errror of the exception</param>
        protected DomainOperationException(string message, Exception innerException, OperationErrorStatus status, int errorCode, string stackTrace, IEnumerable<ValidationResult> validationErrors)
            : base(message, innerException)
        {
            Debug.Assert(!innerException.IsFatal(), "Fatal exception passed in as InnerException");

            this._data.Status = status;
            this._data.StackTrace = stackTrace;
            this._data.ErrorCode = errorCode;
            // Iterate the enumerable now in order to capture the validation errors at the moment the exception is created
            if (validationErrors != null)
                this._data.ValidationResults = new ReadOnlyCollection<ValidationResult>(validationErrors.ToList());
            
#if !SILVERLIGHT
            // TODO: uncomment when CLR fixes 851783
            //// The new CLR 4.0 safe serialization model accepts custom data through
            //// this pattern.  We are called back during serialization to provide
            //// our custom data.
            //SerializeObjectState += delegate(object exception, SafeSerializationEventArgs eventArgs)
            //{
            //    eventArgs.AddSerializedState(this._data);
            //};
#endif
        }

        /// <summary>
        /// Gets the stack trace of the exception
        /// </summary>
        public override string StackTrace
        {
            get
            {
                if (this._data.StackTrace != null)
                {
                    return this._data.StackTrace;
                }
                return base.StackTrace;
            }
        }

        /// <summary>
        /// Gets or sets the status code for this exception. See <see cref="OperationErrorStatus"/>
        /// </summary>
        public OperationErrorStatus Status
        {
            get
            {
                return this._data.Status;
            }
            set
            {
                this._data.Status = value;
            }
        }

        /// <summary>
        /// Gets or sets the custom error code for this exception. Can be any user
        /// defined value.
        /// </summary>
        public int ErrorCode
        {
            get { return this._data.ErrorCode; }
            set { this._data.ErrorCode = value; }
        }

        /// <summary>
        /// Gets any validation errors for this exception.
        /// </summary>
        public IEnumerable<ValidationResult> ValidationErrors
        {
            get
            {
                if (_data.ValidationResults == null)
                    _data.ValidationResults = Enumerable.Empty<ValidationResult>();
                return _data.ValidationResults;
            }
        }
    }
}
