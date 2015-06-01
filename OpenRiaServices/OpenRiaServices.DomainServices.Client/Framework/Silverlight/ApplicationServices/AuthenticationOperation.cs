using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Abstract subclass of the <see cref="OperationBase"/> class
    /// that is the base operation type for all the operations supported
    /// by <see cref="AuthenticationService"/>.
    /// </summary>
    public abstract class AuthenticationOperation : OperationBase
    {
        #region Member fields

        // By default, events will be dispatched to the context the service is created in
        private readonly SynchronizationContext _synchronizationContext =
            SynchronizationContext.Current ?? new SynchronizationContext();

        private IAsyncResult _asyncResult;

        private readonly AuthenticationService _service;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOperation"/> class.
        /// </summary>
        /// <param name="service">The service this operation will use to implement Begin, Cancel, and End</param>
        /// <param name="userState">Optional user state.</param>
        internal AuthenticationOperation(AuthenticationService service, object userState) :
            base(userState)
        {
            Debug.Assert(service != null, "The service cannot be null.");
            this._service = service;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the async result returned from <see cref="BeginCore"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="BeginCore"/> has not been called, this may be <c>null</c>.
        /// </remarks>
        protected IAsyncResult AsyncResult
        {
            get { return this._asyncResult; }
        }

        /// <summary>
        /// Gets the service this operation will use to implement Begin, Cancel, and End.
        /// </summary>
        protected AuthenticationService Service
        {
            get { return this._service; }
        }

        /// <summary>
        /// Gets a value that indicates whether the operation supports cancellation.
        /// </summary>
        protected override bool SupportsCancellation
        {
            get { return this.Service.SupportsCancellation; }
        }

        /// <summary>
        /// Gets the result as an <see cref="AuthenticationResult"/>.
        /// </summary>
        protected new AuthenticationResult Result
        {
            get { return (AuthenticationResult)base.Result; }
        }

        /// <summary>
        /// Gets the user principal.
        /// </summary>
        /// <remarks>
        /// This value will be <c>null</c> before the operation completes, if the operation
        /// is canceled, or if the operation has errors.
        /// </remarks>
        public IPrincipal User
        {
            get { return (this.Result == null) ? null : this.Result.User; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the operation.
        /// </summary>
        /// <remarks>
        /// This method will invoke <see cref="BeginCore"/> and will allow all
        /// exceptions thrown from <see cref="BeginCore"/> to pass through.
        /// </remarks>
        internal void Start()
        {
            this._asyncResult = this.BeginCore(this.HandleAsyncCompleted);
        }

        /// <summary>
        /// Template method for invoking the corresponding Begin method in the
        /// underlying async result implementation.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <see cref="Start"/>. Any exceptions thrown
        /// will be passed through.
        /// </remarks>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke when the
        /// async call has completed. This can be passed directly to the underlying
        /// Begin method.
        /// </param>
        /// <returns>The async result returned by the underlying Begin call</returns>
        protected abstract IAsyncResult BeginCore(AsyncCallback callback);

        /// <summary>
        /// Handles completion of the underlying async call.
        /// </summary>
        /// <remarks>
        /// If <see cref="AsyncResult"/> is <c>null</c> (as in the case of a synchronous
        /// completion), it will be set to <paramref name="asyncResult"/>.
        /// </remarks>
        /// <param name="asyncResult">The async result returned by the underlying Begin call</param>
        private void HandleAsyncCompleted(IAsyncResult asyncResult)
        {
            if (this._asyncResult == null)
            {
                this._asyncResult = asyncResult;
            }
            this.RunInSynchronizationContext(state => this.End(asyncResult), null);
        }

        /// <summary>
        /// Completes the operation.
        /// </summary>
        /// <remarks>
        /// This method will invoke <see cref="EndCore"/> and will funnel all
        /// exceptions throw from <see cref="EndCore"/> into the operation.
        /// </remarks>
        /// <param name="result">The async result returned by the underlying Begin call</param>
        private void End(IAsyncResult result)
        {
            object endResult = null;
            try
            {
                endResult = this.EndCore(result);
            }
            catch (Exception e)
            {
                if (e.IsFatal())
                {
                    throw;
                }
                this.Complete(e);
                this.RaiseCompletionPropertyChanges();
                return;
            }

            this.Complete(endResult);
            this.RaiseCompletionPropertyChanges();
        }

        /// <summary>
        /// Template method for invoking the corresponding End method in the
        /// underlying async result implementation.
        /// </summary>
        /// <remarks>
        /// This method is invoked by the callback passed into <see cref="BeginCore"/>.
        /// Any exceptions thrown will be captured in the <see cref="OperationBase.Error"/>.
        /// </remarks>
        /// <param name="asyncResult">The async result returned by the underlying Begin call</param>
        /// <returns>The result of the End call to store in <see cref="OperationBase.Result"/></returns>
        protected abstract object EndCore(IAsyncResult asyncResult);

        /// <summary>
        /// Runs the callback in the synchronization context the operation was created in.
        /// </summary>
        /// <param name="callback">The callback to run in the synchronization context</param>
        /// <param name="state">The optional state to pass to the callback</param>
        private void RunInSynchronizationContext(SendOrPostCallback callback, object state)
        {
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
        /// Raises property changes after the operation has completed.
        /// </summary>
        /// <remarks>
        /// This method is invoked by the callback passed into <see cref="BeginCore"/> once
        /// <see cref="OperationBase.Result"/> and <see cref="OperationBase.Error"/> have
        /// been set. Change notifications for any properties that have been affected by the
        /// state changes should occur here.
        /// </remarks>
        protected virtual void RaiseCompletionPropertyChanges()
        {
            if (this.User != null)
            {
                this.RaisePropertyChanged("User");
            }
        }

        #endregion
    }
}
