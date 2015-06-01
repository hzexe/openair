using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using OpenRiaServices.DomainServices.Client;

namespace OpenRiaServices.Data.DomainServices
{
    /// <summary>
    /// Builder that can be used to compose and apply a query to existing
    /// <see cref="IQueryable{T}"/> and <see cref="EntityQuery{T}"/> instances.
    /// </summary>
    /// <typeparam name="T">The entity type of the query</typeparam>
    public class QueryBuilder<T> where T : Entity
    {
        #region Member Fields

        private Func<EntityQuery<T>, EntityQuery<T>> _entityQueryReplay;
        private Func<IQueryable<T>, IQueryable<T>> _queryableReplay;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBuilder{T}"/> class.
        /// </summary>
        public QueryBuilder()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBuilder{T}"/> class.
        /// </summary>
        /// <param name="requestTotalItemCount">whether to request the total item count</param>
        public QueryBuilder(bool requestTotalItemCount)
        {
            this.RequestTotalItemCount = requestTotalItemCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to request the total item count.
        /// </summary>
        public bool RequestTotalItemCount { get; set; }

        #endregion

        #region Methods

        #region Query Methods

        /// <summary>
        /// Applies the specified ascending order clause to the builder
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query builder</returns>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="keySelector"/>
        /// is <c>null</c>
        /// </exception>
        public QueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
            {
                return (entityQueryReplay == null) ?
                    entityQuery.OrderBy(keySelector) :
                    entityQueryReplay(entityQuery).OrderBy(keySelector);
            };

            this._queryableReplay = (queryable) =>
            {
                return (queryableReplay == null) ?
                    queryable.OrderBy(keySelector) :
                    queryableReplay(queryable).OrderBy(keySelector);
            };

            return this;
        }

        /// <summary>
        /// Applies the specified descending order clause to the builder
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query builder</returns>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="keySelector"/>
        /// is <c>null</c>
        /// </exception>
        public QueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
            {
                return (entityQueryReplay == null) ?
                    entityQuery.OrderByDescending(keySelector) :
                    entityQueryReplay(entityQuery).OrderByDescending(keySelector);
            };

            this._queryableReplay = (queryable) =>
            {
                return (queryableReplay == null) ?
                    queryable.OrderByDescending(keySelector) :
                    queryableReplay(queryable).OrderByDescending(keySelector);
            };

            return this;
        }

        /// <summary>
        /// Applies the specified selection to the builder. Only empty selections are supported.
        /// </summary>
        /// <param name="selector">The selector function. Note that projections are not supported; the
        /// selection must be the entity itself.</param>
        /// <returns>The composed query builder</returns>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="selector"/>
        /// is <c>null</c>
        /// </exception>
        public QueryBuilder<T> Select(Expression<Func<T, T>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
            {
                return (entityQueryReplay == null) ?
                    entityQuery.Select(selector) :
                    entityQueryReplay(entityQuery).Select(selector);
            };

            this._queryableReplay = (queryable) =>
            {
                return (queryableReplay == null) ?
                    queryable.Select(selector) :
                    queryableReplay(queryable).Select(selector);
            };

            return this;
        }

        /// <summary>
        /// Applies the specified skip clause to the builder
        /// </summary>
        /// <param name="count">The number to skip</param>
        /// <returns>The composed query builder</returns>
        public QueryBuilder<T> Skip(int count)
        {
            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
            {
                return (entityQueryReplay == null) ?
                    entityQuery.Skip(count) :
                    entityQueryReplay(entityQuery).Skip(count);
            };

            this._queryableReplay = (queryable) =>
            {
                return (queryableReplay == null) ?
                    queryable.Skip(count) :
                    queryableReplay(queryable).Skip(count);
            };

            return this;
        }

        /// <summary>
        /// Applies the specified take clause to the builder
        /// </summary>
        /// <param name="count">The number to take</param>
        /// <returns>The composed query builder</returns>
        public QueryBuilder<T> Take(int count)
        {
            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
            {
                return (entityQueryReplay == null) ?
                    entityQuery.Take(count) :
                    entityQueryReplay(entityQuery).Take(count);
            };

            this._queryableReplay = (queryable) =>
            {
                return (queryableReplay == null) ?
                    queryable.Take(count) :
                    queryableReplay(queryable).Take(count);
            };

            return this;
        }

        /// <summary>
        /// Applies the specified ascending order clause to the builder
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query builder</returns>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="keySelector"/>
        /// is <c>null</c>
        /// </exception>
        public QueryBuilder<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
            {
                return (entityQueryReplay == null) ?
                    entityQuery.ThenBy(keySelector) :
                    entityQueryReplay(entityQuery).ThenBy(keySelector);
            };

