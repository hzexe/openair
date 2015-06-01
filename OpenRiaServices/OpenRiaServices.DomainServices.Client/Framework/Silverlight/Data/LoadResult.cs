using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// The result of a sucessfully completed load operation
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity loaded.</typeparam>
    public class LoadResult<TEntity> : IEnumerable<TEntity>, ICollection
        where TEntity : Entity
    {
        private readonly ReadOnlyCollection<TEntity> _loadedEntites;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadResult{TEntity}" /> class.
        /// </summary>
        /// <param name="loadOperation">The load operation which have been completed.</param>
        /// <exception cref="System.ArgumentException">load operation must have been completed successfully</exception>
        public LoadResult(LoadOperation<TEntity> loadOperation)
        {
            if (loadOperation.IsCanceled || loadOperation.HasError || !loadOperation.IsComplete)
                throw new ArgumentException(Resources.OperationNotComplete);

            // LoadOperation.Entities is a ReadOnlyObservableCollection which inherit ReadOnlyCollection
            _loadedEntites = (ReadOnlyCollection<TEntity>)loadOperation.Entities;

            EntityQuery = loadOperation.EntityQuery; 
            AllEntities = loadOperation.AllEntities;
            
            TotalEntityCount = loadOperation.TotalEntityCount;
            LoadBehavior = loadOperation.LoadBehavior;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadResult{TEntity}"/> class.
        /// </summary>
        /// <param name="query">The entity query which was completed.</param>
        /// <param name="loadBehavior">The load behavior used for load.</param>
        /// <param name="entities">Top level entities loaded.</param>
        /// <param name="allEntities">All entities loaded.</param>
        /// <param name="totalEntityCount">The total entity count.</param>
        public LoadResult(EntityQuery<TEntity> query, LoadBehavior loadBehavior, IEnumerable<TEntity> entities, IEnumerable<Entity> allEntities, int totalEntityCount)
        {
            _loadedEntites = (entities as ReadOnlyCollection<TEntity>) ?? new ReadOnlyCollection<TEntity>(entities.ToList());

            EntityQuery = query;
            LoadBehavior = loadBehavior;            
            AllEntities = allEntities;            
            TotalEntityCount = totalEntityCount;
            
        }

        /// <summary>
        /// Gets all the top level entities loaded by the operation.
        /// </summary>
        public IEnumerable<TEntity> Entities { get { return _loadedEntites; } }

        /// <summary>
        ///  Gets all the entities loaded by the operation, including any
        /// entities referenced by the top level entities. 
        /// </summary>
        public IEnumerable<Entity> AllEntities
        {
            get;
            private set;
        }

        /// <summary>
        /// The <see cref="EntityQuery"/> for this load operation.
        /// </summary>
        public EntityQuery<TEntity> EntityQuery { get; private set; }

        /// <summary>
        /// Gets the total server entity count for the query used by this operation. Automatic
        /// evaluation of the total server entity count requires the property <see cref="OpenRiaServices.DomainServices.Client.EntityQuery.IncludeTotalCount"/>
        /// on the query for the load operation to be set to <c>true</c>.
        /// </summary>
        public int TotalEntityCount { get; private set; }

        /// <summary>
        /// The <see cref="LoadBehavior"/> for this load operation.
        /// </summary>
        public LoadBehavior LoadBehavior { get; private set; }

        /// <summary>
        /// Gets the number of top level Entities loaded
        /// </summary>
        /// <returns>The number top level Entities loaded.</returns>
        public int Count { get { return _loadedEntites.Count; } }

        #region ICollection, IEnumerator implementations
        /// <summary>
        /// Copies Entities to an array (implements ICollection.CopyTo)
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        protected void CopyTo(Array array, int index) { ((ICollection)_loadedEntites).CopyTo(array, index); }
        void ICollection.CopyTo(Array array, int index) { this.CopyTo(array, index); }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).
        /// </summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
        protected bool IsSynchronized { get { return ((ICollection)_loadedEntites).IsSynchronized; } }
        bool ICollection.IsSynchronized { get { return this.IsSynchronized; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        protected object SyncRoot { get { return ((ICollection)_loadedEntites).SyncRoot; } }
        object ICollection.SyncRoot { get { return this.SyncRoot; } }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        protected IEnumerator<TEntity> GetEnumerator() { return _loadedEntites.GetEnumerator(); }
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() { return GetEnumerator(); }
        
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Protected function for IEnumerable<T> already exist")]
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        #endregion
    }
}