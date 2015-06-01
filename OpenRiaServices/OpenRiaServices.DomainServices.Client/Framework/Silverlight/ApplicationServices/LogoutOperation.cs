using System;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Operation type returned from <c>Logout</c> operations on <see cref="AuthenticationService"/>.
    /// </summary>
    public sealed class LogoutOperation : AuthenticationOperation
    {
        #region Member fields

        private Action<LogoutOperation> _completeAction;

        #endregion

        #region Constructors

        internal LogoutOperation(AuthenticationService service, Action<LogoutOperation> completeAction, object userState)
            : base(service, userState)
        {
            this._completeAction = completeAction;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a logout operation
        /// </summary>
        /// <param name="callback">The callback invoked when the operation completes</param>
        /// <returns>The async result for the operation</returns>
        protected override IAsyncResult BeginCore(AsyncCallback callback)
        {
            return this.Service.BeginLogout(callback, null);
        }

        /// <summary>
        /// Cancels a logout operation
        /// </summary>
        protected override void CancelCore()
        {
            this.Service.CancelLogout(this.AsyncResult);
        }

        /// <summary>
        /// Ends a logout operation
        /// </summary>
        /// <param name="asyncResult">The async result for the operation</param>
        /// <returns>The result of the operation</returns>
        protected override object EndCore(IAsyncResult asyncResult)
        {
            return this.Service.EndLogout(asyncResult);
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
