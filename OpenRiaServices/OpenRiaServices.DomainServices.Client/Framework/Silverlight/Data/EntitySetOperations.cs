using System;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Enumeration of the types of operations permissable
    /// on an <see cref="EntitySet"/>.
    /// </summary>
    [Flags]
    public enum EntitySetOperations
    {
        /// <summary>
        /// Only read operations are permitted, no update operations are allowed.
        /// </summary>
        None = 0,

        /// <summary>
        /// New entities may be added
        /// </summary>
        Add = 1,
        
        /// <summary>
        /// Entities may be updated
        /// </summary>
        Edit = 2,
        
        /// <summary>
        /// Entities may be removed
        /// </summary>
        Remove = 4,

        /// <summary>
        /// Entities may be added, updated and removed
        /// </summary>
        All = Add | Edit | Remove
    }
}
