using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents an asynchronous load operation
    /// </summary>
    public abstract class LoadOperation : OperationBase
    {
        private ObservableCollection<Entity> _entitiesCollection = new ObservableCollection<Entity>();
        private ReadOnlyObservableCollection<Entity> _entities;
        private ObservableCollection<Entity> _allEntitiesCollection = new ObservableCollection<Entity>();
        private ReadOnlyObservableCollection<Entity> _allEntities;
        private IEnumerable<ValidationResult> _validationErrors;
        private LoadBehavior _loadBehavior;
        private EntityQuery _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadOperation"/> class.
        /// </summary>
        /// <param name="query">The query to load.</param>
        /// <param name="loadBehavior"><see cref="LoadBehavior"/> to use for the load operation.</param>
        /// <param name="userState">Optional user state for the operation.</param>
        internal LoadOperation(EntityQuery query, LoadBehavior loadBehavior, object userState)
            : base(userState)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            this._query = query;
            this._loadBehavior = loadBehavior;
        }

        /// <summary>
        /// Creates a strongly typed <see cref="LoadOperation"/> for the specified Type.
        /// </summary>
        /// <typeparam name="TEntity">The entity Type.</typeparam>
        /// <param name="query">The query to load.</param>
        /// <param name="loadBehavior"><see cref="LoadBehavior"/> to use for the load operation.</param>
        /// <param name="completeAction">Action to execute when the operation completes.</param>
        /// <param name="userState">Optional user state for the operation.</param>
        /// <param name="cancelAction">Action to execute when the operation is canceled. If null, cancellation will not be supported.</param>
        /// <returns>The operation instance created.</returns>
        internal static LoadOperation Create<TEntity>(EntityQuery<TEntity> query, LoadBehavior loadBehavior,
            Action<LoadOperation> completeAction, object userState,
            Action<LoadOperation> cancelAction) where TEntity : Entity
        {
            Action<LoadOperation<TEntity>> wrappedCompleteAction = null;
            Action<LoadOperation<TEntity>> wrappedCancelAction = null;
            if (completeAction != null)
            {
                wrappedCompleteAction = arg => completeAction(arg);
            }
            if (cancelAction != null)
            {
                wrappedCancelAction = arg => cancelAction(arg);
            }

            return new LoadOperation<TEntity>(query, loadBehavior, wrappedCompleteAction, userState, wrappedCancelAction);
        }

        /// <summary>
        /// The <see cref="DomainClientResult"/> for this operation.
        /// </summary>
        protected new DomainClientResult Result
        {
            get
            {
                return (DomainClientResult)base.Result;
            }
        }

        /// <summary>
        /// The <see cref="EntityQuery"/> for this load operation.
        /// </summary>
        public EntityQuery EntityQuery
        {
            get
            {
                return this._query;
            }
        }

        /// <summary>
        /// The <see cref="LoadBehavior"/> for this load operation.
        /// </summary>
        public LoadBehavior LoadBehavior
        {
            get
            {
                return this._loadBehavior;
            }
        }

        /// <summary>
        /// Gets all the top level entities loaded by the operation. The collection returned implements
        /// <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>.
        /// </summary>
        public IEnumerable<Entity> Entities
        {
            get
            {
                if (this._entities == null)
                {
                    if (this.Result != null && this.Result.Entities.Any())
                    {
                        foreach (Entity entity in this.Result.Entities)
                        {
                            this._entitiesCollection.Add(entity);
                        }
                    }
                    this._entities = new ReadOnlyObservableCollection<Entity>((ObservableCollection<Entity>)this._entitiesCollection);
                }
                return this._entities;
            }
        }

        /// <summary>
        /// Gets all the entities loaded by the operation, including any
        /// entities referenced by the top level entities. The collection returned implements
        /// <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>.
        /// </summary>
        public IEnumerable<Entity> AllEntities
        {
            get
            {
                if (this._allEntities == null)
                {
                    if (this.Result != null && this.Result.AllEntities.Any())
                    {
                        foreach (Entity entity in this.Result.AllEntities)
                        {
                            this._allEntitiesCollection.Add(entity);
                        }
                    }
                    this._allEntities = new ReadOnlyObservableCollection<Entity>(this._allEntitiesCollection);
                }
                return this._allEntities;
            }
        }

        /// <summary>
        /// Gets the total server entity count for the query used by this operation. Automatic
        /// evaluation of the total server entity count requires the property <see cref="OpenRiaServices.DomainServices.Client.EntityQuery.IncludeTotalCount"/>
        /// on the query for the load operation to be set to <c>true</c>.
        /// </summary>
        public int TotalEntityCount
        {
            get
            {
                if (this.Result != null)
                {
                    return this.Result.TotalEntityCount;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public IEnumerable<ValidationResult> ValidationErrors
        {
            get
            {
                if (this.Result != null)
                {
                    return this.Result.ValidationErrors;
                }
                else
                {
                    // return any errors if set, otherwise return an empty
                    // collection
                    if (this._validationErrors == null)
                    {
                        this._validationErrors = new ValidationResult[0];
                    }
                }
                return this._validationErrors;
            }
        }

        /// <summary>
        /// Successfully completes the load operation with the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        internal void Complete(DomainClientResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // before calling base, we need to update any cached
            // observable collection results so the correct data
            // is accessible in the completion callback
            if (result.Entities.Any())
            {
                this.UpdateResults(result);
            }

            base.Complete(result);

            // raise our property events after all base property
            // events have been raised
            if (result.Entities.Any())
            {
                this.RaisePropertyChanged("TotalEntityCount");
            }
        }

        /// <summary>
        /// Completes the load operation with the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        internal new void Complete(Exception error)
        {
            if (typeof(DomainException).IsAssignableFrom(error.GetType()))
            {
                // DomainExceptions should not be modified
                base.Complete(error);
                return;
            }

            string message = string.Format(CultureInfo.CurrentCulture,
                Resource.DomainContext_LoadOperationFailed,
                this.EntityQuery.QueryName, error.Message);

            DomainOperationException domainOperationException = error as DomainOperationException;
            if (domainOperationException != null)
            {
                error = new DomainOperationException(message, domainOperationException);
            }
            else
            {
                error = new DomainOperationException(message, error);
            }

            base.Complete(error);
        }

        /// <summary>
        /// Completes the load operation with the specified validation errors.
        /// </summary>
        /// <param name="validationErrors">The validation errors.</param>
        internal void Complete(IEnumerable<ValidationResult> validationErrors)
        {
            this._validationErrors = validationErrors;
            this.RaisePropertyChanged("ValidationErrors");

            string message = string.Format(CultureInfo.CurrentCulture, 
                Resource.DomainContext_LoadOperationFailed_Validation, 
                this.EntityQuery.QueryName);
            DomainOperationException error = new DomainOperationException(message, validationErrors);

            base.Complete(error);
        }

        /// <summary>
        /// Update the observable result collections.
        /// </summary>
        /// <param name="result">The results of the completed load operation.</param>
        protected virtual void UpdateResults(DomainClientResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // if the Entities property has been examined, update the backing
            // observable collection
            if (this._entities != null)
            {
                this._entitiesCollection.Clear();
                foreach (Entity entity in result.Entities)
                {
                    this._entitiesCollection.Add(entity);
                }
            }

            // if the AllEntities property has been examined, update the backing
            // observable collection
            if (this._allEntities != null)
            {
                this._allEntitiesCollection.Clear();
                foreach (Entity entity in result.AllEntities)
                {
                    this._allEntitiesCollection.Add(entity);
                }
            }
        }
    }

    /// <summary>
    /// Represents an asynchronous load operation
    /// </summary>
    /// <typeparam name="TEntity">The entity Type being loaded.</typeparam>
    public sealed class LoadOperation<TEntity> : LoadOperation where TEntity : Entity
    {
        private ObservableCollection<TEntity> _entitiesCollection = new ObservableCollection<TEntity>();
        private ReadOnlyObservableCollection<TEntity> _entities;
        private Action<LoadOperation<TEntity>> _cancelAction;
        private Action<LoadOperation<TEntity>> _completeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadOperation"/> class.
        /// </summary>
        /// <param name="query">The query to load.</param>
        /// <param name="loadBehavior"><see cref="LoadBehavior"/> to use for the load operation.</param>
        /// <param name="completeAction">Action to execute when the operation completes.</param>
        /// <param name="userState">Optional user state for the operation.</param>
        /// <param name="cancelAction">Action to execute when the operation is canceled.</param>
        internal LoadOperation(EntityQuery<TEntity> query, LoadBehavior loadBehavior,
            Action<LoadOperation<TEntity>> completeAction, object userState,
            Action<LoadOperation<TEntity>> cancelAction)
            : base(query, loadBehavior, userState)
        {
            this._cancelAction = cancelAction;
            this._completeAction = completeAction;
        }

        /// <summary>
        /// The <see cref="EntityQuery"/> for this load operation.
        /// </summary>
        public new EntityQuery<TEntity> EntityQuery
        {
            get
            {
                return (EntityQuery<TEntity>)base.EntityQuery;
            }
        }

        /// <summary>
        /// Gets all the entities loaded by the operation, including any
        /// entities referenced by the top level entities. The collection returned implements
        /// <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>.
        /// </summary>
        public new IEnumerable<TEntity> Entities
        {
            get
            {
                if (this._entities == null)
                {
                    if (this.Result != null && this.Result.Entities.Any())
                    {
                        foreach (TEntity entity in this.Result.Entities)
                        {
                            this._entitiesCollection.Add(entity);
                        }
                    }
                    this._entities = new ReadOnlyObservableCollection<TEntity>((ObservableCollection<TEntity>)this._entitiesCollection);
                }
                return this._entities;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this operation supports cancellation.
        /// </summary>
        protected override bool SupportsCancellation
        {
            get
            {
                return (this._cancelAction != null);
            }
        }

        /// <summary>
        /// Update the observable result collections.
        /// </summary>
        /// <param name="result">The results of the completed load operation.</param>
        protected override void UpdateResults(DomainClientResult result)
        {
            base.UpdateResults(result);

            // if the Entities property has been examined, update the backing
            // observable collection
            if (this._entities != null)
            {
                // update the Entities observable collection
                this._entitiesCollection.Clear();
                foreach (TEntity entity in result.Entities)
                {
                    this._entitiesCollection.Add(entity);
                }
            }
        }

        /// <summary>
        /// Invokes the cancel callback.
        /// </summary>
        protected override void CancelCore()
        {
            this._cancelAction(this);
        }

        /// <summary>
        /// Invoke the completion callback.
        /// </summary>
        protected override void InvokeCompleteAction()
        {
            if (this._completeAction != null)
            {
                this._completeAction(this);
            }
        }
    }
}
