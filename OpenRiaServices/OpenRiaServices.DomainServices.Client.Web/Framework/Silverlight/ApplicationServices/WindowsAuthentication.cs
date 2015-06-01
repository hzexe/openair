using System;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// <see cref="AuthenticationService"/> that performs Windows authentication using
    /// a <see cref="OpenRiaServices.DomainServices.Client.DomainContext"/> generated from a domain service
    /// implementing <c>OpenRiaServices.DomainServices.Server.ApplicationServices.IAuthentication&lt;T&gt;</c>.
    /// </summary>
    public class WindowsAuthentication : WebAuthenticationService
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsAuthentication"/> class.
        /// </summary>
        public WindowsAuthentication() { }

        #endregion

        #region Methods

        /// <summary>
        /// <c>Login</c> is not an operation supported for Windows authentication
        /// </summary>
        /// <param name="parameters">The parameter is not used.</param>
        /// <param name="callback">The parameter is not used.</param>
        /// <param name="state">The parameter is not used.</param>
        /// <returns>The result.</returns>
        /// <exception cref="NotSupportedException"> is always thrown.</exception>
        protected override IAsyncResult BeginLogin(LoginParameters parameters, AsyncCallback callback, object state)
        {
            throw new NotSupportedException(Resources.ApplicationServices_WANoLogin);
        }

        /// <summary>
        /// <c>Logout</c> is not an operation supported for Windows authentication
        /// </summary>
        /// <param name="callback">The parameter is not used.</param>
        /// <param name="state">The parameter is not used.</param>
        /// <returns>The result.</returns>
        /// <exception cref="NotSupportedException"> is always thrown.</exception>
        protected override IAsyncResult BeginLogout(AsyncCallback callback, object state)
        {
            throw new NotSupportedException(Resources.ApplicationServices_WANoLogout);
        }

        #endregion
    }
}
