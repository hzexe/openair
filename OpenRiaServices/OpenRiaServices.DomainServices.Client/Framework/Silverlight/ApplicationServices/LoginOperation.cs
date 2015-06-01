using System;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Operation type returned from <c>Login</c> operations on <see cref="AuthenticationService"/>.
    /// </summary>
    public sealed class LoginOperation : AuthenticationOperation
    {
        #region Member fields

        private Action<LoginOperation> _completeAction;
        private readonly LoginParameters _loginParameters;

        #endregion

        #region Constructors

        internal LoginOperation(AuthenticationService service, LoginParameters loginParameters, Action<LoginOperation> completeAction, object userState) :
            base(service, userState)
        {
            this._loginParameters = loginParameters;
            this._completeAction = completeAction;
        }

        #endregion

        #region Properties

        private new LoginResult Result
        {
            get { return (LoginResult)base.Result; }
        }

        /// <summary>
        /// Gets the login parameters used when invoking this operation.
        /// </summary>
        public LoginParameters LoginParameters
        {
            get { return this._loginParameters; }
        }

        /// <summary>
        /// Gets a value indicating whether this operation was able to successfully login.
        /// </summary>
        /// <remarks>
        /// This value will be <c>false</c> before the operation completes, if the operation
        /// is canceled, or if the operation has errors.
        /// </remarks>
        public bool LoginSuccess
        {
            get { return (this.Result == null) ? false : this.Result.LoginSuccess; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a login operation
        /// </summary>
        /// <param name="callback">The callback invoked when the operation completes</param>
        /// <returns>The async result for the operation</returns>
        protected override IAsyncResult BeginCore(AsyncCallback callback)
        {
            return this.Service.BeginLogin(this.LoginParameters, callback, null);
        }

        /// <summary>
        /// Cancels a login operation
        /// </summary>
        protected override void CancelCore()
        {
            this.Service.CancelLogin(this.AsyncResult);
        }

        /// <summary>
        /// Ends a login operation
        /// </summary>
        /// <param name="asyncResult">The async result for the operation</param>
        /// <returns>The result of the operation</returns>
        protected override object EndCore(IAsyncResult asyncResult)
        {
            return this.Service.EndLogin(asyncResult);
        }

        /// <summary>
        /// Raises property changes after the operation has completed.
        /// </summary>
        protected override void RaiseCompletionPropertyChanges()
        {
            base.RaiseCompletionPropertyChanges();
            if (this.LoginSuccess)
            {
                this.RaisePropertyChanged("LoginSuccess");
            }
        }

        /// <summary>
        /// Invoke the completion callback.
        /// </summary>
        protected override void InvokeCompleteAction()
        {
            if (this._completeAction != null)
            {
                this._completeAction(this);
            }
        }

        #endregion
    }
}