            this._queryableReplay = (queryable) =>
            {
                return (queryableReplay == null) ?
                    ((IOrderedQueryable<T>)queryable).ThenBy(keySelector) :
                    ((IOrderedQueryable<T>)queryableReplay(queryable)).ThenBy(keySelector);
            };

            return this;
        }

        /// <summary>
        /// Applies the specified descending order clause to the builder
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query builder</returns>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="keySelector"/>
        /// is <c>null</c>
        /// </exception>
        public QueryBuilder<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
            {
                return (entityQueryReplay == null) ?
                    entityQuery.ThenByDescending(keySelector) :
                    entityQueryReplay(entityQuery).ThenByDescending(keySelector);
            };

            this._queryableReplay = (queryable) =>
            {
                return (queryableReplay == null) ?
                    ((IOrderedQueryable<T>)queryable).ThenByDescending(keySelector) :
                    ((IOrderedQueryable<T>)queryableReplay(queryable)).ThenByDescending(keySelector);
            };

            return this;
        }

        /// <summary>
        /// Applies the specified filter to the builder
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <returns>The composed query builder</returns>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="predicate"/>
        /// is <c>null</c>
        /// </exception>
        public QueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            Func<EntityQuery<T>, EntityQuery<T>> entityQueryReplay = this._entityQueryReplay;
            Func<IQueryable<T>, IQueryable<T>> queryableReplay = this._queryableReplay;

            this._entityQueryReplay = (entityQuery) =>
                {
                    return (entityQueryReplay == null) ?
                        entityQuery.Where(predicate) :
                        entityQueryReplay(entityQuery).Where(predicate);
                };

            this._queryableReplay = (queryable) =>
                {
                    return (queryableReplay == null) ?
                        queryable.Where(predicate) :
                        queryableReplay(queryable).Where(predicate);
                };
            
            return this;
        }

        #endregion

        #region Apply Methods

        /// <summary>
        /// Applies the builer query to the specified <paramref name="entityQuery"/>
        /// </summary>
        /// <param name="entityQuery">The entity query to apply the builer query to</param>
        /// <returns>An <see cref="EntityQuery{T}"/> with the builder query applied</returns>
        /// <exception cref="ArgumentNullException"> is thrown when <paramref name="entityQuery"/>
        /// is <c>null</c>
        /// </exception>
        public EntityQuery<T> ApplyTo(EntityQuery<T> entityQuery)
        {
            if (entityQuery == null)
            {
                throw new ArgumentNullException("entityQuery");
            }

            entityQuery.IncludeTotalCount = this.RequestTotalItemCount;
            if (this._entityQueryReplay != null)
            {
                entityQuery = this._entityQueryReplay(entityQuery);
            }
            return entityQuery;
        }

        /// <summary>
        /// Applies the builer query to the specified <paramref name="enumerable"/>
        /// </summary>
        /// <param name="enumerable">The eumerable to apply the builer query to</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with the builder query applied</returns>
        /// <exception cref="ArgumentNullException"> is thrown when <paramref name="enumerable"/>
        /// is <c>null</c>
        /// </exception>
        public IEnumerable<T> ApplyTo(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            return this.ApplyTo(enumerable.AsQueryable());
        }

        /// <summary>
        /// Applies the builer query to the specified <paramref name="queryable"/>
        /// </summary>
        /// <param name="queryable">The queryable to apply the builer query to</param>
        /// <returns>An <see cref="IQueryable{T}"/> with the builder query applied</returns>
        /// <exception cref="ArgumentNullException"> is thrown when <paramref name="queryable"/>
        /// is <c>null</c>
        /// </exception>
        public IQueryable<T> ApplyTo(IQueryable<T> queryable)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException("queryable");
            }

            if (this._queryableReplay != null)
            {
                queryable = this._queryableReplay(queryable);
            }
            return queryable;
        }

        /// <summary>
        /// Applies the builer query to the specified <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">The observable collection to apply the builer query to</param>
        /// <returns>A <see cref="ObservableCollection{T}"/> with the builder query applied</returns>
        /// <exception cref="ArgumentNullException"> is thrown when <paramref name="collection"/>
        /// is <c>null</c>
        /// </exception>
        public ObservableCollection<T> ApplyTo(ObservableCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            return new ObservableCollection<T>(this.ApplyTo((IEnumerable<T>)collection));
        }

        #endregion

        #endregion
    }
}
