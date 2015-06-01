using System;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Operation type returned from <c>SaveUser</c> operations on <see cref="AuthenticationService"/>.
    /// </summary>
    public sealed class SaveUserOperation : AuthenticationOperation
    {
        #region Member fields

        private Action<SaveUserOperation> _completeAction;

        #endregion

        #region Constructors

        internal SaveUserOperation(AuthenticationService service, Action<SaveUserOperation> completeAction, object userState) :
            base(service, userState)
        {
            this._completeAction = completeAction;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a save operation
        /// </summary>
        /// <param name="callback">The callback invoked when the operation completes</param>
        /// <returns>The async result for the operation</returns>
        protected override IAsyncResult BeginCore(AsyncCallback callback)
        {
            return this.Service.BeginSaveUser(this.Service.User, callback, null);
        }

        /// <summary>
        /// Cancels a save operation
        /// </summary>
        protected override void CancelCore()
        {
            this.Service.CancelSaveUser(this.AsyncResult);
        }

        /// <summary>
        /// Ends a save operation
        /// </summary>
        /// <param name="asyncResult">The async result for the operation</param>
        /// <returns>The result of the operation</returns>
        protected override object EndCore(IAsyncResult asyncResult)
        {
            return this.Service.EndSaveUser(asyncResult);
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
