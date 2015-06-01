namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Enumeration of the possible states an <see cref="Entity"/>
    /// can be in.
    /// </summary>
    public enum EntityState
    {
        /// <summary>
        /// The entity is not attached and is not being tracked
        /// </summary>
        Detached,

        /// <summary>
        /// The entity is in its original, unmodified state
        /// </summary>
        Unmodified,

        /// <summary>
        /// The entity has been modified
        /// </summary>
        Modified,

        /// <summary>
        /// The entity is attached in the new state
        /// </summary>
        New,

        /// <summary>
        /// The entity is marked for delete
        /// </summary>
        Deleted
    }
}
