using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Windows;

namespace OpenRiaServices.DomainServices.Client.ApplicationServices
{
    /// <summary>
    /// Context for the application.
    /// </summary>
    /// <remarks>
    /// This context makes services and other values available from code and xaml. When
    /// a context is created, it should be registered as a lifetime-object with
    /// <see cref="Application.ApplicationLifetimeObjects"/> (Silverlight specific).
    /// </remarks>
    public abstract class WebContextBase : INotifyPropertyChanged
#if SILVERLIGHT
        , IApplicationService, IApplicationLifetimeAware
#endif
    {
        #region Static fields

        private static WebContextBase current;

        #endregion

        #region Member fields

        private PropertyChangedEventHandler _propertyChangedEventHandler;
        private bool _started;
        private AuthenticationService _authentication;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebContextBase"/> class.
        /// </summary>
        /// <remarks>
        /// This first context that is created will become the current instance. If more than
        /// one context is created, an exception will be thrown.
        /// </remarks>
        /// <exception cref="InvalidOperationException"> is thrown if the constructor is invoked
        /// when <see cref="WebContextBase.Current"/> is valid.
        /// </exception>
        protected WebContextBase() : this(true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebContextBase"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if the constructor is invoked
        /// when <see cref="WebContextBase.Current"/> is valid and <paramref name="setAsCurrent"/>
        /// is <c>true</c>.
        /// </exception>
        /// <param name="setAsCurrent">Whether to use this context as the current instance</param>
        internal WebContextBase(bool setAsCurrent)
        {
            if (setAsCurrent)
            {
                if (WebContextBase.current != null)
                {
                    throw new InvalidOperationException(Resources.WebContext_OnlyOne);
                }
                WebContextBase.current = this;
            }

            this._authentication = new DefaultAuthentication();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current context.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if no contexts have been added.
        /// </exception>
        public static WebContextBase Current
        {
            get 
            {
                if (WebContextBase.current == null)
                {
                    throw new InvalidOperationException(Resources.WebContext_NoContexts);
                }
                return WebContextBase.current;
            }
        }

        /// <summary>
        /// Gets a principal representing the authenticated identity.
        /// </summary>
        /// <remarks>
        /// This value is the same one available in <see cref="AuthenticationService.User"/>.
        /// </remarks>
        protected IPrincipal User
        {
            get { return this._authentication.User; }
        }

        /// <summary>
        /// Gets or sets the authentication context for the application.
        /// </summary>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public AuthenticationService Authentication
        {
            get
            {
                return this._authentication;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this._started)
                {
                    throw new InvalidOperationException(Resources.WebContext_CannotModifyAuthentication);
                }
#if !SILVERLIGHT
                // Since IApplicationLifetimeAware.Started() will only be called from silverlight
                // execute the code here instead for now
                _started = true;
#endif

                if (this._authentication != value)
                {
                    ((INotifyPropertyChanged)this._authentication).PropertyChanged -= this.AuthenticationService_PropertyChanged;
                    this._authentication = value;
                    ((INotifyPropertyChanged)this._authentication).PropertyChanged += this.AuthenticationService_PropertyChanged;
                    this.RaisePropertyChanged("Authentication");
                    this.RaisePropertyChanged("User");
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises an <see cref="INotifyPropertyChanged.PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The property to raise an event for</param>
        /// <exception cref="ArgumentNullException"> is thrown if the 
        /// <paramref name="propertyName"/> is <c>null</c>.
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
        /// Raises an <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The event to raise</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this._propertyChangedEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Handles property changed events raised by the <see cref="Authentication"/> context.
        /// </summary>
        /// <param name="sender">The authentication context</param>
        /// <param name="e">The event that occurred</param>
        private void AuthenticationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "User")
            {
                this.RaisePropertyChanged("User");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this._propertyChangedEventHandler += value; }
            remove { this._propertyChangedEventHandler -= value; }
        }

        #endregion

#if SILVERLIGHT
        #region IApplicationService Members

        /// <summary>
        /// Starts the context as an application service
        /// </summary>
        /// <param name="context">The service context</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "There isn't a scenario where this behavior would need to be customized in a derived class.")]
        void IApplicationService.StartService(ApplicationServiceContext context)
        {
        }

        /// <summary>
        /// Stops the context as an application service
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "There isn't a scenario where this behavior would need to be customized in a derived class.")]
        void IApplicationService.StopService()
        {
        }

        #endregion

        #region IApplicationLifetimeAware Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "There isn't a scenario where this behavior would need to be customized in a derived class.")]
        void IApplicationLifetimeAware.Starting()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "There isn't a scenario where this behavior would need to be customized in a derived class.")]
        void IApplicationLifetimeAware.Started()
        {
            this._started = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "There isn't a scenario where this behavior would need to be customized in a derived class.")]
        void IApplicationLifetimeAware.Exiting()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "There isn't a scenario where this behavior would need to be customized in a derived class.")]
        void IApplicationLifetimeAware.Exited()
        {
        }

        #endregion
#endif
        #region Nested Classes

        private class DefaultIdentity : IIdentity
        {
            public string AuthenticationType
            {
                get { return string.Empty; }
            }

            public bool IsAuthenticated
            {
                get { return false; }
            }

            public string Name
            {
                get { return string.Empty; }
            }
        }

        private class DefaultPrincipal : IPrincipal
        {
            private readonly IIdentity _identity = new DefaultIdentity();

            public IIdentity Identity
            {
                get { return this._identity; }
            }

            public bool IsInRole(string role)
            {
                return false;
            }
        }

        private class DefaultAuthentication : AuthenticationService
        {
            protected override IPrincipal CreateDefaultUser()
            {
                return new DefaultPrincipal();
            }

            protected override IAsyncResult BeginLogin(LoginParameters parameters, AsyncCallback callback, object state)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }

            protected override LoginResult EndLogin(IAsyncResult asyncResult)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }

            protected override IAsyncResult BeginLogout(AsyncCallback callback, object state)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }

            protected override LogoutResult EndLogout(IAsyncResult asyncResult)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }

            protected override IAsyncResult BeginLoadUser(AsyncCallback callback, object state)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }

            protected override LoadUserResult EndLoadUser(IAsyncResult asyncResult)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }

            protected override IAsyncResult BeginSaveUser(IPrincipal user, AsyncCallback callback, object state)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }

            protected override SaveUserResult EndSaveUser(IAsyncResult asyncResult)
            {
                throw new NotSupportedException(Resources.WebContext_AuthenticationNotSet);
            }
        }

        #endregion
    }
}
