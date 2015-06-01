using System.Security.Principal;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Result returned from <see cref="AuthenticationService.EndLoadUser"/>
    /// </summary>
    public sealed class LoadUserResult : AuthenticationResult
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadUserResult"/> class.
        /// </summary>
        /// <param name="user">The loaded user</param>
        public LoadUserResult(IPrincipal user)
            : base(user)
        {
        }

        #endregion
    }
}
