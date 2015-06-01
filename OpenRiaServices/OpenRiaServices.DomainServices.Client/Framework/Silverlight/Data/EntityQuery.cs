using System;

namespace OpenRiaServices.DomainServices.Client
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Class representing a query method invocation. LINQ query operators can
    /// also be applied to the query.
    /// </summary>
    public abstract class EntityQuery
    {
        private string _queryName;
        private Type _entityType;
        private IDictionary<string, object> _parameters;
        private bool _hasSideEffects;
        private bool _isComposable;
        private bool _includeTotalCount;
        private IQueryable _query;
        private DomainClient _domainClient;

        /// <summary>
        /// Initializes a new instance of the EntityQuery class
        /// </summary>
        /// <param name="domainClient">The <see cref="DomainClient"/> for the query.</param>
        /// <param name="queryName">The name of the query method.</param>
        /// <param name="entityType">The Type this query queries over.</param>
        /// <param name="parameters">Optional parameters to the query method. Specify null
        /// if the method takes no parameters.</param>
        /// <param name="hasSideEffects">True if the query has side-effects, false otherwise.</param>
        /// <param name="isComposable">True if the query supports composition, false otherwise.</param>
        internal EntityQuery(DomainClient domainClient, string queryName, Type entityType, IDictionary<string, object> parameters, bool hasSideEffects, bool isComposable)
        {
            if (domainClient == null)
            {
                throw new ArgumentNullException("domainClient");
            }
            if (string.IsNullOrEmpty(queryName))
            {
                throw new ArgumentNullException("queryName");
            }
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }
            this._domainClient = domainClient;
            this._queryName = queryName;
            this._entityType = entityType;
            this._parameters = parameters;
            this._hasSideEffects = hasSideEffects;
            this._isComposable = isComposable;
        }

        /// <summary>
        /// Initializes a new instance of the EntityQuery class based on an existing "base"
        /// query. The query is constructed with the same properties as the base query
        /// using the specified IQueryable as the new query.
        /// </summary>
        /// <param name="baseQuery">The existing query.</param>
        /// <param name="query">The new query.</param>
        internal EntityQuery(EntityQuery baseQuery, IQueryable query)
        {
            if (baseQuery == null)
            {
                throw new ArgumentNullException("baseQuery");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            this._domainClient = baseQuery._domainClient;
            this._queryName = baseQuery._queryName;
            this._entityType = baseQuery._entityType;
            this._parameters = baseQuery._parameters;
            this._hasSideEffects = baseQuery._hasSideEffects;
            this._isComposable = baseQuery._isComposable;
            this._includeTotalCount = baseQuery._includeTotalCount;
            this._query = query;
        }

        /// <summary>
        /// Gets the <see cref="DomainClient"/> for this query.
        /// </summary>
        public DomainClient DomainClient
        {
            get
            {
                return this._domainClient;
            }
        }

        /// <summary>
        /// Gets the name of the query method.
        /// </summary>
        public string QueryName
        {
            get
            {
                return this._queryName;
            }
        }

        /// <summary>
        /// Optional parameters required by the query method. Returns null
        /// if the method takes no parameters.
        /// </summary>
        public IDictionary<string, object> Parameters
        {
            get
            {
                return this._parameters;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the query has side-effects.
        /// </summary>
        public bool HasSideEffects
        {
            get
            {
                return this._hasSideEffects;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the query supports composition.
        /// </summary>
        public bool IsComposable
        {
            get
            {
                return this._isComposable;
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="IQueryable"/> for the query. Returns
        /// null if no query exists.
        /// </summary>
        public IQueryable Query
        {
            get
            {
                return this._query;
            }
            protected set
            {
                this._query = value;
            }
        }

        /// <summary>
        /// Gets the Type this query queries over.
        /// </summary>
        public Type EntityType
        {
            get 
            { 
                return this._entityType; 
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="DomainClientResult.TotalEntityCount"/> property is required.
        /// </summary>
        public bool IncludeTotalCount
        {
            get
            {
                return this._includeTotalCount;
            }
            set
            {
                this._includeTotalCount = value;
            }
        }
    }

    /// <summary>
    /// Class representing a LINQ query over a collection of entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public sealed class EntityQuery<TEntity> : EntityQuery where TEntity : Entity
    {
        internal EntityQuery(DomainClient domainClient, string queryName, IDictionary<string, object> parameters, bool hasSideEffects, bool isComposable)
            : base(domainClient, queryName, typeof(TEntity), parameters, hasSideEffects, isComposable)
        {
        }

        internal EntityQuery(EntityQuery eq, IQueryable<TEntity> query)
            : base(eq, query)
        {
        }

        internal new IQueryable<TEntity> Query
        {
            get
            {
                return (IQueryable<TEntity>)base.Query;
            }
            set
            {
                base.Query = value;
            }
        }

        /// <summary>
        /// If a query exists it is returned, otherwise a "dummy"
        /// root is returned.
        /// </summary>
        internal IQueryable<TEntity> QueryRoot
        {
            get
            {
                return this.Query ?? new TEntity[0].AsQueryable();
            }
        }
    }
}
