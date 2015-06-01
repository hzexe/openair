using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel.Channels;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Internal <see cref="IAsyncResult"/> used during <see cref="WebDomainClient&lt;TContract&gt;"/> operations.
    /// </summary>
    /// <typeparam name="TContract">The contract type.</typeparam>
    internal sealed class WebDomainClientAsyncResult<TContract> : DomainClientAsyncResult where TContract : class
    {
        private IEnumerable<ChangeSetEntry> _changeSetEntries;
        private readonly TContract _channel;
        private readonly MethodInfo _endOperationMethod;

        /// <summary>
        /// Initializes a new <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> instance used for Query operations.
        /// </summary>
        /// <param name="domainClient">The <see cref="WebDomainClient&lt;TContract&gt;"/> associated with this result.</param>
        /// <param name="channel">The channel used to communicate with the server.</param>
        /// <param name="endOperationMethod">The method that completes an asynchronous operation.</param>
        /// <param name="callback">Optional <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="domainClient"/> is null.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="endOperationMethod"/> is null.</exception>
        private WebDomainClientAsyncResult(WebDomainClient<TContract> domainClient, TContract channel, MethodInfo endOperationMethod, AsyncCallback callback, object asyncState)
            : base(domainClient, callback, asyncState)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            if (endOperationMethod == null)
            {
                throw new ArgumentNullException("endOperationMethod");
            }

            this._endOperationMethod = endOperationMethod;
            this._channel = channel;
        }

        /// <summary>
        /// Initializes a new <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> instance used for Submit operations.
        /// </summary>
        /// <param name="domainClient">The <see cref="WebDomainClient&lt;TContract&gt;"/> associated with this result.</param>
        /// <param name="channel">The channel used to communicate with the server.</param>
        /// <param name="endOperationMethod">The method that completes an asynchronous operation.</param>
        /// <param name="entityChangeSet">The Submit operation <see cref="EntityChangeSet"/>.</param>
        /// <param name="changeSetEntries">The collection of <see cref="ChangeSetEntry"/>s to submit.</param>
        /// <param name="callback">Optional <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="domainClient"/> is null.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="endOperationMethod"/> is null.</exception>
        private WebDomainClientAsyncResult(WebDomainClient<TContract> domainClient, TContract channel, MethodInfo endOperationMethod, EntityChangeSet entityChangeSet, IEnumerable<ChangeSetEntry> changeSetEntries, AsyncCallback callback, object asyncState)
            : base(domainClient, entityChangeSet, callback, asyncState)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            if (endOperationMethod == null)
            {
                throw new ArgumentNullException("endOperationMethod");
            }

            this._endOperationMethod = endOperationMethod;
            this._channel = channel;
            this._changeSetEntries = changeSetEntries;
        }

        /// <summary>
        /// Initializes a new <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> instance used for Invoke operations.
        /// </summary>
        /// <param name="domainClient">The <see cref="WebDomainClient&lt;TContract&gt;"/> associated with this result.</param>
        /// <param name="channel">The channel used to communicate with the server.</param>
        /// <param name="endOperationMethod">The method that completes an asynchronous operation.</param>
        /// <param name="invokeArgs">The arguments to the Invoke operation.</param>
        /// <param name="callback">Optional <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="domainClient"/> is null.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="endOperationMethod"/> is null.</exception>
        private WebDomainClientAsyncResult(WebDomainClient<TContract> domainClient, TContract channel, MethodInfo endOperationMethod, InvokeArgs invokeArgs, AsyncCallback callback, object asyncState)
            : base(domainClient, invokeArgs, callback, asyncState)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            if (endOperationMethod == null)
            {
                throw new ArgumentNullException("endOperationMethod");
            }

            this._endOperationMethod = endOperationMethod;
            this._channel = channel;
        }

        /// <summary>
        /// Gets a collection of <see cref="ChangeSetEntry"/>s used with Submit operations.
        /// </summary>
        public IEnumerable<ChangeSetEntry> ChangeSetEntries
        {
            get
            {
                return this._changeSetEntries;
            }
        }

        /// <summary>
        /// Gets the channel used to communicate with the server.
        /// </summary>
        public TContract Channel
        {
            get
            {
                return this._channel;
            }
        }

        /// <summary>
        /// Gets the method that completes an asynchronous operation.
        /// </summary>
        public MethodInfo EndOperationMethod
        {
            get
            {
                return this._endOperationMethod;
            }
        }

        /// <summary>
        /// Creates a new <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> used for Query operations.
        /// </summary>
        /// <param name="domainClient">The <see cref="WebDomainClient&lt;TContract&gt;"/> associated with this result.</param>
        /// <param name="channel">The channel used to communicate with the server.</param>
        /// <param name="endOperationMethod">The method that completes an asynchronous operation.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <returns>A <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> used for Query operations</returns>
        public static WebDomainClientAsyncResult<TContract> CreateQueryResult(WebDomainClient<TContract> domainClient, TContract channel, MethodInfo endOperationMethod, AsyncCallback callback, object asyncState)
        {
            return new WebDomainClientAsyncResult<TContract>(domainClient, channel, endOperationMethod, callback, asyncState);
        }

        /// <summary>
        /// Creates a new <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> used for Submit operations.
        /// </summary>
        /// <param name="domainClient">The <see cref="WebDomainClient&lt;TContract&gt;"/> associated with this result.</param>
        /// <param name="channel">The channel used to communicate with the server.</param>
        /// <param name="endOperationMethod">The method that completes an asynchronous operation.</param>
        /// <param name="entityChangeSet">The Submit operation <see cref="EntityChangeSet"/>.</param>
        /// <param name="changeSetEntries">The collection of <see cref="ChangeSetEntry"/>s to submit.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <returns>A <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> used for Submit operations</returns>
        public static WebDomainClientAsyncResult<TContract> CreateSubmitResult(WebDomainClient<TContract> domainClient, TContract channel, MethodInfo endOperationMethod, EntityChangeSet entityChangeSet, IEnumerable<ChangeSetEntry> changeSetEntries, AsyncCallback callback, object asyncState)
        {
            return new WebDomainClientAsyncResult<TContract>(domainClient, channel, endOperationMethod, entityChangeSet, changeSetEntries, callback, asyncState);
        }

        /// <summary>
        /// Creates a new <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> used for Invoke operations.
        /// </summary>
        /// <param name="domainClient">The <see cref="WebDomainClient&lt;TContract&gt;"/> associated with this result.</param>
        /// <param name="channel">The channel used to communicate with the server.</param>
        /// <param name="endOperationMethod">The method that completes an asynchronous operation.</param>
        /// <param name="invokeArgs">The arguments to the Invoke operation.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <returns>A <see cref="WebDomainClientAsyncResult&lt;TContract&gt;"/> used for Invoke operations</returns>
        public static WebDomainClientAsyncResult<TContract> CreateInvokeResult(WebDomainClient<TContract> domainClient, TContract channel, MethodInfo endOperationMethod, InvokeArgs invokeArgs, AsyncCallback callback, object asyncState)
        {
            return new WebDomainClientAsyncResult<TContract>(domainClient, channel, endOperationMethod, invokeArgs, callback, asyncState);
        }

        /// <summary>
        /// Attempts to cancel this operation and aborts the underlying request if cancellation was successfully requested.
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();

            if (this.CancellationRequested)
            {
                ((IChannel)this._channel).Abort();
            }
        }
    }
}
