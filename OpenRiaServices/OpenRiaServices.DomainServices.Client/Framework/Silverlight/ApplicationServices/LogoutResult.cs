using System.Security.Principal;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Result returned from <see cref="AuthenticationService.EndLogout"/>
    /// </summary>
    public sealed class LogoutResult : AuthenticationResult
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutResult"/> class.
        /// </summary>
        /// <param name="user">The anonymous user</param>
        public LogoutResult(IPrincipal user)
            : base(user)
        {
        }

        #endregion
    }
}
