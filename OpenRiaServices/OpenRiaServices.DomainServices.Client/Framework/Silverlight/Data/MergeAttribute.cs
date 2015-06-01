using System;

namespace OpenRiaServices.DomainServices.Server.Data
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MergeAttribute : Attribute
    {
        private bool _isMergeable;

        /// <summary>
        /// Default constructor
        /// Set the IsMergable Property True
        /// </summary>
        public MergeAttribute()
        {
            this._isMergeable = true;
        }


        /// <summary>
        /// Constructor used to specify a member is mergeable.
        /// </summary>
        /// <param name="isMergeable">The member name for the mergeable member.</param>
        public MergeAttribute(bool isMergeable)
        {
            this._isMergeable = isMergeable;
        }

        /// <summary>
        /// Gets the whether the property is IsMergeable 
        /// </summary>
        public bool IsMergeable
        {
            get { return _isMergeable; }
        }
    }
}