using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents the result of an invoke operation.
    /// </summary>
    public class InvokeCompletedResult
    {
        private object _returnValue;
        private ReadOnlyCollection<ValidationResult> _validationErrors;

        /// <summary>
        /// Initializes a new instance of the InvokeCompletedResult class
        /// </summary>
        /// <param name="returnValue">The value returned from the server.</param>
        public InvokeCompletedResult(object returnValue)
            : this(returnValue, new ValidationResult[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvokeCompletedResult class
        /// </summary>
        /// <param name="returnValue">The value returned from the server.</param>
        /// <param name="validationErrors">A collection of validation errors.</param>
        public InvokeCompletedResult(object returnValue, IEnumerable<ValidationResult> validationErrors)
        {
            if (validationErrors == null)
            {
                throw new ArgumentNullException("validationErrors");
            }

            this._returnValue = returnValue;
            this._validationErrors = validationErrors.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public IEnumerable<ValidationResult> ValidationErrors
        {
            get
            {
                return this._validationErrors;
            }
        }

        /// <summary>
        /// Gets the value returned from the server.
        /// </summary>
        public object ReturnValue
        {
            get
            {
                return this._returnValue;
            }
        }
    }
}