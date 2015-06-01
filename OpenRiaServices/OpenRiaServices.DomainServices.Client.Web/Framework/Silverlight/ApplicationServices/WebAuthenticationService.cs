using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Abstract extension of the <see cref="AuthenticationService"/> that
    /// interacts with a <see cref="DomainContext"/> generated from a domain
    /// service implementing
    /// <c>OpenRiaServices.DomainServices.Server.ApplicationServices.IAuthentication&lt;T&gt;</c>.
    /// </summary>
    public abstract class WebAuthenticationService : AuthenticationService
    {
        #region Static fields

        private const string LoginQueryName = "LoginQuery";
        private const string LogoutQueryName = "LogoutQuery";
        private const string LoadUserQueryName = "GetUserQuery";

        #endregion

        #region Member fields

        private readonly object _syncLock = new object();

        private bool _initialized;
        private string _domainContextType;
        private AuthenticationDomainContextBase _domainContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAuthenticationService"/> class.
        /// </summary>
        internal WebAuthenticationService() 
        { 
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of the domain context.
        /// </summary>
        /// <remarks>
        /// If the <see cref="DomainContext"/> is not set when this service is
        /// started, it will instantiate a context specified by the
        /// <see cref="DomainContextType"/>. In determining the type, this
        /// string is treated as the full name of a type in the application 
        /// assembly. If the initial search does not return a valid type, this 
        /// string is treated as the assembly qualified name of a type.
        /// </remarks>
        /// <exception cref="InvalidOperationException"> is thrown if this
        /// property is set after the service is started.
        /// </exception>
        public string DomainContextType
        {
            get
            {
                return this._domainContextType;
            }

            set
            {
                this.AssertIsNotActive();
                this._domainContextType = value;
            }
        }

        /// <summary>
        /// Gets the domain context this service delegates authenticating, loading, 
        /// and saving to.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if this
        /// property is set after the service is started.
        /// </exception>
        public AuthenticationDomainContextBase DomainContext
        {
            get
            {
                return this._domainContext;
            }

            set
            {
                this.AssertIsNotActive();
                this._domainContext = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether this service supports cancellation.
        /// </summary>
        /// <remarks>
        /// This implementation always returns <c>true</c>.
        /// </remarks>
        protected override bool SupportsCancellation
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a default user.
        /// </summary>
        /// <remarks>
        /// Creates a user using the default constructor.
        /// </remarks>
        /// <returns>A default user</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the
        /// <see cref="WebAuthenticationService.DomainContext"/> is <c>null</c> and a new instance
        /// cannot be created.
        /// </exception>
        protected override IPrincipal CreateDefaultUser()
        {
            this.Initialize();

            IPrincipal user = null;

            if (user == null)
            {
                ConstructorInfo userConstructor = this.DomainContext.UserType.GetConstructor(Type.EmptyTypes);

                if (userConstructor != null)
                {
                    try
                    {
                        user = (IPrincipal)userConstructor.Invoke(Type.EmptyTypes);
                    }
                    catch (TargetInvocationException tie)
                    {
                        if (tie.InnerException != null)
                        {
                            throw tie.InnerException;
                        }
                        throw;
                    }
                }
            }

            if (user == null)
            {
                throw new InvalidOperationException(Resources.ApplicationServices_CannotInitializeUser);
            }

            return user;
        }

        /// <summary>
        /// Begins an asynchronous <c>Login</c> operation.
        /// </summary>
        /// <param name="parameters">Login parameters that specify the user to authenticate</param>
        /// <param name="callback">The callback to invoke when the asynchronous call completes</param>
        /// <param name="state">The optional result state</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the
        /// <see cref="WebAuthenticationService.DomainContext"/> is <c>null</c> and a new instance
        /// cannot be created.
        /// </exception>
        protected override IAsyncResult BeginLogin(LoginParameters parameters, AsyncCallback callback, object state)
        {
            this.Initialize();

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            WebAsyncResult result = new WebAsyncResult(callback, state);
            EntityQuery query;

            try
            {
                query = (EntityQuery)this.DomainContext.GetType().GetMethod(
                    WebAuthenticationService.LoginQueryName,
                    new Type[] { typeof(string), typeof(string), typeof(bool), typeof(string) }).Invoke(
                    this.DomainContext,
                    new object[] { parameters.UserName, parameters.Password, parameters.IsPersistent, parameters.CustomData });
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }
                throw;
            }

            result.InnerOperation = this.DomainContext.Load(
                query,
                LoadBehavior.MergeIntoCurrent,
                (Action<LoadOperation>)this.HandleOperationComplete,
                result);

            return result;
        }

        /// <summary>
        /// Cancels an asynchronous <c>Login</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogin"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogin"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override void CancelLogin(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult, true);
            if (result.InnerOperation.CanCancel)
            {
                result.InnerOperation.Cancel();
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>Login</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogin"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>Login</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogin"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override LoginResult EndLogin(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult);

            if (result.InnerOperation.HasError)
            {
                throw result.InnerOperation.Error;
            }

            IPrincipal user = (IPrincipal)((LoadOperation)result.InnerOperation).Entities.SingleOrDefault();
            this.PrepareUser(user);
            return new LoginResult(user, (user != null));
        }

        /// <summary>
        /// Begins an asynchronous <c>Logout</c> operation.
        /// </summary>
        /// <param name="callback">The callback to invoke when the asynchronous call completes</param>
        /// <param name="state">The optional result state</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the
        /// <see cref="WebAuthenticationService.DomainContext"/> is <c>null</c> and a new instance
        /// cannot be created.
        /// </exception>
        protected override IAsyncResult BeginLogout(AsyncCallback callback, object state)
        {
            this.Initialize();

            WebAsyncResult result = new WebAsyncResult(callback, state);
            EntityQuery query;

            try
            {
                query = (EntityQuery)this.DomainContext.GetType().GetMethod(
                    WebAuthenticationService.LogoutQueryName,
                    Type.EmptyTypes).Invoke(
                    this.DomainContext,
                    Type.EmptyTypes);
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }
                throw;
            }

            result.InnerOperation = this.DomainContext.Load(
                query,
                LoadBehavior.MergeIntoCurrent,
                (Action<LoadOperation>)this.HandleOperationComplete,
                result);

            return result;
        }

        /// <summary>
        /// Cancels an asynchronous <c>Logout</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogout"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogout"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override void CancelLogout(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult, true);
            if (result.InnerOperation.CanCancel)
            {
                result.InnerOperation.Cancel();
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>Logout</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogout"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>Logout</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogout"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override LogoutResult EndLogout(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult);

            if (result.InnerOperation.HasError)
            {
                throw result.InnerOperation.Error;
            }

            IPrincipal user = (IPrincipal)((LoadOperation)result.InnerOperation).Entities.SingleOrDefault();
            if (user == null)
            {
                throw new InvalidOperationException(Resources.ApplicationServices_LogoutNoUser);
            }
            this.PrepareUser(user);
            return new LogoutResult(user);
        }

        /// <summary>
        /// Begins an asynchronous <c>LoadUser</c> operation.
        /// </summary>
        /// <param name="callback">The callback to invoke when the asynchronous call completes</param>
        /// <param name="state">The optional result state</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the
        /// <see cref="WebAuthenticationService.DomainContext"/> is <c>null</c> and a new instance
        /// cannot be created.
        /// </exception>
        protected override IAsyncResult BeginLoadUser(AsyncCallback callback, object state)
        {
            this.Initialize();

            WebAsyncResult result = new WebAsyncResult(callback, state);
            EntityQuery query;

            try
            {
                query = (EntityQuery)this.DomainContext.GetType().GetMethod(
                    WebAuthenticationService.LoadUserQueryName,
                    Type.EmptyTypes).Invoke(
                    this.DomainContext,
                    Type.EmptyTypes);
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }
                throw;
            }

            result.InnerOperation = this.DomainContext.Load(
                query,
                LoadBehavior.MergeIntoCurrent,
                (Action<LoadOperation>)this.HandleOperationComplete,
                result);

            return result;
        }

        /// <summary>
        /// Cancels an asynchronous <c>LoadUser</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginLoadUser"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLoadUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override void CancelLoadUser(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult, true);
            if (result.InnerOperation.CanCancel)
            {
                result.InnerOperation.Cancel();
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>LoadUser</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginLoadUser"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>LoadUser</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLoadUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override LoadUserResult EndLoadUser(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult);

            if (result.InnerOperation.HasError)
            {
                throw result.InnerOperation.Error;
            }

            IPrincipal user = (IPrincipal)((LoadOperation)result.InnerOperation).Entities.SingleOrDefault();
            if (user == null)
            {
                throw new InvalidOperationException(Resources.ApplicationServices_LoadNoUser);
            }
            this.PrepareUser(user);
            return new LoadUserResult(user);
        }

        /// <summary>
        /// Begins an asynchronous <c>SaveUser</c> operation.
        /// </summary>
        /// <param name="user">The authenticated user to save</param>
        /// <param name="callback">The callback to invoke when the asynchronous call completes</param>
        /// <param name="state">The optional result state</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the user is anonymous.</exception>
        /// <exception cref="InvalidOperationException"> is thrown if the
        /// <see cref="WebAuthenticationService.DomainContext"/> is <c>null</c> and a new instance
        /// cannot be created.
        /// </exception>
        protected override IAsyncResult BeginSaveUser(IPrincipal user, AsyncCallback callback, object state)
        {
            this.Initialize();

            if (!user.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException(Resources.ApplicationServices_CannotSaveAnonymous);
            }

            WebAsyncResult result = new WebAsyncResult(callback, state);
            result.InnerOperation = this.DomainContext.SubmitChanges(
                (Action<SubmitOperation>)this.HandleOperationComplete,
                result);

            return result;
        }

        /// <summary>
        /// Cancels an asynchronous <c>SaveUser</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginSaveUser"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginSaveUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override void CancelSaveUser(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult, true);
            if (result.InnerOperation.CanCancel)
            {
                result.InnerOperation.Cancel();
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>SaveUser</c> operation.
        /// </summary>
        /// <param name="asyncResult">A result returned from <see cref="BeginSaveUser"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>SaveUser</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginSaveUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        protected override SaveUserResult EndSaveUser(IAsyncResult asyncResult)
        {
            WebAsyncResult result = AsyncResultBase.EndAsyncOperation<WebAsyncResult>(asyncResult);

            if (result.InnerOperation.HasError)
            {
                throw result.InnerOperation.Error;
            }

            if (((SubmitOperation)result.InnerOperation).EntitiesInError.Any())
            {
                throw new InvalidOperationException(Resources.ApplicationServices_SaveErrors);
            }

            IPrincipal user = (IPrincipal)((SubmitOperation)result.InnerOperation).ChangeSet.OfType<IPrincipal>().SingleOrDefault();
            this.PrepareUser(user);
            return new SaveUserResult(user);
        }

        /// <summary>
        /// Handles completion of the underlying operation in the <see cref="DomainContext"/>.
        /// </summary>
        /// <param name="operation">The operation that completed</param>
        private void HandleOperationComplete(OperationBase operation)
        {
            WebAsyncResult result = ((WebAsyncResult)operation.UserState);
            if (operation.HasError)
            {
                operation.MarkErrorAsHandled();
            }
            if (!operation.IsCanceled)
            {
                result.Complete();
            }
        }

        /// <summary>
        /// Initializes this authentication service.
        /// </summary>
        /// <remarks>
        /// This method is invoked before the service is used for the first time from
        /// <see cref="CreateDefaultUser"/> and the <c>BeginXx</c> methods. It can also
        /// be called earlier to ensure the service is ready for use. Subsequent
        /// invocations will not reinitialize the service.
        /// </remarks>
        /// <exception cref="InvalidOperationException"> is thrown if the
        /// <see cref="WebAuthenticationService.DomainContext"/> is <c>null</c> and a new instance
        /// cannot be created.
        /// </exception>
        protected void Initialize()
        {
            lock (this._syncLock)
            {
                if (!this._initialized)
                {
                    this._initialized = true;
                    this.InitializeDomainContext();
                }
            }
        }

        /// <summary>
        /// Initializes the domain context.
        /// </summary>
        /// <remarks>
        /// If the domain context has not already been set, this method trys to instantiate
        /// one specified by the <see cref="DomainContextType"/> string.
        /// </remarks>
        /// <exception cref="InvalidOperationException"> is thrown if the
        /// <see cref="WebAuthenticationService.DomainContext"/> is <c>null</c> and a new instance
        /// cannot be created.
        /// </exception>
        private void InitializeDomainContext()
        {
            if (this._domainContext == null)
            {
                Type type = null;

                // Get application assembly so we can start searching for web context type there
                Assembly applicationAssembly =
#if SILVERLIGHT
                    Application.Current.GetType().Assembly;
#else
                    Assembly.GetEntryAssembly();
#endif

                if (!string.IsNullOrEmpty(this.DomainContextType))
                {
                    // First, try to load the type by full name from the application assembly

                    type = applicationAssembly.GetType(this.DomainContextType);
                    // If that doesn't work, allow for assembly qualified names
                    if (type == null)
                    {
                        type = Type.GetType(this.DomainContextType);
                    }
                }

                if (type == null)
                {
                    // Finally, we'll look for a domain context that has been generated from a domain 
                    // service extending AuthenticationBase<T>. Our CodeProcessor generates these 
                    // providers as extending AuthenticationDomainContextBase.
                    foreach (Type tempType in applicationAssembly.GetTypes())
                    {
                        if (typeof(AuthenticationDomainContextBase).IsAssignableFrom(tempType))
                        {
                            type = tempType;
                            break;
                        }
                    }
                }

                if ((type != null) && typeof(AuthenticationDomainContextBase).IsAssignableFrom(type))
                {
                    ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        try
                        {
                            this._domainContext = constructor.Invoke(Type.EmptyTypes) as AuthenticationDomainContextBase;
                        }
                        catch (TargetInvocationException tie)
                        {
                            if (tie.InnerException != null)
                            {
                                throw tie.InnerException;
                            }
                            throw;
                        }
                    }
                }
            }

            if (this._domainContext == null)
            {
                throw new InvalidOperationException(Resources.ApplicationServices_CannotInitializeDomainContext);
            }
        }

        /// <summary>
        /// Throws an exception if the service is active.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if the service is active.
        /// </exception>
        private void AssertIsNotActive()
        {
            lock (this._syncLock)
            {
                if (this._initialized)
                {
                    throw new InvalidOperationException(Resources.ApplicationServices_ServiceMustNotBeActive);
                }
            }
        }

        /// <summary>
        /// Prepares the deserialized user to return from an End method.
        /// </summary>
        /// <remarks>
        /// This methods ensures only a single user is present in the entity set of the
        /// <see cref="WebAuthenticationService.DomainContext"/>.
        /// </remarks>
        /// <param name="user">The user to prepare</param>
        private void PrepareUser(IPrincipal user)
        {
            if (user != null)
            {
                this.ClearOldUsers(user);
            }
        }

        /// <summary>
        /// Clears all users but the one specified from the user entity set in the <see cref="DomainContext"/>.
        /// </summary>
        /// <param name="user">The single user to keep in the user entity set</param>
        private void ClearOldUsers(IPrincipal user)
        {
            IEnumerable<IPrincipal> usersToDetach = this.DomainContext.UserSet.OfType<IPrincipal>().Where(u => u != user).ToList();
            foreach (IPrincipal userToDetach in usersToDetach)
            {
                this.DomainContext.UserSet.Detach((Entity)userToDetach);
            }
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// The <see cref="IAsyncResult"/> type used by this authentication service.
        /// </summary>
        internal class WebAsyncResult : AsyncResultBase
        {
            public WebAsyncResult(AsyncCallback asyncCallback, object asyncState) : base(asyncCallback, asyncState) { }

            public OperationBase InnerOperation { get; set; }
        }

        #endregion
    }
}
