using System;
using System.Threading;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Internal type used as a common <see cref="IAsyncResult"/> base for all cancellable asynchronous operations.
    /// </summary>
    internal abstract class AsyncResultBase : IAsyncResult
    {
        private readonly object _syncLock = new object();
        private readonly AsyncCallback _callback;
        private readonly object _asyncState;
        private IAsyncResult _innerAsyncResult;
        private bool _endMethodInvoked;
        private bool _isCancellationRequested;
        private bool _isCompleted;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="callback">Optional callback to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state to pass to <paramref name="callback"/>.</param>
        public AsyncResultBase(AsyncCallback callback, object asyncState)
        {
            this._callback = callback;
            this._asyncState = asyncState;
        }

        /// <summary>
        /// Gets the optional user state.
        /// </summary>
        public object AsyncState
        {
            get
            {
                return this._asyncState;
            }
        }

        /// <summary>
        /// Gets the assoicated <see cref="WaitHandle"/>.  Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException"> is always thrown.</exception>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the asynchronous operation completed synchronously.
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the asynchronous operation is complete.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return this._isCompleted;
            }
        }

        /// <summary>
        /// Gets a value indicating whether cancellation was requested.
        /// </summary>
        public bool CancellationRequested
        {
            get
            {
                return this._isCancellationRequested;
            }
        }

        /// <summary>
        /// Gets the inner <see cref="IAsyncResult"/>.
        /// </summary>
        public IAsyncResult InnerAsyncResult
        {
            get
            {
                return this._innerAsyncResult;
            }
            set
            {
                this._innerAsyncResult = value;
            }
        }

        /// <summary>
        /// Transitions an <see cref="IAsyncResult"/> instance to mark the operation as ended.
        /// </summary>
        /// <typeparam name="TAsyncResult">The <see cref="AsyncResultBase"/> type expected.</typeparam>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> to examine.</param>
        /// <returns>The <paramref name="asyncResult"/> cast as the <typeparamref name="TAsyncResult"/> type expected.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="asyncResult"/> is null.</exception>
        /// <exception cref="ArgumentException"> if <paramref name="asyncResult"/> is not of type <typeparamref name="TAsyncResult"/>.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/> has been canceled.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/>'s End* method has already been invoked.</exception>
        public static TAsyncResult EndAsyncOperation<TAsyncResult>(IAsyncResult asyncResult) where TAsyncResult : AsyncResultBase
        {
            return AsyncResultBase.EndAsyncOperation<TAsyncResult>(asyncResult, false);
        }

        /// <summary>
        /// Transitions an <see cref="IAsyncResult"/> instance to mark the operation as ended (and optionally canceled).
        /// </summary>
        /// <typeparam name="TAsyncResult">The <see cref="AsyncResultBase"/> type expected.</typeparam>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> to examine.</param>
        /// <param name="cancel">Indicates whether the operation should be canceled.</param>
        /// <returns>The <paramref name="asyncResult"/> cast as the <typeparamref name="TAsyncResult"/> type expected.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="asyncResult"/> is null.</exception>
        /// <exception cref="ArgumentException"> if <paramref name="asyncResult"/> is not of type <typeparamref name="TAsyncResult"/>.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/> has been canceled.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/>'s End* method has already been invoked.</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="asyncResult"/> has not completed.</exception>
        public static TAsyncResult EndAsyncOperation<TAsyncResult>(IAsyncResult asyncResult, bool cancel) where TAsyncResult : AsyncResultBase
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            TAsyncResult asyncResultBase = asyncResult as TAsyncResult;
            if (asyncResultBase == null)
            {
                throw new ArgumentException(Resources.WrongAsyncResult, "asyncResult");
            }

            if (asyncResultBase.CancellationRequested)
            {
                throw new InvalidOperationException(Resources.OperationCancelled);
            }

            if (cancel)
            {
                asyncResultBase.Cancel();
            }
            else if (!asyncResultBase.IsCompleted)
            {
                throw new InvalidOperationException(Resources.OperationNotComplete);
            }

            asyncResultBase.SetEndMethodInvokedFlag();

            return asyncResultBase;
        }

        /// <summary>
        /// Signals that the asynchronous operation has completed and invokes the callback.
        /// </summary>
        /// <exception cref="InvalidOperationException"> if Complete has already been called.</exception>
        public void Complete()
        {
            lock (this._syncLock)
            {
                if (this._isCompleted)
                {
                    throw new InvalidOperationException(Resources.MethodCanOnlyBeInvokedOnce);
                }

                this._isCompleted = true;
            }

            if (this._callback != null)
            {
                this._callback(this);
            }
        }

        /// <summary>
        /// Signals a cancellation request for this operation.
        /// </summary>
        /// <remarks>
        /// If this operation is completing or has already completed, this method returns without modifying any state.
        /// Cancellation is not guaranteed.
        /// </remarks>
        public virtual void Cancel()
        {
            lock (this._syncLock)
            {
                if (!this._endMethodInvoked)
                {
                    this._isCancellationRequested = true;
                }
            }
        }

        /// <summary>
        /// Signals that the asynchronous EndXxx method has been invoked.
        /// </summary>
        /// <exception cref="InvalidOperationException"> if SetEndMethodInvokedFlag has already been called.</exception>
        private void SetEndMethodInvokedFlag()
        {
            lock (this._syncLock)
            {
                if (this._endMethodInvoked)
                {
                    throw new InvalidOperationException(Resources.MethodCanOnlyBeInvokedOnce);
                }

                this._endMethodInvoked = true;
            }
        }
    }
}