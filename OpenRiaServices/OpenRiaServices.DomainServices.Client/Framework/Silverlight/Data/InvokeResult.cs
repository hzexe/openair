using System;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// The value of a sucessfully completed Invoke operation
    /// </summary>
    public class InvokeResult
    {

    }

    /// <summary>
    /// The value of a sucessfully completed Invoke operation
    /// </summary>
    public class InvokeResult<T> : InvokeResult
    {
        private readonly T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeResult{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public InvokeResult(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value  
        {
            get { return _value; }
        }
    }

}
