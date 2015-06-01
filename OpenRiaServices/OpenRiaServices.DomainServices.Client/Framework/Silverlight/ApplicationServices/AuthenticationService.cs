using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Service that is responsible for authenticating, loading, and saving the current user. 
    /// </summary>
    /// <remarks>
    /// This abstract base exposes <c>Login</c>, <c>Logout</c>, <c>LoadUser</c>, and
    /// <c>SaveUser</c> as asynchronous operations. It also provides a number of properties
    /// that can be bound to including <see cref="IsBusy"/> and <see cref="User"/>.
    /// <para>
    /// Concrete implementations will have a much different view of this class through a
    /// number of abstract template methods. These methods follow the async result pattern
    /// and are presented in Begin/End pairs for each operation. Optionally, cancel methods
    /// for each operation can also be implemented.
    /// </para>
    /// </remarks>
    public abstract class AuthenticationService : INotifyPropertyChanged
    {
        #region Member fields

        private readonly object _syncLock = new object();

        // By default, events will be dispatched to the context the service is created in
        private readonly SynchronizationContext _synchronizationContext =
            SynchronizationContext.Current ?? new SynchronizationContext();

        private IPrincipal _user;
        private PropertyChangedEventHandler _propertyChangedEventHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        protected AuthenticationService() 
        { 
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a new user is successfully logged in.
        /// </summary>
        /// <remarks>
        /// This event is raised either when <see cref="User"/> changes from anonymous to
        /// authenticated or when it changes from one authenticated identity to another.
        /// </remarks>
        public event EventHandler<AuthenticationEventArgs> LoggedIn;

        /// <summary>
        /// Raised when a user is successfully logged out.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="User"/> changes from authenticated to
        /// anonymous.
        /// </remarks>
        public event EventHandler<AuthenticationEventArgs> LoggedOut;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether an asynchronous operation is in progress
        /// </summary>
        public bool IsBusy
        {
            get { return (this.Operation != null); }
        }

        /// <summary>
        /// Gets a value indicating whether an asynchronous <c>Login</c> operation is in progress
        /// </summary>
        public bool IsLoggingIn
        {
            get { return this.IsBusy && (this.Operation is LoginOperation); }
        }

        /// <summary>
        /// Gets a value indicating whether an asynchronous <c>Logout</c> operation is in progress
        /// </summary>
        public bool IsLoggingOut
        {
            get { return this.IsBusy && (this.Operation is LogoutOperation); }
        }

        /// <summary>
        /// Gets a value indicating whether an asynchronous <c>LoadUser</c> operation is in progress
        /// </summary>
        public bool IsLoadingUser
        {
            get { return this.IsBusy && (this.Operation is LoadUserOperation); }
        }

        /// <summary>
        /// Gets a value indicating whether an asynchronous <c>SaveUser</c> operation is in progress
        /// </summary>
        public bool IsSavingUser
        {
            get { return this.IsBusy && (this.Operation is SaveUserOperation); }
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <remarks>
        /// This value may be updated by the <c>Login</c>, <c>Logout</c>, and <c>LoadUser</c>
        /// operations. Prior to one of those methods completing successfully, this property
        /// will contain a default user.
        /// </remarks>
        public IPrincipal User
        {
            get 
            {
                if (this._user == null)
                {
                    this._user = this.CreateDefaultUser();
                    if (this._user == null)
                    {
                        throw new InvalidOperationException(Resources.ApplicationServices_UserIsNull);
                    }
                }
                return this._user;
            }
        }

        /// <summary>
        /// Gets or sets the current operation.
        /// </summary>
        /// <remarks>
        /// Only one operation can be active at a time. This property should not be set directly
        /// but instead can be modified via the <see cref="StartOperation"/> method.
        /// </remarks>
        private AuthenticationOperation Operation { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously authenticates and logs in to the server with the specified parameters.
        /// </summary>
        /// <remarks>
        /// This method starts an operation with no complete action or user state. If this method
        /// returns normally, a <see cref="LoggedIn"/> event may be raised. Also, successful
        /// completion of this operation will update the <see cref="User"/>.
        /// </remarks>
        /// <param name="userName">The username associated with the user to authenticate</param>
        /// <param name="password">the password associated with the user to authenticate</param>
        /// <returns>Returns the login operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        /// <seealso cref="LoggedIn"/>
        public LoginOperation Login(string userName, string password)
        {
            return this.Login(new LoginParameters(userName, password));
        }

        /// <summary>
        /// Asynchronously authenticates and logs in to the server with the specified parameters.
        /// </summary>
        /// <remarks>
        /// This method starts an operation with no complete action or user state. If this method
        /// returns normally, a <see cref="LoggedIn"/> event may be raised. Also, successful
        /// completion of this operation will update the <see cref="User"/>.
        /// </remarks>
        /// <param name="parameters">Login parameters that specify the user to authenticate</param>
        /// <returns>Returns the login operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        /// <seealso cref="LoggedIn"/>
        public LoginOperation Login(LoginParameters parameters)
        {
            return this.Login(parameters, null, null);
        }

        /// <summary>
        /// Asynchronously authenticates and logs in to the server with the specified parameters.
        /// </summary>
        /// <remarks>
        /// If this method returns normally, a <see cref="LoggedIn"/> event may be raised. Also,
        /// successful completion of this operation will update the <see cref="User"/>.
        /// </remarks>
        /// <param name="parameters">Login parameters that specify the user to authenticate</param>
        /// <param name="completeAction">This action will be invoked immediately after the operation
        /// completes and is called in all cases including success, cancellation, and error. This
        /// parameter is optional.
        /// </param>
        /// <param name="userState">This state will be set into
        /// <see cref="OperationBase.UserState"/>. This parameter is optional.
        /// </param>
        /// <returns>Returns the login operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        /// <seealso cref="LoggedIn"/>
        public LoginOperation Login(LoginParameters parameters, Action<LoginOperation> completeAction, object userState)
        {
            this.StartOperation(
                new LoginOperation(this, parameters, this.WrapCompleteAction<LoginOperation>(completeAction), userState));

            return (LoginOperation)this.Operation;
        }

        /// <summary>
        /// Asynchronously logs an authenticated user out from the server.
        /// </summary>
        /// <remarks>
        /// This method starts an operation with no complete action or user state. If this method
        /// returns normally, a <see cref="LoggedOut"/> event may be raised. Also, successful
        /// completion of this operation will update the <see cref="User"/>.
        /// </remarks>
        /// <param name="throwOnError">True if an unhandled error should result in an exception, false otherwise.
        /// To handle an operation error, <see cref="OperationBase.MarkErrorAsHandled"/> can be called from the
        /// operation completion callback or from a <see cref="OperationBase.Completed"/> event handler.
        /// </param>
        /// <returns>Returns the logout operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        /// <seealso cref="LoggedOut"/>
        public LogoutOperation Logout(bool throwOnError)
        {
            var callback = !throwOnError ? AuthenticationService.HandleOperationError : (Action<LogoutOperation>)null;
            return this.Logout(callback, null);
        }

        /// <summary>
        /// Asynchronously logs an authenticated user out from the server.
        /// </summary>
        /// <remarks>
        /// If this method returns normally, a <see cref="LoggedOut"/> event may be raised. Also,
        /// successful completion of this operation will update the <see cref="User"/>.
        /// </remarks>
        /// <param name="completeAction">This action will be invoked immediately after the operation
        /// completes and is called in all cases including success, cancellation, and error. This
        /// parameter is optional.
        /// </param>
        /// <param name="userState">This state will be set into
        /// <see cref="OperationBase.UserState"/>. This parameter is optional.
        /// </param>
        /// <returns>Returns the logout operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        /// <seealso cref="LoggedOut"/>
        public LogoutOperation Logout(Action<LogoutOperation> completeAction, object userState)
        {
            this.StartOperation(
                new LogoutOperation(this, this.WrapCompleteAction<LogoutOperation>(completeAction), userState));

            return (LogoutOperation)this.Operation;
        }

        /// <summary>
        /// Asynchronously loads the authenticated user from the server.
        /// </summary>
        /// <remarks>
        /// This method starts an operation with no complete action or user state. Successful
        /// completion of this operation will update the <see cref="User"/>.
        /// </remarks>
        /// <returns>Returns the load user operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        public LoadUserOperation LoadUser()
        {
            return this.LoadUser(null, null);
        }

        /// <summary>
        /// Asynchronously loads the authenticated user from the server.
        /// </summary>
        /// <remarks>
        /// Successful completion of this operation will update the <see cref="User"/>.
        /// </remarks>
        /// <param name="completeAction">This action will be invoked immediately after the operation
        /// completes and is called in all cases including success, cancellation, and error. This
        /// parameter is optional.
        /// </param>
        /// <param name="userState">This state will be set into
        /// <see cref="OperationBase.UserState"/>. This parameter is optional.
        /// </param>
        /// <returns>Returns the load user operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        public LoadUserOperation LoadUser(Action<LoadUserOperation> completeAction, object userState)
        {
            this.StartOperation(
                new LoadUserOperation(this, this.WrapCompleteAction<LoadUserOperation>(completeAction), userState));

            return (LoadUserOperation)this.Operation;
        }

        /// <summary>
        /// Asynchronously saves the authenticated user to the server.
        /// </summary>
        /// <remarks>
        /// This method starts an operation with no complete action or user state.
        /// </remarks>
        /// <param name="throwOnError">True if an unhandled error should result in an exception, false otherwise.
        /// To handle an operation error, <see cref="OperationBase.MarkErrorAsHandled"/> can be called from the
        /// operation completion callback or from a <see cref="OperationBase.Completed"/> event handler.
        /// </param>
        /// <returns>Returns the save user operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        public SaveUserOperation SaveUser(bool throwOnError)
        {
            var callback = !throwOnError ? AuthenticationService.HandleOperationError : (Action<SaveUserOperation>)null;
            return this.SaveUser(callback, null);
        }

        /// <summary>
        /// Asynchronously saves the authenticated user to the server.
        /// </summary>
        /// <param name="completeAction">This action will be invoked immediately after the operation
        /// completes and is called in all cases including success, cancellation, and error. This
        /// parameter is optional.
        /// </param>
        /// <param name="userState">This state will be set into
        /// <see cref="OperationBase.UserState"/>. This parameter is optional.
        /// </param>
        /// <returns>Returns the save user operation.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        public SaveUserOperation SaveUser(Action<SaveUserOperation> completeAction, object userState)
        {
            this.StartOperation(
                new SaveUserOperation(this, this.WrapCompleteAction<SaveUserOperation>(completeAction), userState));

            return (SaveUserOperation)this.Operation;
        }

        /// <summary>
        /// Wraps the specified action so the <see cref="AuthenticationService"/> can complete
        /// processing of the operation before invoking the <paramref name="completeAction"/>.
        /// </summary>
        /// <typeparam name="T">The type of operation.</typeparam>
        /// <param name="completeAction">The action to invoke once the service finishes
        /// processing the operation. This parameter is optional.
        /// </param>
        /// <returns>An action that will complete processing of the operation before invoking
        /// the wrapped action.
        /// </returns>
        private Action<T> WrapCompleteAction<T>(Action<T> completeAction) where T : AuthenticationOperation
        {
            return new Action<T>(ao =>
                {
                    bool raiseUserChanged = false;
                    bool raiseLoggedIn = false;
                    bool raiseLoggedOut = false;

                    // If the operation completed successfully, update the user and 
                    // determine which events should be raised
                    if (!ao.IsCanceled && !ao.HasError && (ao.User != null))
                    {
                        if (this._user != ao.User)
                        {
                            raiseLoggedIn =
                                // anonymous -> authenticated
                                (this._user == null) ||
                                (!this._user.Identity.IsAuthenticated && ao.User.Identity.IsAuthenticated) ||
                                // authenticated -> authenticated
                                (ao.User.Identity.IsAuthenticated && (this._user.Identity.Name != ao.User.Identity.Name));
                            raiseLoggedOut =
                                // authenticated -> anonymous
                                (this._user != null) &&
                                (this._user.Identity.IsAuthenticated && !ao.User.Identity.IsAuthenticated);

                            this._user = ao.User;
                            raiseUserChanged = true;
                        }
                    }

                    // Setting the operation to null indicates the service is no longer busy and
                    // can process another operation
                    this.Operation = null;
                    
                    // Invoke the wrapped action
                    if (completeAction != null)
                    {
                        try
                        {
                            completeAction.DynamicInvoke(ao);
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

                    // Raise notification events as appropriate
                    if (raiseUserChanged)
                    {
                        this.RaisePropertyChanged("User");
                    }
                    this.RaisePropertyChanged("IsBusy");
                    this.RaisePropertyChanged(AuthenticationService.GetBusyPropertyName(ao));

                    if (raiseLoggedIn)
                    {
                        this.OnLoggedIn(new AuthenticationEventArgs(ao.User));
                    }
                    if (raiseLoggedOut)
                    {
                        this.OnLoggedOut(new AuthenticationEventArgs(ao.User));
                    }
                });
        }

        /// <summary>
        /// Starts an asynchronous operation if one is not already in progress
        /// </summary>
        /// <param name="operation">The operation to start</param>
        /// <exception cref="InvalidOperationException"> is returned if this method is called while
        /// another asynchronous operation is still being processed.
        /// </exception>
        private void StartOperation(AuthenticationOperation operation)
        {
            Debug.Assert(operation != null, "The operation cannot be null.");
            lock (this._syncLock)
            {
                if (this.IsBusy)
                {
                    throw new InvalidOperationException(Resources.ApplicationServices_UserServiceIsBusy);
                }
                this.Operation = operation;
            }

            try
            {
                operation.Start();
            }
            catch (Exception)
            {
                this.Operation = null;
                throw;
            }

            this.RaisePropertyChanged("IsBusy");
            this.RaisePropertyChanged(AuthenticationService.GetBusyPropertyName(this.Operation));
        }

        /// <summary>
        /// Returns the name of the "busy" property for the specified operation
        /// </summary>
        /// <param name="operation">The operation to get the property name for</param>
        /// <returns>The name of the "busy" property for the operation</returns>
        /// <seealso cref="IsLoggingIn"/>
        /// <seealso cref="IsLoggingOut"/>
        /// <seealso cref="IsLoadingUser"/>
        /// <seealso cref="IsSavingUser"/>
        private static string GetBusyPropertyName(AuthenticationOperation operation)
        {
            Debug.Assert(operation != null, "The operation cannot be null.");
            Type operationType = operation.GetType();

            if (typeof(LoginOperation) == operationType)
            {
                return "IsLoggingIn";
            }
            else if (typeof(LogoutOperation) == operationType)
            {
                return "IsLoggingOut";
            }
            else if (typeof(LoadUserOperation) == operationType)
            {
                return "IsLoadingUser";
            }
            else if (typeof(SaveUserOperation) == operationType)
            {
                return "IsSavingUser";
            }
            else
            {
                Debug.Assert(false, "operationType parameter is invalid.");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Runs the callback in the synchronization context
        /// </summary>
        /// <param name="callback">The callback to run</param>
        /// <param name="state">The state to pass to the callback</param>
        private void RunInSynchronizationContext(SendOrPostCallback callback, object state)
        {
            Debug.Assert(callback != null, "The callback cannot be null.");
            if (SynchronizationContext.Current == this._synchronizationContext)
            {
                // We're in the current context, just execute synchronously
                callback(state);
            }
            else
            {
                this._synchronizationContext.Post(callback, state);
            }
        }

        /// <summary>
        /// Raises a <see cref="INotifyPropertyChanged.PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The property to raise an event for</param>
        /// <exception cref="ArgumentNullException"> is thrown if the <paramref name="propertyName"/>
        /// is null.
        /// </exception>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises a <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The event to raise</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this._propertyChangedEventHandler;
            if (handler != null)
            {
                this.RunInSynchronizationContext(state => handler(this, e), null);
            }
        }

        /// <summary>
        /// Raises a <see cref="LoggedIn"/> event.
        /// </summary>
        /// <param name="e">The event to raise</param>
        private void OnLoggedIn(AuthenticationEventArgs e)
        {
            Debug.Assert(e != null, "The event args cannot be null.");
            EventHandler<AuthenticationEventArgs> handler = this.LoggedIn;
            if (handler != null)
            {
                this.RunInSynchronizationContext(state => handler(this, e), null);
            }
        }

        /// <summary>
        /// Raises a <see cref="LoggedOut"/> event.
        /// </summary>
        /// <param name="e">The event to raise</param>
        private void OnLoggedOut(AuthenticationEventArgs e)
        {
            Debug.Assert(e != null, "The event args cannot be null.");
            EventHandler<AuthenticationEventArgs> handler = this.LoggedOut;
            if (handler != null)
            {
                this.RunInSynchronizationContext(state => handler(this, e), null);
            }
        }

        /// <summary>
        /// Marks an operation in error as handled.
        /// </summary>
        /// <param name="ao">The operation in error.</param>
        private static void HandleOperationError(AuthenticationOperation ao)
        {
            if (ao.HasError)
            {
                ao.MarkErrorAsHandled();
            }
        }

        #endregion

        #region Template Properties

        /// <summary>
        /// Gets a value indicating whether this authentication implementation supports
        /// cancellation.
        /// </summary>
        /// <remarks>
        /// This value is <c>false</c> by default. When a derived class sets it to <c>true</c>,
        /// it is assumed that all cancellation methods have also been implemented.
        /// </remarks>
        protected internal virtual bool SupportsCancellation
        {
            get { return false; }
        }

        #endregion

        #region Template Methods

        /// <summary>
        /// Creates a default user.
        /// </summary>
        /// <remarks>
        /// This method will be invoked by <see cref="User"/> when it is accessed before the value
        /// is set. The returned value is then stored and returned on all subsequent gets until a
        /// <c>Login</c>, <c>Logout</c>, or <c>LoadUser</c> operation completes successfully.
        /// </remarks>
        /// <returns>A default user. This value may not be <c>null</c>.</returns>
        /// <exception cref="InvalidOperationException"> is thrown from <see cref="User"/> if
        /// this operation returns <c>null</c>.
        /// </exception>
        protected abstract IPrincipal CreateDefaultUser();

        /// <summary>
        /// Begins an asynchronous <c>Login</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <c>Login</c>. Exceptions thrown from this method will
        /// prevent the operation from starting and then be thrown from <c>Login</c>.
        /// </remarks>
        /// <param name="parameters">Login parameters that specify the user to authenticate. This
        /// parameter is optional.</param>
        /// <param name="callback">This callback should be invoked when the asynchronous call completes.
        /// If the asynchronous call is canceled, the callback should not be invoked. This parameter
        /// is optional.
        /// </param>
        /// <param name="state">The state should be set into the <see cref="IAsyncResult"/> this
        /// method returns. This parameter is optional.
        /// </param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call and
        /// will be passed to the cancel and end methods.
        /// </returns>
        /// <seealso cref="CancelLogin"/>
        /// <seealso cref="EndLogin"/>
        protected internal abstract IAsyncResult BeginLogin(LoginParameters parameters, AsyncCallback callback, object state);

        /// <summary>
        /// Cancels an asynchronous <c>Login</c> operation.
        /// </summary>
        /// <remarks>
        /// The default implementation throws a <see cref="NotSupportedException"/> when
        /// <see cref="SupportsCancellation"/> is <c>false</c> and does not need to be called from
        /// overridden implementations. This method is invoked when a <c>Login</c> operation is
        /// canceled. Either this or <see cref="EndLogin"/> will be invoked to conclude the operation
        /// but not both. After <see cref="CancelLogin"/> is called, the callback passed in to
        /// <see cref="BeginLogin"/> should not be invoked. Exceptions thrown from this method will
        /// be available in <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogin"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogin"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="OperationBase.Cancel()"/>
        /// <seealso cref="BeginLogin"/>
        /// <seealso cref="EndLogin"/>
        protected internal virtual void CancelLogin(IAsyncResult asyncResult)
        {
            if (!this.SupportsCancellation)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ApplicationServices_OperationCannotCancel,
                    "Login"));
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>Login</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked when a <c>Login</c> operation completes. Either this or
        /// <see cref="CancelLogin"/> will be invoked to conclude the operation but not both.
        /// Exceptions thrown from this method will be available in
        /// <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogin"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>Login</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogin"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="BeginLogin"/>
        /// <seealso cref="CancelLogin"/>
        protected internal abstract LoginResult EndLogin(IAsyncResult asyncResult);

        /// <summary>
        /// Begins an asynchronous <c>Logout</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <c>Logout</c>. Exceptions thrown from this method will
        /// prevent the operation from starting and then be thrown from <c>Logout</c>.
        /// </remarks>
        /// <param name="callback">This callback should be invoked when the asynchronous call completes.
        /// If the asynchronous call is canceled, the callback should not be invoked. This parameter
        /// is optional.
        /// </param>
        /// <param name="state">The state should be set into the <see cref="IAsyncResult"/> this
        /// method returns. This parameter is optional.
        /// </param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call and
        /// will be passed to the cancel and end methods.
        /// </returns>
        /// <seealso cref="CancelLogout"/>
        /// <seealso cref="EndLogout"/>
        protected internal abstract IAsyncResult BeginLogout(AsyncCallback callback, object state);

        /// <summary>
        /// Cancels an asynchronous <c>Logout</c> operation.
        /// </summary>
        /// <remarks>
        /// The default implementation throws a <see cref="NotSupportedException"/> when
        /// <see cref="SupportsCancellation"/> is <c>false</c> and does not need to be called from
        /// overridden implementations. This method is invoked when a <c>Logout</c> operation is
        /// canceled. Either this or <see cref="EndLogout"/> will be invoked to conclude the operation
        /// but not both. After <see cref="CancelLogout"/> is called, the callback passed in to
        /// <see cref="BeginLogout"/> should not be invoked. Exceptions thrown from this method will
        /// be available in <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogout"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogout"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="OperationBase.Cancel()"/>
        /// <seealso cref="BeginLogout"/>
        /// <seealso cref="EndLogout"/>
        protected internal virtual void CancelLogout(IAsyncResult asyncResult)
        {
            if (!this.SupportsCancellation)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ApplicationServices_OperationCannotCancel,
                    "Logout"));
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>Logout</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked when a <c>Logout</c> operation completes. Either this or
        /// <see cref="CancelLogout"/> will be invoked to conclude the operation but not both.
        /// Exceptions thrown from this method will be available in
        /// <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginLogout"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>Logout</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLogout"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="BeginLogout"/>
        /// <seealso cref="CancelLogout"/>
        protected internal abstract LogoutResult EndLogout(IAsyncResult asyncResult);

        /// <summary>
        /// Begins an asynchronous <c>LoadUser</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <c>LoadUser</c>. Exceptions thrown from this method will
        /// prevent the operation from starting and then be thrown from <c>LoadUser</c>.
        /// </remarks>
        /// <param name="callback">This callback should be invoked when the asynchronous call completes.
        /// If the asynchronous call is canceled, the callback should not be invoked. This parameter
        /// is optional.
        /// </param>
        /// <param name="state">The state should be set into the <see cref="IAsyncResult"/> this
        /// method returns. This parameter is optional.
        /// </param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call and
        /// will be passed to the cancel and end methods.
        /// </returns>
        /// <seealso cref="CancelLoadUser"/>
        /// <seealso cref="EndLoadUser"/>
        protected internal abstract IAsyncResult BeginLoadUser(AsyncCallback callback, object state);

        /// <summary>
        /// Cancels an asynchronous <c>LoadUser</c> operation.
        /// </summary>
        /// <remarks>
        /// The default implementation throws a <see cref="NotSupportedException"/> when
        /// <see cref="SupportsCancellation"/> is <c>false</c> and does not need to be called from
        /// overridden implementations. This method is invoked when a <c>LoadUser</c> operation is
        /// canceled. Either this or <see cref="EndLoadUser"/> will be invoked to conclude the operation
        /// but not both. After <see cref="CancelLoadUser"/> is called, the callback passed in to
        /// <see cref="BeginLoadUser"/> should not be invoked. Exceptions thrown from this method will
        /// be available in <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginLoadUser"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLoadUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="OperationBase.Cancel()"/>
        /// <seealso cref="BeginLoadUser"/>
        /// <seealso cref="EndLoadUser"/>
        protected internal virtual void CancelLoadUser(IAsyncResult asyncResult)
        {
            if (!this.SupportsCancellation)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ApplicationServices_OperationCannotCancel,
                    "LoadUser"));
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>LoadUser</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked when a <c>LoadUser</c> operation completes. Either this or
        /// <see cref="CancelLoadUser"/> will be invoked to conclude the operation but not both.
        /// Exceptions thrown from this method will be available in
        /// <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginLoadUser"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>LoadUser</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginLoadUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="BeginLoadUser"/>
        /// <seealso cref="CancelLoadUser"/>
        protected internal abstract LoadUserResult EndLoadUser(IAsyncResult asyncResult);

        /// <summary>
        /// Begins an asynchronous <c>SaveUser</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <c>SaveUser</c>. Exceptions thrown from this method will
        /// prevent the operation from starting and then be thrown from <c>SaveUser</c>.
        /// </remarks>
        /// <param name="user">The user to save. This parameter will not be null.</param>
        /// <param name="callback">This callback should be invoked when the asynchronous call completes.
        /// If the asynchronous call is canceled, the callback should not be invoked. This parameter
        /// is optional.
        /// </param>
        /// <param name="state">The state should be set into the <see cref="IAsyncResult"/> this
        /// method returns. This parameter is optional.
        /// </param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous call and
        /// will be passed to the cancel and end methods.
        /// </returns>
        /// <seealso cref="CancelSaveUser"/>
        /// <seealso cref="EndSaveUser"/>
        protected internal abstract IAsyncResult BeginSaveUser(IPrincipal user, AsyncCallback callback, object state);

        /// <summary>
        /// Cancels an asynchronous <c>SaveUser</c> operation.
        /// </summary>
        /// <remarks>
        /// The default implementation throws a <see cref="NotSupportedException"/> when
        /// <see cref="SupportsCancellation"/> is <c>false</c> and does not need to be called from
        /// overridden implementations. This method is invoked when a <c>SaveUser</c> operation is
        /// canceled. Either this or <see cref="EndSaveUser"/> will be invoked to conclude the operation
        /// but not both. After <see cref="CancelSaveUser"/> is called, the callback passed in to
        /// <see cref="BeginSaveUser"/> should not be invoked. Exceptions thrown from this method will
        /// be available in <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginSaveUser"/> that represents
        /// the asynchronous call to cancel.
        /// </param>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginSaveUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="OperationBase.Cancel()"/>
        /// <seealso cref="BeginSaveUser"/>
        /// <seealso cref="EndSaveUser"/>
        protected internal virtual void CancelSaveUser(IAsyncResult asyncResult)
        {
            if (!this.SupportsCancellation)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ApplicationServices_OperationCannotCancel,
                    "SaveUser"));
            }
        }

        /// <summary>
        /// Ends an asynchronous <c>SaveUser</c> operation.
        /// </summary>
        /// <remarks>
        /// This method is invoked when a <c>SaveUser</c> operation completes. Either this or
        /// <see cref="CancelSaveUser"/> will be invoked to conclude the operation but not both.
        /// Exceptions thrown from this method will be available in
        /// <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">A result returned from <see cref="BeginSaveUser"/> that represents
        /// the asynchronous call to conclude.
        /// </param>
        /// <returns>The result of the asynchronous <c>SaveUser</c> call</returns>
        /// <exception cref="InvalidOperationException"> is thrown if <paramref name="asyncResult"/>
        /// was not returned from <see cref="BeginSaveUser"/> or the asynchronous call has already been
        /// concluded with a previous call to cancel or end.
        /// </exception>
        /// <seealso cref="BeginSaveUser"/>
        /// <seealso cref="CancelSaveUser"/>
        protected internal abstract SaveUserResult EndSaveUser(IAsyncResult asyncResult);

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised every time a property value changes. See <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this._propertyChangedEventHandler = (PropertyChangedEventHandler)Delegate.Combine(this._propertyChangedEventHandler, value);
            }
            remove
            {
                this._propertyChangedEventHandler = (PropertyChangedEventHandler)Delegate.Remove(this._propertyChangedEventHandler, value);
            }
        }

        #endregion
    }
}
