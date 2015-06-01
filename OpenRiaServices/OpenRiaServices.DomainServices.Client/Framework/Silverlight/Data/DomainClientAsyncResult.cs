using System;
using System.Collections.Generic;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Internal enumeration used to qualify operation results.
    /// </summary>
    internal enum AsyncOperationType
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None,

        /// <summary>
        /// An invoke operation.
        /// </summary>
        Invoke,

        /// <summary>
        /// A query operation.
        /// </summary>
        Query,

        /// <summary>
        /// A submit operation.
        /// </summary>
        Submit
    }

    /// <summary>
    /// Internal <see cref="IAsyncResult"/> used during <see cref="DomainClient"/> operations.
    /// </summary>
    internal class DomainClientAsyncResult : AsyncResultBase
    {
        private readonly AsyncOperationType _asyncOperationType;
        private readonly DomainClient _domainClient;
        private readonly EntityChangeSet _entityChangeSet;
        private readonly InvokeArgs _invokeArgs;

        /// <summary>
        /// Initializes a new <see cref="DomainClientAsyncResult"/> instance used for Query operations.
        /// </summary>
        /// <param name="domainClient">The associated <see cref="DomainClient"/>.</param>
        /// <param name="callback">Optional <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="domainClient"/> is null.</exception>
        protected DomainClientAsyncResult(DomainClient domainClient, AsyncCallback callback, object asyncState)
            : base(callback, asyncState)
        {
            if (domainClient == null)
            {
                throw new ArgumentNullException("domainClient");
            }

            this._asyncOperationType = AsyncOperationType.Query;
            this._domainClient = domainClient;
        }

        /// <summary>
        /// Initializes a new <see cref="DomainClientAsyncResult"/> instance used for Submit operations.
        /// </summary>
        /// <param name="domainClient">The associated <see cref="DomainClient"/>.</param>
        /// <param name="entityChangeSet">The Submit operation <see cref="EntityChangeSet"/>.</param>
        /// <param name="callback">Optional <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="domainClient"/> is null.</exception>
        protected DomainClientAsyncResult(DomainClient domainClient, EntityChangeSet entityChangeSet, AsyncCallback callback, object asyncState)
            : base(callback, asyncState)
        {
            if (domainClient == null)
            {
                throw new ArgumentNullException("domainClient");
            }

            this._asyncOperationType = AsyncOperationType.Submit;
            this._domainClient = domainClient;
            this._entityChangeSet = entityChangeSet;
        }

        /// <summary>
        /// Initializes a new <see cref="DomainClientAsyncResult"/> instance used for Invoke operations.
        /// </summary>
        /// <param name="domainClient">The associated <see cref="DomainClient"/>.</param>
        /// <param name="invokeArgs">The arguments to the Invoke operation.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="domainClient"/> is null.</exception>
        protected DomainClientAsyncResult(DomainClient domainClient, InvokeArgs invokeArgs, AsyncCallback callback, object asyncState)
            : base(callback, asyncState)
        {
            if (domainClient == null)
            {
                throw new ArgumentNullException("domainClient");
            }
            if (invokeArgs == null)
            {
                throw new ArgumentNullException("invokeArgs");
            }

            this._asyncOperationType = AsyncOperationType.Invoke;
            this._domainClient = domainClient;
            this._invokeArgs = invokeArgs;
        }

        /// <summary>
        /// Gets the <see cref="DomainClient"/> associated with this result.
        /// </summary>
        public DomainClient DomainClient
        {
            get
            {
                return this._domainClient;
            }
        }

        /// <summary>
        /// Gets the <see cref="EntityChangeSet"/> used with Submit operations.
        /// </summary>
        public EntityChangeSet EntityChangeSet
        {
            get
            {
                return this._entityChangeSet;
            }
        }

        /// <summary>
        /// Gets the arguments for Invoke operations.
        /// </summary>
        public InvokeArgs InvokeArgs
        {
            get
            {
                return this._invokeArgs;
            }
        }

        /// <summary>
        /// Gets the <see cref="AsyncOperationType"/> describing this operation.
        /// </summary>
        public AsyncOperationType AsyncOperationType
        {
            get
            {
                return this._asyncOperationType;
            }
        }

        /// <summary>
        /// Creates a new <see cref="DomainClientAsyncResult"/> used for Query operations.
        /// </summary>
        /// <param name="domainClient">The <see cref="DomainClient"/> associated with this result.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <returns>A <see cref="DomainClientAsyncResult"/> used for Query operations</returns>
        public static DomainClientAsyncResult CreateQueryResult(DomainClient domainClient, AsyncCallback callback, object asyncState)
        {
            return new DomainClientAsyncResult(domainClient, callback, asyncState);
        }

        /// <summary>
        /// Creates a new <see cref="DomainClientAsyncResult"/> used for Submit operations.
        /// </summary>
        /// <param name="domainClient">The associated <see cref="DomainClient"/>.</param>
        /// <param name="entityChangeSet">The Submit operation <see cref="EntityChangeSet"/>.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <returns>A <see cref="DomainClientAsyncResult"/> used for Submit operations</returns>
        public static DomainClientAsyncResult CreateSubmitResult(DomainClient domainClient, EntityChangeSet entityChangeSet, AsyncCallback callback, object asyncState)
        {
            return new DomainClientAsyncResult(domainClient, entityChangeSet, callback, asyncState);
        }

        /// <summary>
        /// Creates a new <see cref="DomainClientAsyncResult"/> used for Invoke operations.
        /// </summary>
        /// <param name="domainClient">The associated <see cref="DomainClient"/>.</param>
        /// <param name="invokeArgs">The arguments to the Invoke operation.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to invoke upon completion.</param>
        /// <param name="asyncState">Optional user state information that will be passed to the <paramref name="callback"/>.</param>
        /// <returns>A <see cref="DomainClientAsyncResult"/> used for Invoke operations</returns>
        public static DomainClientAsyncResult CreateInvokeResult(DomainClient domainClient, InvokeArgs invokeArgs, AsyncCallback callback, object asyncState)
        {
            return new DomainClientAsyncResult(domainClient, invokeArgs, callback, asyncState);
        }
    }
}