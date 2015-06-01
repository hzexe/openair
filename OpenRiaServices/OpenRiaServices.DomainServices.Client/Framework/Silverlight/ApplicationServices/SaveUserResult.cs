using System.Security.Principal;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Result returned from <see cref="AuthenticationService.EndSaveUser"/>
    /// </summary>
    public sealed class SaveUserResult : AuthenticationResult
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveUserResult"/> class.
        /// </summary>
        /// <param name="user">The saved user</param>
        public SaveUserResult(IPrincipal user)
            : base(user)
        {
        }

        #endregion
    }
}
