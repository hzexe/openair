using System;
using System.Linq;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// <see cref="DomainContext"/> generated as the base class for providers implementing
    /// <c>OpenRiaServices.DomainServices.Server.ApplicationServices.IAuthentication&lt;T&gt;</c>.
    /// </summary>
    [DomainIdentifier("Authentication", IsApplicationService = true)]
    public abstract class AuthenticationDomainContextBase : DomainContext
    {
        #region Member fields

        private Type _userType;
        private EntitySet _userSet;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDomainContextBase"/> class.
        /// </summary>
        /// <param name="domainClient">The <see cref="DomainClient"/> instance this <see cref="AuthenticationDomainContextBase"/> should use</param>
        protected AuthenticationDomainContextBase(DomainClient domainClient) : base(domainClient) { }

        #endregion

        #region Properties


        /// <summary>
        /// Gets the type of the user entity.
        /// </summary>
        /// <remarks>
        /// Since the user type is only declared in the generated class, this accessor
        /// allows the <see cref="WebAuthenticationService"/> to take type-specific actions.
        /// </remarks>
        internal Type UserType
        {
            get
            {
                if (this._userType == null)
                {
                    // The load methods are generated with an attribute indicating the entity type
                    MethodInfo loadUserInfo = 
                        this.GetType().GetMethods().Where(m => m.Name == "GetUserQuery").FirstOrDefault();
                    if (loadUserInfo != null)
                    {
                        this._userType = loadUserInfo.ReturnType.GetGenericArguments().Single();
                    }
                    if (this._userType == null)
                    {
                        throw new InvalidOperationException(Resources.ApplicationServices_NoLoadUserMethod);
                    }
                }
                return this._userType;
            }
        }

        /// <summary>
        /// Gets the <see cref="EntitySet"/> for the user entity.
        /// </summary>
        /// <remarks>
        /// This allows <see cref="WebAuthenticationService"/> to deal with users in a
        /// type-agnostic way.
        /// </remarks>
        internal EntitySet UserSet
        {
            get
            {
                if (this._userSet == null)
                {
                    this.EntityContainer.TryGetEntitySet(this.UserType, out this._userSet);
                }
                return this._userSet;
            }
        }

        #endregion
    }
}
