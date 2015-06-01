using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// This exception indicates that at least one error has occurred
    /// during the processing of submit operations on the server.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class SubmitOperationException : DomainOperationException
    {
        private readonly EntityChangeSet _changeSet;
        private readonly ReadOnlyCollection<Entity> _entitiesInError;

        /// <summary>
        /// Constructor that accepts a localized exception message and status
        /// </summary>
        /// <param name="changeSet">the changeset being submitted</param>
        /// <param name="message">The localized exception message</param>
        /// <param name="status">The status of the exception</param>
        public SubmitOperationException(EntityChangeSet changeSet, string message, OperationErrorStatus status)
            : this(changeSet, message, /*innerException*/ null, status, /*errorCode*/ 0, /*stackTrace*/ null)
        {
        }

        /// <summary>
        /// Constructor that accepts a localized exception message and status
        /// </summary>
        /// <param name="changeSet">the changeset being submitted</param>
        /// <param name="message">The localized exception message</param>
        /// <param name="innerException">inner exception.</param>
        public SubmitOperationException(EntityChangeSet changeSet, string message, Exception innerException)
            : this(changeSet, message, innerException, /*status*/ OperationErrorStatus.ServerError, /*errorCode*/ 0, /*stackTrace*/ null)
        {
        }

        /// <summary>
        /// Internal "copy" constructor.
        /// </summary>
        /// <param name="changeSet">the changeset being submitted</param>
        /// <param name="message">The new error message to use</param>
        /// <param name="exception">The exception to copy</param>
        internal SubmitOperationException(EntityChangeSet changeSet, string message, DomainOperationException exception)
            : this(changeSet, message, exception.InnerException, exception.Status, exception.ErrorCode, exception.StackTrace)
        {
        }

        /// <summary>
        /// Private constructor used by all public constructors.
        /// </summary>
        /// <param name="changeSet">the changeset being submitted</param>
        /// <param name="message">The localized error message</param>
        /// <param name="innerException">Optional inner exception.</param>
        /// <param name="status">status of the exception</param>
        /// <param name="errorCode">custom error code</param>
        /// <param name="stackTrace">stack trace of the exception</param>
        private SubmitOperationException(EntityChangeSet changeSet, string message, Exception innerException, OperationErrorStatus status, int errorCode, string stackTrace)
            : base(message, innerException, status, errorCode, stackTrace, GetValidationResults(changeSet))
        {
            _changeSet = changeSet;
            _entitiesInError = new ReadOnlyCollection<Entity>(changeSet
                                                                .Where(p => p.EntityConflict != null || p.HasValidationErrors)
                                                                .ToList());
        }

        /// <summary>
        /// Returns any entities in error after the submit operation completes.
        /// </summary>
        public IEnumerable<Entity> EntitiesInError
        {
            get
            {
                return _entitiesInError; 
            }
        }

        /// <summary>
        /// Create a IEnumerable which iterates the changeset and returns all validation errors
        /// </summary>
        /// <param name="changeSet">ChangeSet to read validation errors from</param>
        /// <returns></returns>
        private static IEnumerable<ValidationResult> GetValidationResults(EntityChangeSet changeSet)
        {
            return changeSet.Where(p => p.EntityConflict != null || p.HasValidationErrors)
                .SelectMany(p => p.ValidationErrors);
        }

        /// <summary>
        /// The changeset being submitted.
        /// </summary>
        public EntityChangeSet ChangeSet
        {
            get { return _changeSet; }
        }
    }
}