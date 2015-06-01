using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Class containing static extension methods implementing a subset of
    /// the LINQ pattern for <see cref="EntityQuery"/>
    /// </summary>
    public static class EntityQueryable
    {
        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> OrderBy<TEntity, TKey>(this EntityQuery<TEntity> source, Expression<Func<TEntity, TKey>> keySelector) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (!source.IsComposable)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityQuery_NotComposable, typeof(TEntity).Name, source.QueryName));
            }
            return new EntityQuery<TEntity>(source, Queryable.OrderBy<TEntity, TKey>(source.QueryRoot, keySelector));
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> OrderByDescending<TEntity, TKey>(this EntityQuery<TEntity> source, Expression<Func<TEntity, TKey>> keySelector) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (!source.IsComposable)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityQuery_NotComposable, typeof(TEntity).Name, source.QueryName));
            }
            return new EntityQuery<TEntity>(source, Queryable.OrderByDescending<TEntity, TKey>(source.QueryRoot, keySelector));
        }

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="count">The number to skip.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> Skip<TEntity>(this EntityQuery<TEntity> source, int count) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.IsComposable)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityQuery_NotComposable, typeof(TEntity).Name, source.QueryName));
            }
            return new EntityQuery<TEntity>(source, Queryable.Skip<TEntity>(source.QueryRoot, count));
        }

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="count">The number to take.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> Take<TEntity>(this EntityQuery<TEntity> source, int count) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.IsComposable)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityQuery_NotComposable, typeof(TEntity).Name, source.QueryName));
            }
            return new EntityQuery<TEntity>(source, Queryable.Take<TEntity>(source.QueryRoot, count));
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> ThenBy<TEntity, TKey>(this EntityQuery<TEntity> source, Expression<Func<TEntity, TKey>> keySelector) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (!source.IsComposable)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityQuery_NotComposable, typeof(TEntity).Name, source.QueryName));
            }
            return new EntityQuery<TEntity>(source, Queryable.ThenBy<TEntity, TKey>((IOrderedQueryable<TEntity>)(source.QueryRoot), keySelector));
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> ThenByDescending<TEntity, TKey>(this EntityQuery<TEntity> source, Expression<Func<TEntity, TKey>> keySelector) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (!source.IsComposable)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityQuery_NotComposable, typeof(TEntity).Name, source.QueryName));
            }
            return new EntityQuery<TEntity>(source, Queryable.ThenByDescending<TEntity, TKey>((IOrderedQueryable<TEntity>)(source.QueryRoot), keySelector));
        }

        /// <summary>
        /// Applies the specified filter to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> Where<TEntity>(this EntityQuery<TEntity> source, Expression<Func<TEntity, bool>> predicate) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (!source.IsComposable)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityQuery_NotComposable, typeof(TEntity).Name, source.QueryName));
            }
            return new EntityQuery<TEntity>(source, Queryable.Where<TEntity>(source.QueryRoot, predicate));
        }

        /// <summary>
        /// Applies the specified selection to the source query. Only empty selections are supported.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="selector">The selector function. Note that projections are not supported; the
        /// selection must be the entity itself.</param>
        /// <returns>The composed query.</returns>
        public static EntityQuery<TEntity> Select<TEntity>(this EntityQuery<TEntity> source, Expression<Func<TEntity, TEntity>> selector) where TEntity : Entity
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            return new EntityQuery<TEntity>(source, Queryable.Select<TEntity, TEntity>(source.QueryRoot, selector));
        }
    }
}
