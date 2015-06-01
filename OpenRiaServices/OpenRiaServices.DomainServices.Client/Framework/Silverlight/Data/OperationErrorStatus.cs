namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Enumeration representing possible domain operation error statuses.
    /// </summary>
    public enum OperationErrorStatus
    {
        /// <summary>
        /// Resource cannot be found.
        /// </summary>
        /// <remarks>Corresponds to HttpStatusCode 404</remarks>
        NotFound,

        /// <summary>
        /// A server error has occurred during the processing of the operation.
        /// </summary>
        /// <remarks>Corresponds to HttpStatusCode 500</remarks>
        ServerError,

        /// <summary>
        /// The operation is not supported.
        /// </summary>
        /// <remarks>Corresponds to HttpStatusCode 400</remarks>
        NotSupported,

        /// <summary>
        /// The operation is not authorized for execution.
        /// </summary>
        /// <remarks>Corresponds to HttpStatusCode 401</remarks>
        Unauthorized,
        
        /// <summary>
        /// Validation for the operation has failed.
        /// </summary>
        ValidationFailed,

        /// <summary>
        /// Conflicts have been detected. See <see cref="Entity.EntityConflict"/> 
        /// for the conflicts for a given entity.
        /// </summary>
        Conflicts
    }
}
