namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Used to control identity cache behavior when loading entities.
    /// Each option specifies the merge behavior when an entity that has been
    /// previously read and cached is read again.
    /// </summary>
    public enum LoadBehavior
    {
        /// <summary>
        /// Values from the newly read instance are merged into the cached instance
        /// for property values that are unmodified. No changes are lost in this merge.
        /// </summary>
        MergeIntoCurrent,

        /// <summary>
        /// The cached instance is not changed and the newly read instance is discarded.
        /// </summary>
        KeepCurrent,

        /// <summary>
        /// All members of the cached instance are overwritten with current values from the
        /// newly read instance, regardless of whether they have been modified. In addition,
        /// the original state of the entity is also set to the newly read instance.
        /// </summary>
        RefreshCurrent
    }
}
