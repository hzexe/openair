using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents a collection of associated Entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of <see cref="Entity"/> in the collection</typeparam>
    public sealed class EntityCollection<TEntity> : IEntityCollection, ICollection<TEntity>, IEnumerable<TEntity>, INotifyCollectionChanged, INotifyPropertyChanged, ICollectionViewFactory where TEntity : Entity
    {
        private Action<TEntity> _attachAction;
        private Action<TEntity> _detachAction;
        private AssociationAttribute _assocAttribute;
        private Entity _parent;
        private EntitySet _sourceSet;
        private Func<TEntity, bool> _entityPredicate;
        private List<TEntity> _entities;
        private NotifyCollectionChangedEventHandler _collectionChangedEventHandler;
        private PropertyChangedEventHandler _propertyChangedEventHandler;
        private TEntity _attachingEntity;
        private TEntity _detachingEntity;
        private bool _entitiesLoaded;
        private bool _entitiesAdded;
        private bool _isComposition;

        /// <summary>
        /// Initializes a new instance of the EntityCollection class
        /// </summary>
        /// <param name="parent">The entity that this collection is a member of</param>
        /// <param name="memberName">The name of this EntityCollection member on the parent entity</param>
        /// <param name="entityPredicate">The function used to filter the associated entities, determining
        /// which are members of this collection.</param>
        public EntityCollection(Entity parent, string memberName, Func<TEntity, bool> entityPredicate)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException("memberName");
            }
            if (entityPredicate == null)
            {
                throw new ArgumentNullException("entityPredicate");
            }

            this._parent = parent;
            this._entityPredicate = entityPredicate;

            PropertyInfo propInfo = this._parent.GetType().GetProperty(memberName, MetaType.MemberBindingFlags);
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Property_Does_Not_Exist, parent.GetType(), memberName), "memberName");
            }
            this._assocAttribute = propInfo.GetCustomAttributes(false).OfType<AssociationAttribute>().SingleOrDefault();
            if (this._assocAttribute == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.MemberMustBeAssociation, memberName), "memberName");
            }

            this._isComposition = propInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any();

            // register our callback so we'll be notified whenever the
            // parent entity is added or removed from an EntitySet
            this._parent.RegisterSetChangedCallback(this.OnEntitySetChanged);

            this._parent.PropertyChanged += this.ParentEntityPropertyChanged;
        }

        /// <summary>
        /// Initializes a new instance of the EntityCollection class
        /// </summary>
        /// <param name="parent">The entity that this collection is a member of</param>
        /// <param name="memberName">The name of this EntityCollection member on the parent entity</param>
        /// <param name="entityPredicate">The function used to filter the associated entities, determining
        /// which are members of this collection.</param>
        /// <param name="attachAction">The function used to establish a back reference from an associated entity
        /// to the parent entity.</param>
        /// <param name="detachAction">The function used to remove the back reference from an associated entity
        /// to the parent entity.</param>
        public EntityCollection(Entity parent, string memberName, Func<TEntity, bool> entityPredicate, Action<TEntity> attachAction, Action<TEntity> detachAction)
            : this(parent, memberName, entityPredicate)
        {
            if (attachAction == null)
            {
                throw new ArgumentNullException("attachAction");
            }
            if (detachAction == null)
            {
                throw new ArgumentNullException("detachAction");
            }

            this._attachAction = attachAction;
            this._detachAction = detachAction;
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator for this collection
        /// </summary>
        /// <returns>An enumerator for this collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Event raised whenever an <see cref="Entity"/> is added to this collection
        /// </summary>
        public event EventHandler<EntityCollectionChangedEventArgs<TEntity>> EntityAdded;

        /// <summary>
        /// Event raised whenever an <see cref="Entity"/> is removed from this collection
        /// </summary>
        public event EventHandler<EntityCollectionChangedEventArgs<TEntity>> EntityRemoved;

        /// <summary>
        /// Gets the internal list of entities, creating it if it is null.
        /// </summary>
        private List<TEntity> Entities
        {
            get
            {
                if (this._entities == null)
                {
                    this._entities = new List<TEntity>();
                }
                return this._entities;
            }
        }

        /// <summary>
        /// Gets the current count of entities in this collection
        /// </summary>
        public int Count
        {
            get
            {
                this.Load();
                return this.Entities.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the EntityCollection source is external.
        /// </summary>
        private bool IsSourceExternal
        {
            get
            {
                return this.SourceSet != null && this.SourceSet.EntityContainer != this._parent.EntitySet.EntityContainer;
            }
        }

        private EntitySet SourceSet
        {
            get
            {
                if (this._parent.EntitySet != null)
                {
                    this._sourceSet = this._parent.EntitySet.EntityContainer.GetEntitySet(typeof(TEntity));
                }
                return this._sourceSet;
            }
        }

        /// <summary>
        /// Add the specified entity to this collection. If the entity is unattached, it
        /// will be added to its <see cref="EntitySet"/> automatically.
        /// </summary>
        /// <param name="entity">The entity to add</param>
        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (this.IsSourceExternal)
            {
                // Modifications are not allowed when the entity set source is external.
                throw new InvalidOperationException(Resource.EntityCollection_ModificationNotAllowedForExternalReference);
            }

            if (entity == this._attachingEntity)
            {
                return;
            }

            if (this.SourceSet != null)
            {
                this.SourceSet.EntityContainer.CheckCrossContainer(entity);
            }

            this.Attach(entity);

            if (!this.Entities.Contains(entity))
            {
                bool addedToSet = false;
                if (this.SourceSet != null)
                {
                    if (!this.SourceSet.IsAttached(entity))
                    {
                        // if an unattached entity is added to the collection, we infer it
                        // as an Add on its EntitySet
                        entity.IsInferred = true;
                        this.SourceSet.Add(entity);
                        addedToSet = true;
                    }
                    else if (this._isComposition && entity.EntityState == EntityState.Deleted)
                    {
                        // if a deleted entity is added to a compositional association,
                        // the delete should be undone
                        this.SourceSet.Add(entity);
                        addedToSet = true;
                    }  
                }

                // we may have to check for containment once more, since the EntitySet.Add calls
                // above can cause a dynamic add to this EntityCollection behind the scenes
                if (!addedToSet || !this.Entities.Contains(entity))
                {
                    this.AddEntity(entity);
                    this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entity, this.Entities.Count - 1));
                }

                this._entitiesAdded = true;
            }

            // When entities are added, we must load the collection to ensure
            // we're monitoring the source entity set from here on.
            this.Load();

            if (this._isComposition)
            {
                entity.Parent.OnChildUpdate();
            } 
        }

        /// <summary>
        /// Remove the specified entity from this collection.
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        public void Remove(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (entity == this._detachingEntity)
            {
                return;
            }

            int idx = this.Entities.IndexOf(entity);
            if (idx == -1 && !this._entityPredicate(entity))
            {
                // If the entity is not in this collection and the FK doesn't
                // match throw
                throw new InvalidOperationException(Resource.Entity_Not_In_Collection);
            }

            if (this.IsSourceExternal)
            {
                // Modifications are not allowed when the entity set source is external.
                throw new InvalidOperationException(Resource.EntityCollection_ModificationNotAllowedForExternalReference);
            }

            this.Detach(entity);

            if (idx != -1)
            {
                if (this.Entities.Remove(entity))
                {
                    // If the entity was removed, raise a collection changed notification. Note that the Detach call above might
                    // have caused a dynamic removal behind the scenes resulting in the entity no longer being in the collection,
                    // with the event already having been raised
                    this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entity, idx));
                }
            }

            if (this._isComposition)
            {
                // when a composed entity is removed from its collection,
                // it's inferred as a delete
                if (this._sourceSet != null && this._sourceSet.IsAttached(entity))
                {
                    this._sourceSet.Remove(entity);
                }

                entity.Parent.OnChildUpdate();
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the <see cref="EntityCollection&lt;TEntity&gt;"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the <see cref="EntityCollection&lt;TEntity&gt;"/>.</returns>
        public override string ToString()
        {
            return typeof(TEntity).Name;
        }

        /// <summary>
        /// Add the specified <paramref name="entity"/> this collection, setting its
        /// Parent if this is a compositional association. Whenever an
        /// entity is added to the underlying physical collection, it
        /// should be done through this method.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/>to add.</param>
        private void AddEntity(TEntity entity)
        {
            Debug.Assert(!this.Entities.Contains(entity), "Entity is already in this collection!");

            this.Entities.Add(entity);

            if (this._isComposition)
            {
                entity.SetParent(this._parent, this._assocAttribute);
            }
        }

        /// <summary>
        /// Calls the attach method to set the entity association reference.
        /// </summary>
        /// <param name="entity">entity to attach</param>
        private void Attach(TEntity entity)
        {
            if (this._attachAction != null)
            {
                TEntity prev = this._attachingEntity;
                this._attachingEntity = entity;
                try
                {
                    this._attachAction(entity);
                }
                finally
                {
                    this._attachingEntity = prev;
                }
            }
        }

        /// <summary>
        /// Calls the detach method to set the entity association reference.
        /// </summary>
        /// <param name="entity">entity to detach</param>
        private void Detach(TEntity entity)
        {
            if (this._detachAction != null)
            {
                TEntity prev = this._detachingEntity;
                this._detachingEntity = entity;
                try
                {
                    this._detachAction(entity);
                }
                finally
                {
                    this._detachingEntity = prev;
                }
            }
        }

        /// <summary>
        /// If not already loaded, this method runs our predicate against the source
        /// EntitySet
        /// </summary>
        private void Load()
        {
            if ((this._parent.EntitySet == null) || this._entitiesLoaded)
            {
                return;
            }

            // Get associated entity set and filter based on FK predicate
            EntitySet set = this._parent.EntitySet.EntityContainer.GetEntitySet(typeof(TEntity));
            foreach (TEntity entity in set.OfType<TEntity>().Where(this.Filter))
            {
                if (!this.Entities.Contains(entity))
                {
                    this.AddEntity(entity);
                }
            }

            // once we've loaded entities, we're caching them, so we need to update
            // our cached collection any time the source EntitySet is updated
            this._entitiesLoaded = true;
            this.MonitorEntitySet();
        }

        /// <summary>
        /// When filtering entities during query execution against the source set, or during
        /// source set collection changed notifications, we don't want to include New entities, 
        /// to ensure that we don't get false positives in cases where the entity's
        /// FK members are auto-generated on the server or haven't been set yet.
        /// </summary>
        /// <param name="entity">The entity to filter</param>
        /// <returns>A <see cref="Boolean"/> value indicating whether or not the <paramref name="entity"/> should be filtered.</returns>
        private bool Filter(TEntity entity)
        {
            return entity.EntityState != EntityState.New && this._entityPredicate(entity);
        }

        /// <summary>
        /// PropertyChanged handler for the parent entity.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The property changed event arguments.</param>
        private void ParentEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Reset the loaded entities as needed.
            if (this._entitiesLoaded && this._assocAttribute.ThisKeyMembers.Contains(e.PropertyName))
            {
                // A FK member for this association has changed on the parent
                // so we need to reset the cached collection
                this.ResetLoadedEntities();
            }
        }

        #region IEnumerable<TEntity> Members

        /// <summary>
        /// Returns an enumerator for this collection
        /// </summary>
        /// <returns>An enumerator for this collection</returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            this.Load();

            // To support iterations that also remove entities
            // from this EntityCollection or the source EntitySet
            // we must return a copy, since those operations
            // will modify our entities collection.
            return this.Entities.ToList().GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        /// <summary>
        /// Event raised whenever the contents of the collection changes
        /// </summary>
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add
            {
                this._collectionChangedEventHandler = (NotifyCollectionChangedEventHandler)Delegate.Combine(this._collectionChangedEventHandler, value);
            }
            remove
            {
                this._collectionChangedEventHandler = (NotifyCollectionChangedEventHandler)Delegate.Remove(this._collectionChangedEventHandler, value);
            }
        }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised whenever a property on this collection changes
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this._propertyChangedEventHandler = (PropertyChangedEventHandler)Delegate.Combine(this._propertyChangedEventHandler, value);
            }
            remove
            {
                this._propertyChangedEventHandler = (PropertyChangedEventHandler)Delegate.Remove(this._propertyChangedEventHandler, value);
            }
        }

        #endregion

        /// <summary>
        /// Called whenever the parent entity's <see cref="EntitySet"/> membership changes,
        /// allowing us to navigate to our source set for this collection.
        /// </summary>
        private void OnEntitySetChanged()
        {
            if (this._parent.EntitySet != null && this._sourceSet == null)
            {
                // if we were detached and we're now being attached, we want to
                // force the collection to reload next time it is inspected, since
                // our EntitySet has changed
                this._entitiesLoaded = false;
            }

            this.MonitorEntitySet();
        }

        /// <summary>
        /// Based on our current load status and our parent's attach status to an <see cref="EntityContainer"/>,
        /// update our event subscription to the source set's CollectionChanged event, the goal being to monitor
        /// the source set if and only if our parent is attached and we have loaded entities (this._entitiesLoaded == true)
        /// and need to keep our cached set in sync.
        /// </summary>
        private void MonitorEntitySet()
        {
            if (this._parent.EntitySet != null)
            {
                // it's expensive to monitor the source set for changes, so we only monitor when
                // entities have been added or loaded
                if (this._entitiesAdded || this._entitiesLoaded)
                {
                    if (this._sourceSet != null)
                    {
                        // Make sure we unsubscribe from any sets we may have already subscribed to (e.g. in case 
                        // of inferred adds). If we didn't already subscribe, this will be a no-op.
                        ((INotifyCollectionChanged)this._sourceSet).CollectionChanged -= this.SourceSet_CollectionChanged;
                        this._sourceSet.RegisterAssociationCallback(this._assocAttribute, this.OnEntityAssociationUpdated, false);
                    }

                    // subscribe to the source set CollectionChanged event
                    this._sourceSet = this._parent.EntitySet.EntityContainer.GetEntitySet(typeof(TEntity));
                    ((INotifyCollectionChanged)this._sourceSet).CollectionChanged += this.SourceSet_CollectionChanged;
                    this._sourceSet.RegisterAssociationCallback(this._assocAttribute, this.OnEntityAssociationUpdated, true);
                }
            }
            else if (this._parent.EntitySet == null && this._sourceSet != null)
            {
                // If the parent entity has been detached and we were monitoring,
                // we need to remove our event handler
                ((INotifyCollectionChanged)this._sourceSet).CollectionChanged -= this.SourceSet_CollectionChanged;
                this._sourceSet.RegisterAssociationCallback(this._assocAttribute, this.OnEntityAssociationUpdated, false);
                this._sourceSet = null;
            }
        }

        /// <summary>
        /// Callback for when an entity in the source set changes such that we need to reevaluate
        /// it's membership in our collection. This could be because an FK member for the association
        /// has changed, or when the entity state transitions to Unmodified.
        /// </summary>
        /// <param name="entity">The entity that has changed</param>
        private void OnEntityAssociationUpdated(Entity entity)
        {
            if ((entity == this._attachingEntity) || (entity == this._detachingEntity))
            {
                // avoid reentrancy issues in cases where the
                // entity is currently being processed by an Add/Remove
                // on this collection.
                return;
            }

            if (entity.EntityState == EntityState.New && entity.IsMergingState)
            {
                // We don't want to perform dynamic updates when merging store state
                // into new entities.
                return;
            }

            TEntity typedEntity = entity as TEntity;
            if (typedEntity != null && this._entitiesLoaded)
            {
                bool containsEntity = this.Entities.Contains(typedEntity);

                if (!containsEntity && this._parent.EntityState != EntityState.New && this.Filter(typedEntity))
                {
                    // Add matching entity to our set. When adding, we use the stronger Filter to
                    // filter out New entities
                    this.AddEntity(typedEntity);
                    this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entity, this.Entities.Count - 1));
                }
                else if (containsEntity && !this._entityPredicate(typedEntity))
                {
                    // The entity is in our set but is no longer a match, so we need to remove it.
                    // Here we use the predicate directly, since even if the entity is New if it
                    // no longer matches it should be removed.
                    int idx = this.Entities.IndexOf(typedEntity);
                    this.Entities.Remove(typedEntity);
                    this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entity, idx));
                }
            }
        }

        /// <summary>
        /// Whenever the source set changes, we need to run our predicate against the
        /// added/removed entities and if we get any matches we propagate the event and
        /// merge the modifications into our cached set if we are in a loaded state.
        /// </summary>
        /// <param name="sender">The caller who raised the collection changed event.</param>
        /// <param name="args">The collection changed event arguments.</param>
        private void SourceSet_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (this._parent.EntityState != EntityState.New &&
                args.Action == NotifyCollectionChangedAction.Add)
            {
                TEntity[] newEntities = args.NewItems.OfType<TEntity>().Where(this.Filter).ToArray();
                if (newEntities.Length > 0)
                {
                    int newStartingIdx = -1;
                    List<object> affectedEntities = new List<object>();
                    foreach (TEntity newEntity in newEntities)
                    {
                        newStartingIdx = this.Entities.Count;
                        if (!this.Entities.Contains(newEntity))
                        {
                            this.AddEntity(newEntity);
                            affectedEntities.Add(newEntity);
                        }
                    }

                    if (affectedEntities.Count > 0)
                    {
#if SILVERLIGHT
                        // SL doesn't support the constructor taking a list of objects
                        this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(args.Action, affectedEntities.Single(), newStartingIdx));
#else
                        this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(args.Action, affectedEntities, newStartingIdx));
#endif
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                // if the entity is in our cached collection, remove it
                TEntity[] entitiesToRemove = args.OldItems.OfType<TEntity>().Where(p => this.Entities.Contains(p)).ToArray();
                if (entitiesToRemove.Length > 0)
                {
                    int oldStartingIdx = this.Entities.IndexOf(entitiesToRemove[0]);
                    foreach (TEntity removedEntity in entitiesToRemove)
                    {
                        this.Entities.Remove(removedEntity);
                    }

#if SILVERLIGHT
                    //// REVIEW: Should we instead send out a reset event?
                    // SL doesn't support the constructor taking a list of objects
                    this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(args.Action, entitiesToRemove.Single(), oldStartingIdx));
#else
                    this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(args.Action, entitiesToRemove, oldStartingIdx));
