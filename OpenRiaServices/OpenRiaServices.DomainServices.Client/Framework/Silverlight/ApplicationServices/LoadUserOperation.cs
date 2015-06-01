using System;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Operation type returned from <c>LoadUser</c> operations on <see cref="AuthenticationService"/>.
    /// </summary>
    public sealed class LoadUserOperation : AuthenticationOperation
    {
        #region Member fields

        private Action<LoadUserOperation> _completeAction;

        #endregion

        #region Constructors

        internal LoadUserOperation(AuthenticationService service, Action<LoadUserOperation> completeAction, object userState) :
            base(service, userState)
        {
            this._completeAction = completeAction;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a load operation
        /// </summary>
        /// <param name="callback">The callback invoked when the operation completes</param>
        /// <returns>The async result for the operation</returns>
        protected override IAsyncResult BeginCore(AsyncCallback callback)
        {
            return this.Service.BeginLoadUser(callback, null);
        }

        /// <summary>
        /// Cancels a load operation
        /// </summary>
        protected override void CancelCore()
        {
            this.Service.CancelLoadUser(this.AsyncResult);
        }

        /// <summary>
        /// Ends a load operation
        /// </summary>
        /// <param name="asyncResult">The async result for the operation</param>
        /// <returns>The result of the operation</returns>
        protected override object EndCore(IAsyncResult asyncResult)
        {
            return this.Service.EndLoadUser(asyncResult);
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
