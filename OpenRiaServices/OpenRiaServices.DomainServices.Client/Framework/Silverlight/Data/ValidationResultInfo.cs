using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

#if SERVERFX
namespace OpenRiaServices.DomainServices.Server
#else
namespace OpenRiaServices.DomainServices.Client
#endif
{
    /// <summary>
    /// The data contract of an error that has occurred 
    /// during the execution of an operation on the server.
    /// This is sent back along with the domain operation 
    /// result(s) to the client.
    /// </summary>
    [DataContract(Namespace = "DomainServices")]
    public sealed class ValidationResultInfo : IEquatable<ValidationResultInfo>
    {
        private string _message;
        private string _stackTrace;
        private int _errorCode;
        private IEnumerable<string> _sourceMemberNames;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ValidationResultInfo()
        {
        }

        /// <summary>
        /// Constructor accepting an optional localized error message and and collection 
        /// of the names of the members the error originated from.
        /// </summary>
        /// <param name="message">The localized message</param>
        /// <param name="sourceMemberNames">A collection of the names of the members the error originated from.</param>
        public ValidationResultInfo(string message, IEnumerable<string> sourceMemberNames)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (sourceMemberNames == null)
            {
                throw new ArgumentNullException("sourceMemberNames");
            }

            this._message = message;
            this._sourceMemberNames = sourceMemberNames;
        }

        /// <summary>
        /// Constructor accepting an optional localized error message, error code, stack trace,
        /// and collection of the names of the members the error originated from.
        /// </summary>
        /// <param name="message">The localized error message</param>
        /// <param name="errorCode">The custom error code</param>
        /// <param name="stackTrace">The error stack trace</param>
        /// <param name="sourceMemberNames">A collection of the names of the members the error originated from.</param>
        public ValidationResultInfo(string message, int errorCode, string stackTrace, IEnumerable<string> sourceMemberNames)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (sourceMemberNames == null)
            {
                throw new ArgumentNullException("sourceMemberNames");
            }

            this._message = message;
            this._errorCode = errorCode;
            this._stackTrace = stackTrace;
            this._sourceMemberNames = sourceMemberNames;
        }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Message
        {
            get { return this._message; }
            set { this._message = value; }
        }

        /// <summary>
        /// Gets or sets custom error code
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int ErrorCode
        {
            get { return this._errorCode; }
            set { this._errorCode = value; }
        }

        /// <summary>
        /// Gets or sets the stack trace of the error
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StackTrace
        {
            get { return this._stackTrace; }
            set { this._stackTrace = value; }
        }

        /// <summary>
        /// Gets or sets the names of the members the error originated from.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<string> SourceMemberNames
        {
            get { return this._sourceMemberNames; }
            set { this._sourceMemberNames = value; }
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>The hash code for this object.</returns>
        public override int GetHashCode()
        {
            return this.Message.GetHashCode();
        }

        #region IEquatable<ValidationResultInfo> Members

        /// <summary>
        /// Test the current instance against the specified instance for equality
        /// </summary>
        /// <param name="other">The ValidationResultInfo to compare to</param>
        /// <returns>True if the instances are equal, false otherwise</returns>
        bool IEquatable<ValidationResultInfo>.Equals(ValidationResultInfo other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            return ((this.Message == other.Message) &&
                    (this.ErrorCode == other.ErrorCode) &&
                    (this.StackTrace == other.StackTrace) &&
                    (this.SourceMemberNames.SequenceEqual(other.SourceMemberNames)));
        }

         #endregion

         /// <summary>
         /// Returns a <see cref="System.String" /> that represents this instance.
         /// </summary>
         /// <returns>
         /// A <see cref="System.String" /> that represents this instance.
         /// </returns>
         public override string ToString()
         {
            return this.Message ?? base.ToString();
         }
    }
}