#endif
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                if (this._entitiesLoaded)
                {
                    this.ResetLoadedEntities();
                }
            }
        }

        private void RaiseCollectionChangedNotification(NotifyCollectionChangedEventArgs args)
        {
            // Reset notifications are handled elsewhere for the EntityRemoved event.
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                if (this.EntityAdded != null)
                {
                    foreach (TEntity entity in args.NewItems.OfType<TEntity>())
                    {
                        this.EntityAdded(this, new EntityCollectionChangedEventArgs<TEntity>(entity));
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                if (this.EntityRemoved != null)
                {
                    foreach (TEntity entity in args.OldItems.OfType<TEntity>())
                    {
                        this.EntityRemoved(this, new EntityCollectionChangedEventArgs<TEntity>(entity));
                    }
                }
            }

            if (this._collectionChangedEventHandler != null)
            {
                this._collectionChangedEventHandler(this, args);
            }

            if (this._propertyChangedEventHandler != null)
            {
                this._propertyChangedEventHandler(this, new PropertyChangedEventArgs("Count"));
            }
        }

        /// <summary>
        /// Removes all non-New entities from the loaded entities collection and raises 
        /// any required EntityRemoved events.
        /// </summary>
        private void ResetLoadedEntities()
        {
            IEnumerable<TEntity> loadedEntities = this.Entities;
            this._entities = this.Entities.Where(p => p.EntityState == EntityState.New).ToList();
            this._entitiesLoaded = false;

            if (this.EntityRemoved != null)
            {
                // for each removed entity, we need to raise a notification
                foreach (TEntity entity in loadedEntities.Where(p => !this._entities.Contains(p)))
                {
                    this.EntityRemoved(this, new EntityCollectionChangedEventArgs<TEntity>(entity));
                }
            }

            this.RaiseCollectionChangedNotification(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion

        #region IEntityCollection Members
        AssociationAttribute IEntityCollection.Association
        {
            get
            {
                return this._assocAttribute;
            }
        }

        bool IEntityCollection.HasValues
        {
            get
            {
                return this._entities != null && this._entities.Count > 0;
            }
        }

        IEnumerable<Entity> IEntityCollection.Entities
        {
            get
            {
                return this.Cast<Entity>();
            }
        }

        void IEntityCollection.Add(Entity entity)
        {
            this.Add((TEntity)entity);
        }

        void IEntityCollection.Remove(Entity entity)
        {
            this.Remove((TEntity)entity);
        }
        #endregion

        #region ICollectionViewFactory

        /// <summary>
        /// Returns a custom view for specialized sorting, filtering, grouping, and currency.
        /// </summary>
        /// <returns>A custom view for specialized sorting, filtering, grouping, and currency</returns>
        ICollectionView ICollectionViewFactory.CreateView()
        {
            // We use the CollectionViewSource to obtain a ListCollectionView, a type internal to Silverlight
            return new CollectionViewSource() { Source = new ListCollectionViewProxy<TEntity>(this) } .View;
        }

        /// <summary>
        /// <see cref="IList"/> proxy that makes the <see cref="EntityCollection{T}"/> usable in the default
        /// collection views. All operations implemented against the proxy are passed through to the source
        /// <see cref="EntityCollection{T}"/>.
        /// </summary>
        /// <remarks>
        /// This proxy does not support a full set of list operations. However, the subset it does support
        /// is sufficient for interaction with the ListCollectionView.
        /// </remarks>
        /// <typeparam name="T">The entity type of this proxy</typeparam>
        private class ListCollectionViewProxy<T> : IList, IEnumerable<T>, INotifyCollectionChanged, ICollectionChangedListener where T : Entity
        {
            private readonly object _syncRoot = new object();
            private readonly EntityCollection<T> _source;
            private readonly WeakCollectionChangedListener _weakCollectionChangedLister;
            // Entities removed from an EntityCollection aren't typically removed from the source EntitySet.
            // However, we need to track entities added through the view and manually remove them from the
            // source EntitySet to achieve correct AddNew/CancelNew behavior.
            private readonly List<T> _addedEntities = new List<T>();

            internal ListCollectionViewProxy(EntityCollection<T> source)
            {
                this._source = source;
                this._weakCollectionChangedLister =
                    WeakCollectionChangedListener.CreateIfNecessary(this._source, this);
            }

            #region IList

            public int Add(object value)
            {
                T entity = value as T;
                if (entity == null)
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, Resource.MustBeAnEntity, "value"),
                        "value");
                }

                this._addedEntities.Add(entity);
                this.Source.Add(entity);
                return this.IndexOf(entity);
            }

            public void Clear()
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.CurrentCulture, Resource.IsNotSupported, "Clear"));
            }

            public bool Contains(object value)
            {
                return this.IndexOf(value) >= 0;
            }

            public int IndexOf(object value)
            {
                return ((IList)this.Source.Entities).IndexOf(value);
            }

            public void Insert(int index, object value)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.CurrentCulture, Resource.IsNotSupported, "Insert"));
            }

            // Always returning false for these two will create scenarios where Add or Remove ends up
            // throwing an exception because it is not actually a supported operation. However, there
            // are too many edge cases where type-based inference would fail. Always returning false
            // (to indicate CanAddNew and CanRemove should be true) provides a better experience by
            // allowing the developer to customize the Add and Remove options in the UI.
            public bool IsFixedSize
            {
                get { return false; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public void Remove(object value)
            {
                T entity = value as T;
                if (entity == null)
                {
                    return;
                }

                this.Source.Remove(entity);
                if (this._addedEntities.Contains(entity))
                {
                    this._addedEntities.Remove(entity);
                    // In case of Composition, the entity in the SourceSet
                    // may already be removed via this.Source.Remove above.
                    if (this.Source.SourceSet.Contains(entity))
                    {
                        this.Source.SourceSet.Remove(entity);
                    }
                }
            }

            public void RemoveAt(int index)
            {
                this.Remove(this[index]);
            }

            public object this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.Source.Count))
                    {
                        // We run into this scenario when the association reference is changed during an
                        // AddNew. The scenario is not supported, but we're trying to improve the error
                        // message. Instead of throwing an ArgumentOutOfRangeException, we'll simply return
                        // null and allow the view to inform us the added item is not at the requested index.
                        return null;
                    }
                    return this.Source.Entities[index];
                }
                set
                {
                    throw new NotSupportedException(
                        string.Format(CultureInfo.CurrentCulture, Resource.IsNotSupported, "Indexed setting"));
                }
            }

            public void CopyTo(Array array, int index)
            {
                ((IList)this.Source.Entities).CopyTo(array, index);
            }

            public int Count
            {
                get { return this.Source.Count; }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            public object SyncRoot
            {
                get { return this._syncRoot; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.Source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private EntityCollection<T> Source
            {
                get { return this._source; }
            }

            #endregion

            #region INotifyCollectionChanged

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                this.OnCollectionChanged(e);
            }

            #endregion

            #region ICollectionChangedListener

            void ICollectionChangedListener.OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                this.OnSourceCollectionChanged(sender, e);
            }

            #endregion
        }

        #endregion

        #region ICollection<TEntity> Members
        bool ICollection<TEntity>.IsReadOnly
        {
            get
            {
                // Modifications are not allowed when the entity set source is external.
                return IsSourceExternal;
            }
        }
        void ICollection<TEntity>.CopyTo(TEntity[] array, int arrayIndex)
        {
            this.Load();
            this.Entities.CopyTo(array, arrayIndex);
        }
        bool ICollection<TEntity>.Contains(TEntity item)
        {
            this.Load();
            return this.Entities.Contains(item);
        }
        bool ICollection<TEntity>.Remove(TEntity item)
        {
            int idx = Entities.IndexOf(item);
            Remove(item);
            return idx != -1;
        }
        /// <summary>
        /// Removes all items.
        /// </summary>
        void ICollection<TEntity>.Clear()
        {
            this.Load();
            foreach(var item in this.Entities.ToList())
                Remove(item);
        }
        #endregion
    }

    /// <summary>
    /// Internal interface providing loosely typed access to <see cref="EntityCollection&lt;TEntity&gt;"/> members needed
    /// by the framework
    /// </summary>
    // TODO : Consider making this interface (or a subset of it) public
    internal interface IEntityCollection
    {
        /// <summary>
        /// Gets the AssociationAttribute for this collection.
        /// </summary>
        AssociationAttribute Association
        {
            get;
        }

        /// <summary>
        /// Gets the collection of entities, loading the collection if it hasn't been loaded
        /// already. To avoid the deferred load, inspect the HasValues property first.
        /// </summary>
        IEnumerable<Entity> Entities
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this EntityCollection has been loaded or
        /// has had entities added to it.
        /// </summary>
        bool HasValues
        {
            get;
        }

        /// <summary>
        /// Adds the specified entity to the collection.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Add(Entity entity);

        /// <summary>
        /// Removes the specified entity from the collection.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        void Remove(Entity entity);
    }
}
