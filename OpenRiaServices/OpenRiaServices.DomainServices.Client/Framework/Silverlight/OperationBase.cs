using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Class representing an asynchronous operation.
    /// </summary>
    public abstract class OperationBase : INotifyPropertyChanged
    {
        private object _result;
        private Exception _error;
        private bool _canceled;
        private bool _completed;
        private object _userState;
        private PropertyChangedEventHandler _propChangedHandler;
        private EventHandler _completedEventHandler;
        private bool _isErrorHandled;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationBase"/> class.
        /// </summary>
        /// <param name="userState">Optional user state.</param>
        protected OperationBase(object userState)
        {
            this._userState = userState;
        }

        /// <summary>
        /// Gets a value indicating whether the operation error has been marked as
        /// handled by calling <see cref="MarkErrorAsHandled"/>.
        /// </summary>
        public bool IsErrorHandled
        {
            get 
            { 
                return this._isErrorHandled; 
            }
        }

        /// <summary>
        /// Event raised when the operation completes.
        /// </summary>
        public event EventHandler Completed
        {
            add
            {
                if (this.IsComplete)
                {
                    // if the operation has already completed, invoke the
                    // handler immediately
                    value(this, EventArgs.Empty);
                }
                else
                {
                    this._completedEventHandler = (EventHandler)Delegate.Combine(this._completedEventHandler, value);
                }
            }
            remove
            {
                this._completedEventHandler = (EventHandler)Delegate.Remove(this._completedEventHandler, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this operation supports cancellation.
        /// If overridden to return true, <see cref="CancelCore"/> must also be overridden.
        /// </summary>
        protected virtual bool SupportsCancellation
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this operation is currently in a state
        /// where it can be canceled.
        /// <remarks>If <see cref="SupportsCancellation"/> is false,
        /// this operation doesn't support cancellation, and <see cref="CanCancel"/>
        /// will always return false.</remarks>
        /// </summary>
        public bool CanCancel
        {
            get
            {
                // can be canceled if cancellation is supported and
                // the operation hasn't already completed
                return this.SupportsCancellation && !this._completed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this operation has been canceled.
        /// </summary>
        /// <remarks>
        /// Note that successful cancellation of this operation does not guarantee 
        /// state changes were prevented from happening on the server.
        /// </remarks>
        public bool IsCanceled
        {
            get
            {
                return this._canceled;
            }
        }

        /// <summary>
        /// Gets the operation error if the operation failed.
        /// </summary>
        public Exception Error
        {
            get
            {
                return this._error;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the operation has failed. If
        /// true, inspect the Error property for details.
        /// </summary>
        public bool HasError
        {
            get
            {
                return this._error != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this operation has completed.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return this._completed;
            }
        }

        /// <summary>
        /// Gets the result of the async operation.
        /// </summary>
        protected object Result
        {
            get
            {
                return this._result;
            }
        }

        /// <summary>
        /// Gets the optional user state for this operation.
        /// </summary>
        public object UserState
        {
            get
            {
                return this._userState;
            }
        }

        /// <summary>
        /// For an operation where <see cref="HasError"/> is <c>true</c>, this method marks the error as handled.
        /// If this method is not called for a failed operation, an exception will be thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if <see cref="HasError"/> is <c>false</c>.</exception>
        public void MarkErrorAsHandled()
        {
            if (this._error == null)
            {
                throw new InvalidOperationException(Resource.Operation_HasErrorMustBeTrue);
            }

            if (!this._isErrorHandled)
            {
                this._isErrorHandled = true;
                this.RaisePropertyChanged("IsErrorHandled");
            }
        }

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        /// <remarks>
        /// Upon completion of the operation, check the IsCanceled property to determine whether 
        /// or not the operation was successfully canceled. Note that successful cancellation
        /// does not guarantee state changes were prevented from happening on the server.
        /// </remarks>
        /// <exception cref="NotSupportedException"> is thrown when <see cref="SupportsCancellation"/>
        /// is <c>false</c>.
        /// </exception>
        public void Cancel()
        {
            if (!this.SupportsCancellation)
            {
                throw new NotSupportedException(Resources.AsyncOperation_CancelNotSupported);
            }

            this.EnsureNotCompleted();

            // must flag completion before callbacks or events are raised
            this._completed = true;
            this._canceled = true;
            
            // invoke the cancel action
            this.CancelCore();

            // callback is called even for a canceled operation
            this.InvokeCompleteAction();

            if (this._completedEventHandler != null)
            {
                this._completedEventHandler(this, EventArgs.Empty);
            }

            this.RaisePropertyChanged("IsCanceled");
            this.RaisePropertyChanged("CanCancel");
            this.RaisePropertyChanged("IsComplete");
        }

        /// <summary>
        /// Override this method to provide a Cancel implementation
        /// for operations that support cancellation.
        /// </summary>
        protected virtual void CancelCore()
        {
        }

        /// <summary>
        /// Successfully completes the operation.
        /// </summary>
        /// <param name="result">The operation result.</param>
        protected void Complete(object result)
        {
            this.EnsureNotCompleted();

            bool prevCanCancel = this.CanCancel;
            this._result = result;

            // must flag completion before callbacks or events are raised
            this._completed = true;

            this.InvokeCompleteAction();

            if (this._completedEventHandler != null)
            {
                this._completedEventHandler(this, EventArgs.Empty);
            }

            this.RaisePropertyChanged("IsComplete");
            if (prevCanCancel == true)
            {
                this.RaisePropertyChanged("CanCancel");
            }
        }

        /// <summary>
        /// Completes the operation with the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        protected void Complete(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            this.EnsureNotCompleted();

            bool prevCanCancel = this.CanCancel;
            this._error = error;

            // must flag completion before callbacks or events are raised
            this._completed = true;

            // callback is called even in error case
            this.InvokeCompleteAction();

            if (this._completedEventHandler != null)
            {
                this._completedEventHandler(this, EventArgs.Empty);
            }

            this.RaisePropertyChanged("Error");
            this.RaisePropertyChanged("HasError");
            this.RaisePropertyChanged("IsComplete");
            if (prevCanCancel == true)
            {
                this.RaisePropertyChanged("CanCancel");
            }

            if (!this.IsErrorHandled)
            {
                //throw error;
            }
        }

        /// <summary>
        /// Invoke the completion callback.
        /// </summary>
        protected abstract void InvokeCompleteAction();

        /// <summary>
        /// Ensures an operation has not been completed or canceled. If
        /// it has been completed, an exception is thrown.
        /// </summary>
        private void EnsureNotCompleted()
        {
            if (this._completed)
            {
                throw new InvalidOperationException(Resources.AsyncOperation_AlreadyCompleted);
            }
        }

        /// <summary>
        /// Called when an property has changed on the operation.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this._propChangedHandler != null)
            {
                this._propChangedHandler(this, e);
            }
        }

        /// <summary>
        /// Called to raise the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this._propChangedHandler = (PropertyChangedEventHandler)Delegate.Combine(this._propChangedHandler, value);
            }
            remove
            {
                this._propChangedHandler = (PropertyChangedEventHandler)Delegate.Remove(this._propChangedHandler, value);
            }
        }

        #endregion
    }
}
