using System;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Attribute used internally by Open Ria Services to control if GET or POST should be 
    /// used when accessing the service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HasSideEffectsAttribute : Attribute
    {
        private readonly bool _hasSideEffects;

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSideEffectsAttribute"/> class.
        /// </summary>
        public HasSideEffectsAttribute()
            : this(true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSideEffectsAttribute"/> class.
        /// </summary>
        /// <param name="hasSideEffects"><c>true</c> if the method has side effects and POST.</param>
        public HasSideEffectsAttribute(bool hasSideEffects)
        {
            _hasSideEffects = hasSideEffects;
        }

        /// <summary>
        /// Gets a value indicating whether the the method has side effects and POST should be used to access
        /// the service, false means use GET.
        /// </summary>
        public bool HasSideEffects { get { return _hasSideEffects; } }
    }
}
