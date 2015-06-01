using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Represents a collection of <see cref="Entity"/> instances.
    /// </summary>
    public abstract class EntitySet : IEnumerable, ICollection, INotifyCollectionChanged, IRevertibleChangeTracking, INotifyPropertyChanged
    {
        private Dictionary<AssociationAttribute, Action<Entity>> _associationUpdateCallbackMap = new Dictionary<AssociationAttribute, Action<Entity>>();
        private Type _entityType;
        private EntityContainer _entityContainer;
        private EntitySetOperations _supportedOperations;
        private IList _list;
        private IDictionary<object, Entity> _identityCache;
        private List<Entity> _interestingEntities;
        private NotifyCollectionChangedEventHandler _collectionChangedEventHandler;

        /// <summary>
        /// Internal constructor since we're not opening up public extensibility scenarios,
        /// we only support our framework derived class.
        /// </summary>
        /// <param name="entityType">The type of <see cref="Entity"/> the set will contain</param>
        internal EntitySet(Type entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }
            if (!typeof(Entity).IsAssignableFrom(entityType))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Type_Not_Entity, entityType), "entityType");
            }

            this._entityType = entityType;
            this._identityCache = new Dictionary<object, Entity>();
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of entity contained by this <see cref="EntitySet"/>.
        /// </summary>
        public Type EntityType
        {
            get 
            { 
                return this._entityType; 
            }
        }

        /// <summary>
        /// Gets a value indicating whether the set allows new entities to be added
        /// </summary>
        public bool CanAdd
        {
            get
            {
                return (this._supportedOperations & EntitySetOperations.Add) != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether entities in the set can be edited
        /// </summary>
        public bool CanEdit
        {
            get
            {
                return (this._supportedOperations & EntitySetOperations.Edit) != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the set allows entities to be removed.
        /// Note that newly added entities can always be removed.
        /// </summary>
        public bool CanRemove
        {
            get
            {
                return (this._supportedOperations & EntitySetOperations.Remove) != 0;
            }
        }

        /// <summary>
        /// Clears all entities from the set
        /// </summary>
        public void Clear()
        {
            if (this._list == null || this.Count == 0)
            {
                // noop if the list is not initialized or is empty
                return;
            }

            // Detach all entities from the set (Reset accomplishes this)
            bool hadChanges = this.HasChanges;
            Entity[] clearedEntities = this._list.Cast<Entity>().ToArray();
            foreach (Entity entity in clearedEntities)
            {
                entity.Reset();
            }

            this._identityCache = new Dictionary<object, Entity>();
            this._interestingEntities = null;
            this._list = this.CreateList();

            this.OnCollectionChanged(NotifyCollectionChangedAction.Reset, clearedEntities, -1);

            if (hadChanges)
            {
                // if we had changes, we no longer do
                this.RaisePropertyChanged("HasChanges");
            }
        }

        /// <summary>
        /// Gets the current count of entities in the set
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }

        /// <summary>
        /// Gets the container this <see cref="EntitySet"/> belongs to
        /// </summary>
        public EntityContainer EntityContainer
        {
            get
            {
                return this._entityContainer;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this set supports update operations.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (this._supportedOperations == EntitySetOperations.None);
            }
        }

        /// <summary>
        /// Gets the backing list for this <see cref="EntitySet"/>
        /// </summary>
        protected internal IList List
        {
            get
            {
                return this._list;
            }
        }

        /// <summary>
        /// Gets a collection of all 'interesting' entities for the purposes of changeset computation. Interesting
        /// entities include those that are new, removed, or have been touched by an update operation.
        /// </summary>
        internal ReadOnlyCollection<Entity> InterestingEntities
        {
            get
            {
                if (this._interestingEntities == null)
                {
                    this._interestingEntities = new List<Entity>();
                }
                return this._interestingEntities.AsReadOnly();
            }
        }

        /// <summary>
        /// Tracks or untracks the specified entity as one that should be considered during
        /// changeset computation. This is more efficient than visiting every
        /// entity when computing changesets.
        /// </summary>
        /// <param name="entity">The entity to track or untrack</param>
        /// <param name="isInteresting">True if the entity is interesting, false otherwise</param>
        internal void TrackAsInteresting(Entity entity, bool isInteresting)
        {
            if (isInteresting)
            {
                if (!this.InterestingEntities.Contains(entity))
                {
                    this._interestingEntities.Add(entity);
                    if (this._interestingEntities.Count == 1)
                    {
                        // this is the first interesting entity in this set
                        // so raise the change notifications
                        this.RaisePropertyChanged("HasChanges");
                    }
                }
            }
            else
            {
                if (this._interestingEntities != null)
                {
                    int prevCount = this._interestingEntities.Count;
                    this._interestingEntities.Remove(entity);
                    if (this._interestingEntities.Count == 0 && prevCount == 1)
                    {
                        // if the last interesting entity has been removed, this set
                        // no longer has changes, so raise the change notifications
                        this.RaisePropertyChanged("HasChanges");
                    }
                }
            }
        }

        /// <summary>
        /// Creates the storage list for the set.
        /// </summary>
        /// <returns>The created storage list instance.</returns>
        protected abstract IList CreateList();

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <returns>The created entity instance.</returns>
        protected abstract Entity CreateEntity();

        internal void EnsureEditable(EntitySetOperations operation)
        {
            if ((this._supportedOperations & operation) == 0)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resource.EntitySet_UnsupportedOperation, this._entityType, operation));
            }
        }

        private void EnsureEntityType(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            if (!this._entityType.IsAssignableFrom(entity.GetType()))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.EntitySet_Wrong_Type, this._entityType, entity.GetType()), "entity");
            }
        }

        /// <summary>
        /// Returns an enumerator for the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        internal void Initialize(EntityContainer container, EntitySetOperations operationsToSupport)
        {
            System.Diagnostics.Debug.Assert(container != null, "EntityContainer must not be null");

            this._entityContainer = container;
            this._supportedOperations = operationsToSupport;

            this._list = this.CreateList();
        }

        /// <summary>
        /// Notifies any registered association update callbacks that the specified property
        /// has changed.
        /// </summary>
        /// <param name="entity">The entity that was updated.</param>
        /// <param name="propertyName">The name of the property that was changed.</param>
        internal void UpdateRelatedAssociations(Entity entity, string propertyName)
        {
            // Here we notify any association update callbacks so they can update collection membership
            // for the modified entity. This needs to happen in the following cases:
            // 1) If the entity is transitioning from a New to Unmodified state.
            // 2) If the property that changed is an FK member of an Association that is being monitored. Note
            //    that a single FK member can be part of multiple Associations.
            // Below we must eagerly enumerate the callbacks (ToArray) since invocation of callbacks might
            // cause _associationUpdateCallbackMap to be modified. We only want to notify the current set
            // of callbacks anyways.
            bool entityChangesAccepted = (this.CanEdit || this.CanAdd) && string.Compare(propertyName, "EntityState", StringComparison.Ordinal) == 0 && entity.EntityState == EntityState.Unmodified;
            IEnumerable<Action<Entity>> callbacks = this._associationUpdateCallbackMap.Where(p => p.Value != null && (entityChangesAccepted || p.Key.OtherKeyMembers.Contains(propertyName))).Select(p => p.Value).ToArray();
            foreach (Action<Entity> callback in callbacks)
            {
                callback(entity);
            }
        }

        /// <summary>
        /// Registers or unregisters the specified callback for update notifications for the specified association member. The
        /// callback will be invoked whenever a FK member participating in the association is modified on an entity in this
        /// EntitySet.
        /// </summary>
        /// <param name="association">AssociationAttribute indicating the association to monitor</param>
        /// <param name="callback">The callback to call</param>
        /// <param name="register">True if the callback is being registered, false if it is being unregistered</param>
        internal void RegisterAssociationCallback(AssociationAttribute association, Action<Entity> callback, bool register)
        {
            Action<Entity> del = null;
            this._associationUpdateCallbackMap.TryGetValue(association, out del);
            if (register)
            {
                this._associationUpdateCallbackMap[association] = (Action<Entity>)Delegate.Combine(del, callback);
            }
            else
            {
                this._associationUpdateCallbackMap[association] = (Action<Entity>)Delegate.Remove(del, callback);
            }
        }

        /// <summary>
        /// Adds the specified <see cref="Entity"/> to this <see cref="EntitySet"/>, also 
        /// recursively adding all unattached entities reachable via associations.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public void Add(Entity entity)
        {
            this.EnsureEntityType(entity);

            bool wasDetached = !this.IsAttached(entity);
            this.AddInternal(entity);

            if (wasDetached)
            {
                // if the entity was previously detached, recursively
                // Add all reachable entities
                AddAttachInferrer.Infer(this.EntityContainer, entity, (l, e) => l.AddInternal(e));
            }
        }

        /// <summary>
        /// Returns true if the specified entity is currently attached
        /// to this <see cref="EntitySet"/>.
        /// </summary>
        /// <param name="entity">The Entity to check</param>
        /// <returns>True if the Entity is attached</returns>
        internal bool IsAttached(Entity entity)
        {
            if (entity.LastSet == null)
            {
                return false;
            }

            // An entity is considered attached if it is in this set or
            // if it is "know" by this set, for example having been deleted
            return entity.EntitySet == this || this.InterestingEntities.Contains(entity);
        }

        /// <summary>
        /// Adds the specified entity to this set.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        internal void AddInternal(Entity entity)
        {
            if (entity.EntityState != EntityState.Deleted)
            {
                // re-adding a removed entity to the set is not considered
                // an actual entity add
                this.EnsureEditable(EntitySetOperations.Add);
            }

            this.EntityContainer.CheckCrossContainer(entity);

            if (entity.EntitySet != null && !entity.IsInferred)
            {
                // Allow Add to be called for attached entities that were
                // infer added
                throw new InvalidOperationException(Resource.EntitySet_EntityAlreadyInSet);
            }

            if (entity.EntitySet != null && entity.IsInferred && entity.EntityState != EntityState.New)
            {
                // when an inferred entity is transitioned to a new
                // state, it is no longer inferred
                entity.IsInferred = false;
            }

            if (entity.EntityState != EntityState.Deleted)
            {
                // Throw if we already have an entity cached with the same identity. Note that we do support
                // the following:
                // - scenarios where an entity is removed and a new entity with the same identity is added
                // - scenarios where the entity instance itself is the one already cached (for infer attach
                //   state transition scenarios)
                object identity = entity.GetIdentity();
                Entity cachedEntity = null;
                if (identity != null && this._identityCache.TryGetValue(identity, out cachedEntity) &&
                    cachedEntity.EntityState != EntityState.Deleted && !(object.ReferenceEquals(entity, cachedEntity)))
                {
                    throw new InvalidOperationException(Resource.EntitySet_DuplicateIdentity);
                }

                // only want to track adds for non-deleted entities
                entity.InitializeNew();
                this.TrackAsInteresting(entity, true);
            }
            else
            {
                // adding a deleted entity back, so undo the delete
                entity.UndoDelete();
            }

            if (!this._list.Contains(entity))
            {
                int idx = this._list.Add(entity);
                entity.EntitySet = this;
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, entity, idx);
            }
        }

        /// <summary>
        /// Removes the specified entity from the set.
        /// </summary>
        /// <remarks>
        /// If the entity is not in this set, an <see cref="InvalidOperationException"/> will be thrown.
        /// If the entity is the root of a compositional hierarchy, all child entities will also be removed.
        /// </remarks>
        /// <param name="entity">The entity to remove.</param>
        public void Remove(Entity entity)
        {
            this.EnsureEntityType(entity);

            // Recursively remove any children
            if (entity.MetaType.HasComposition)
            {
                CompositionalChildRemover.RemoveChildren(entity);
            }

            if (entity.EntityState != EntityState.New)
            {
                // removing a new entity from the set is not considered
                // an actual entity delete
                this.EnsureEditable(EntitySetOperations.Remove);
            }

            // verify the entity is a member of this set
            int idx = -1;
            if (entity.EntitySet == this)
            {
                idx = this._list.IndexOf(entity);
            }
            if (idx == -1)
            {
                throw new InvalidOperationException(Resource.EntitySet_EntityNotInSet);
            }

            if (entity.EntitySet != null && entity.IsInferred)
            {
                // when an inferred entity is transitioned to a new
                // state, it is no longer inferred
                entity.IsInferred = false;
            }

            if (entity.EntityState != EntityState.New)
            {
                // only want to track deletes for non-new entities
                entity.MarkDeleted();
                this.TrackAsInteresting(entity, true);
            }
            else
            {
                entity.MarkDetached();
                this.TrackAsInteresting(entity, false);
            }

            entity.EntitySet = null;

            this._list.Remove(entity);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, entity, idx);
        }

        internal bool Contains(Entity entity)
        {
            return this._list != null && this._list.Contains(entity);
        }

        /// <summary>
        /// Gets a value indicating whether this EntitySet currently has any pending changes
        /// </summary>
        public bool HasChanges
        {
            get
            {
                foreach (Entity entity in this.InterestingEntities)
                {
                    // A set is changed if it has any additions/deletions, or if any
                    // of its contained entities are changed
                    if (entity.EntityState == EntityState.New || entity.EntityState == EntityState.Deleted ||
                        ((IRevertibleChangeTracking)entity).IsChanged)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Accepts all changes made to this <see cref="EntitySet"/>
        /// </summary>
        protected void AcceptChanges()
        {
            // create a copy of InterestingEntities, since Remove modifies the set
            List<Entity> deletedEntities = this.InterestingEntities.Where(entity => entity.EntityState == EntityState.Deleted).ToList();
            List<Entity> otherEntities = this.InterestingEntities.Where(entity => entity.EntityState != EntityState.Deleted).ToList();

            // First accept deleted entities to make sure they're removed from the identity cache.
            // Without this, it's possible we first accept a new entity with PK "X" before deleting 
            // an existing entity with PK "X".
            foreach (Entity entity in deletedEntities)
            {
                ((IRevertibleChangeTracking)entity).AcceptChanges();
            }

            // Now accept the rest.
            foreach (Entity entity in otherEntities)
            {
                ((IRevertibleChangeTracking)entity).AcceptChanges();
            }

            this._interestingEntities.Clear();

            Debug.Assert(!this.HasChanges, "EntitySet.HasChanges should be false");
        }

        /// <summary>
        /// Reverts all changes made to this <see cref="EntitySet"/>. For modified entities,
        /// all modified property values are set back to their original values. New entities are
        /// removed, and any entities that were removed are re-added.
        /// </summary>
        protected void RejectChanges()
        {
            // create a copy of InterestingEntities, since Remove modifies the set
            foreach (Entity entity in this.InterestingEntities.ToList())
            {
                // composed child entities do their own set reject
                // so we don't want to duplicate that here
                if (entity.Parent == null)
                {
                    if (entity.EntityState == EntityState.New)
                    {
                        this.Remove(entity);
                    }
                    else if (entity.EntityState == EntityState.Deleted)
                    {
                        this.Add(entity);
                    }
                }

                ((IRevertibleChangeTracking)entity).RejectChanges();
            }
            this._interestingEntities.Clear();

            Debug.Assert(!this.HasChanges, "EntitySet.HasChanges should be false");
        }

        /// <summary>
        /// Attaches the specified <see cref="Entity"/> to this <see cref="EntitySet"/> in an
        /// unmodified state, also recursively attaching all unattached entities reachable via
        /// associations.
        /// </summary>
        /// <param name="entity">The entity to attach</param>
        public void Attach(Entity entity)
        {
            this.EnsureEntityType(entity);

            bool wasDetached = !this.IsAttached(entity);
            this.AttachInternal(entity);

            if (wasDetached)
            {
                // if the entity was previously detached, recursively
                // Attach all reachable entities
                AddAttachInferrer.Infer(this.EntityContainer, entity, (l, e) => l.AttachInternal(e));
            }
        }

        /// <summary>
        /// Attaches the <see cref="Entity"/> to this <see cref="EntitySet"/> in an
        /// unmodified state.
        /// </summary>
        /// <param name="entity">The entity to attach</param>
        internal void AttachInternal(Entity entity)
        {
            this.EntityContainer.CheckCrossContainer(entity);

            if (entity.EntitySet == this && entity.IsInferred && entity.EntityState == EntityState.Unmodified)
            {
                // Allow inferred entities to be manually attached.
                entity.IsInferred = false;
                return;
            }

            // Throw if the entity identity is null or we already have
            // an entity cached with the same identity.
            object identity = entity.GetIdentity();
            if (identity == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityKey_NullIdentity, entity));
            }
            if (this._identityCache.ContainsKey(identity))
            {
                throw new InvalidOperationException(Resource.EntitySet_DuplicateIdentity);
            }

            bool isInferredAdd = entity.IsInferred && entity.EntityState == EntityState.New;
            if (entity.EntitySet != null && !isInferredAdd)
            {
                // only allow Attach to be called for attached entities
                // that are inferred Adds
                throw new InvalidOperationException(Resource.EntitySet_EntityAlreadyAttached);
            }

            if (entity.EntitySet != null && entity.IsInferred && entity.EntityState != EntityState.Unmodified)
            {
                // when an inferred entity is transitioned to a new
                // state, it is no longer inferred
                entity.IsInferred = false;
            }

            entity.Reset();

            this.LoadEntity(entity);
        }

        /// <summary>
        /// Detaches the specified <see cref="Entity"/> from this <see cref="EntitySet"/>.
        /// </summary>
        /// <remarks>
        /// If the entity is not in this set, an <see cref="InvalidOperationException"/> will be thrown.
        /// If the entity is the root of a compositional hierarchy, all child entities will also be detached.
        /// </remarks>
        /// <param name="entity">The entity to detach</param>
        public void Detach(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this.EnsureEntityType(entity);

            // verify the entity is a member of this set
            int idx = -1;
            if (entity.EntitySet == this)
            {
                idx = this._list.IndexOf(entity);
            }
            if (idx == -1 && !(entity.EntityState == EntityState.Deleted && entity.LastSet == this))
            {
                // to detach an entity, it must either be in the set, or be a deleted
                // entity from the set
                throw new InvalidOperationException(Resource.EntitySet_EntityNotInSet);
            }

            if (idx != -1)
            {
                this._list.Remove(entity);
            }
            this.RemoveFromCache(entity);
            this.TrackAsInteresting(entity, false);
            entity.Reset();

            // Recursively detach any children
            if (entity.MetaType.HasComposition)
            {
                CompositionalChildDetacher.DetachChildren(entity);
            }

            if (idx != -1)
            {
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, entity, idx);
            }
        }

        /// <summary>
        /// Load the specified entity into the set
        /// </summary>
        /// <param name="entity">The entity to load</param>
        /// <returns>If the entity was already in the set
        /// (based on key identity) the cached instance is returned. If the
        /// entity wasn't in the set, the specified instance is returned
        /// </returns>
        internal Entity LoadEntity(Entity entity)
        {
            return this.LoadEntity(entity, LoadBehavior.KeepCurrent);
        }

        /// <summary>
        /// Load the specified entity into the set, using the specified
        /// <see cref="LoadBehavior"/>.
        /// </summary>
        /// <param name="entity">The entity to load</param>
        /// <param name="loadBehavior">The <see cref="LoadBehavior"/> to apply for the Load</param>
        /// <returns>If the entity was already in the set
        /// (based on key identity) the cached instance is returned. If the
        /// entity wasn't in the set, the specified instance is returned
        /// </returns>
        internal Entity LoadEntity(Entity entity, LoadBehavior loadBehavior)
        {
            this.EnsureEntityType(entity);

            if (entity.EntitySet != null)
            {
                throw new InvalidOperationException(Resource.EntitySet_EntityAlreadyAttached);
            }

            object identity = entity.GetIdentity();
            if (identity == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityKey_NullIdentity, entity));
            }

            Entity cachedEntity = null;
            this._identityCache.TryGetValue(identity, out cachedEntity);
            if (cachedEntity == null)
            {
                // add the entity to the cache
                this.AddToCache(entity);
                cachedEntity = entity;

                int idx = this._list.Add(entity);
                entity.MarkUnmodified();
                entity.EntitySet = this;

                if (this.CanEdit)
                {
                    // only want to start change tracking after an entity is completely
                    // deserialized (i.e. don't want to track serializer property sets)
                    entity.StartTracking();
                }

                entity.OnLoaded(true);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, entity, idx);
            }
            else
            {
                if (cachedEntity.IsEditing)
                {
                    // if the entity is currently being edited, merge operations are ignored
                    // to ensure user edits are not lost.
                    return cachedEntity;
                }

                // entity already exists in cache, so we must apply
                // the specified load behavior
                if (loadBehavior != LoadBehavior.KeepCurrent)
                {
                    // apply the state using the specified merge behavior
                    cachedEntity.Merge(entity, loadBehavior);
                }

                // if we're refreshing and the entity has original values
                // we need to update them
                if (cachedEntity.OriginalValues != null &&
                    loadBehavior == LoadBehavior.RefreshCurrent)
                {
                    cachedEntity.UpdateOriginalValues(entity);
                }
            }

            return cachedEntity;
        }

        /// <summary>
        /// Adds the specified entity to the cache. If a cache entry for the
        /// same identity already exists, an exception is thrown.
        /// </summary>
        /// <param name="entity">The entity to add</param>
        internal void AddToCache(Entity entity)
        {
            object identity = entity.GetIdentity();
            if (identity == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityKey_NullIdentity, entity));
            }

            // Throw if we already have an entity cached with the same identity
            if (this._identityCache.ContainsKey(identity))
            {
                throw new InvalidOperationException(Resource.EntitySet_DuplicateIdentity);
            }

            this._identityCache[identity] = entity;
        }

        /// <summary>
        /// Removes the specified entity from the cache.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        internal void RemoveFromCache(Entity entity)
        {
            object identity = entity.GetIdentity();

            Entity cachedEntity;
            if (identity == null || !this._identityCache.TryGetValue(identity, out cachedEntity) || cachedEntity != entity)
            {
                // Entity's identity has changed since it was added to the cache. Do an instance based lookup to find it.
                foreach (KeyValuePair<object, Entity> entry in this._identityCache)
                {
                    if (Object.ReferenceEquals(entry.Value, entity))
                    {
                        this._identityCache.Remove(entry.Key);
                        break;
                    }
                }
            }
            else
            {
                // Entity exists in the cache and its identity maps to its instance.
                this._identityCache.Remove(identity);
            }
        }

        /// <summary>
        /// Queries the cache for the entity indicated by the specified key
        /// values returning it if found, or returning null otherwise
        /// </summary>
        /// <param name="keyValues">The key values specified in the correct member order</param>
        /// <returns>The entity if found, null otherwise</returns>
        internal Entity GetEntityByKey(object[] keyValues)
        {
            Entity entity = null;
            object identity = null;

            if (keyValues.Length == 1)
            {
                identity = keyValues[0];
                if (identity == null)
                {
                    throw new ArgumentNullException("keyValues", Resource.EntityKey_CannotBeNull);
                }
            }
            else
            {
                identity = EntityKey.Create(keyValues);
            }

            this._identityCache.TryGetValue(identity, out entity);
            return entity;
        }

        /// <summary>
        /// Load the specified set of entities
        /// </summary>
        /// <param name="entities">The set of entities to load</param>
        /// <returns>list of loaded entities</returns>
        internal IEnumerable<Entity> LoadEntities(IEnumerable<Entity> entities)
        {
            return this.LoadEntities(entities, LoadBehavior.KeepCurrent);
        }

        /// <summary>
        /// Load the specified set of entities
        /// </summary>
        /// <param name="entities">The set of entities to load</param>
        /// <param name="loadBehavior">The <see cref="LoadBehavior"/> to use for the Load</param>
        /// <returns>The set of entities in the local cache after the load operation</returns>
        internal IEnumerable<Entity> LoadEntities(IEnumerable<Entity> entities, LoadBehavior loadBehavior)
        {
            List<Entity> loadedEntities = new List<Entity>();
            foreach (Entity entity in entities)
            {
                Entity loadedEntity = this.LoadEntity(entity, loadBehavior);
                loadedEntities.Add(loadedEntity);
            }
            return loadedEntities;
        }

        /// <summary>
        /// Method called whenever the collection changes. Overrides should call the base method
        /// to raise any required change notifications
        /// </summary>
        /// <param name="action">The change action</param>
        /// <param name="affectedObject">For Reset events, this will be a collection of removed entities. For all other
        /// events, this will be the single affected entity.</param>
        /// <param name="index">The affected index</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object affectedObject, int index)
        {
            if (this._collectionChangedEventHandler != null)
            {
                NotifyCollectionChangedEventArgs args = null;
                if (action == NotifyCollectionChangedAction.Add)
                {
                    args = new NotifyCollectionChangedEventArgs(action, affectedObject, index);
                }
                else if (action == NotifyCollectionChangedAction.Remove)
                {
                    args = new NotifyCollectionChangedEventArgs(action, affectedObject, index);
                }
                else if (action == NotifyCollectionChangedAction.Reset)
                {
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                }
                this._collectionChangedEventHandler(this, args);
            }

            this.RaisePropertyChanged("Count");
        }

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region ICollection Members
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return _list.SyncRoot; } }
        void ICollection.CopyTo(Array array, int index)
        {
            this._list.CopyTo(array, index);
        }
        #endregion

        #region INotifyCollectionChanged Members

        /// <summary>
        /// Event raised when the collection has changed, or the collection is reset.
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

        #endregion

        #region IRevertibleChangeTracking Members

        void IRevertibleChangeTracking.RejectChanges()
        {
            this.RejectChanges();
        }

        #endregion

        #region IChangeTracking Members

        bool IChangeTracking.IsChanged
        {
            get
            {
                return this.HasChanges;
            }
        }

        void IChangeTracking.AcceptChanges()
        {
            this.AcceptChanges();
        }

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised when a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property
        /// </summary>
        /// <param name="propertyName">The property that has changed</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called when an <see cref="EntitySet"/> property has changed.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (this._entityContainer != null)
            {
                this._entityContainer.SetPropertyChanged(this, e.PropertyName);
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Visitor used to traverse all associations in a graph and infer
        /// Attach/Add entities that are unattached.
        /// </summary>
        private class AddAttachInferrer : EntityVisitor
        {
            private Dictionary<Entity, bool> _visited = new Dictionary<Entity, bool>();
            private EntityContainer _container;
            private bool _isTopLevel = true;
            private Action<EntitySet, Entity> _action;

            public static void Infer(EntityContainer container, Entity entity, Action<EntitySet, Entity> action)
            {
                if (container == null)
                {
                    throw new ArgumentNullException("container");
                }
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }

                new AddAttachInferrer(container, action).Visit(entity);
            }

            private AddAttachInferrer(EntityContainer container, Action<EntitySet, Entity> action)
            {
                this._container = container;
                this._action = action;
            }

            public override void Visit(Entity entity)
            {
                // avoid cycles
                if (this._visited.ContainsKey(entity))
                {
                    return;
                }

                EntitySet set = this._container.GetEntitySet(entity.GetType());
                if (!this._isTopLevel && !set.IsAttached(entity))
                {
                    // infer for all detached entities except the root
                    entity.IsInferred = true;
                    this._action(set, entity);
                }

                this._visited.Add(entity, true);

                this._isTopLevel = false;

                base.Visit(entity);
            }

            protected override void VisitEntityCollection(IEntityCollection entityCollection, PropertyInfo propertyInfo)
            {
                if (entityCollection != null && entityCollection.HasValues)
                {
                    foreach (Entity entity in entityCollection.Entities)
                    {
                        this.Visit(entity);
                    }
                }
            }

            protected override void VisitEntityRef(IEntityRef entityRef, Entity parent, PropertyInfo propertyInfo)
            {
                // only visit the Entity if the value has been assigned
                // or loaded - we don't want to cause deferred loads.
                if (entityRef != null && entityRef.HasValue)
                {
                    this.Visit(entityRef.Entity);
                }
            }
        }

        /// <summary>
        /// Visitor used to recursively remove all compositional children
        /// in a hierarchy.
        /// </summary>
        internal class CompositionalChildRemover : EntityVisitor
        {
            public static void RemoveChildren(Entity entity)
            {
                new CompositionalChildRemover().Visit(entity);
            }

            private CompositionalChildRemover()
            {
            }

            protected override void VisitEntityCollection(IEntityCollection entityCollection, PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any())
                {
                    IEnumerable<Entity> children = entityCollection.Entities.ToArray();
                    foreach (Entity child in children)
                    {
                        entityCollection.Remove(child);
                        this.Visit(child);
                    }
                }
            }

            protected override void VisitEntityRef(IEntityRef entityRef, Entity parent, PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any())
                {
                    Entity child = null;
                    if (entityRef == null)
                    {
                        // If the EntityRef hasn't been accesssed before, it might
                        // not be initialized yet. In this case we need to access
                        // the property directly.
                        child = (Entity)propertyInfo.GetValue(parent, null);
                    }
                    else
                    {
                        child = entityRef.Entity;
                    }

                    // set value though property setter to ensure that
                    // FK sync code is run
                    propertyInfo.SetValue(parent, null, null);

                    if (child != null)
                    {
                        this.Visit(child);
                    }
                }
            }
        }

        /// <summary>
        /// Visitor used to detach all compositional children for a specified entity.
        /// </summary>
        internal class CompositionalChildDetacher : EntityVisitor
        {
            public static void DetachChildren(Entity entity)
            {
                new CompositionalChildDetacher().Visit(entity);
            }

            private CompositionalChildDetacher()
            {
            }

            protected override void VisitEntityCollection(IEntityCollection entityCollection, PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any())
                {
                    IEnumerable<Entity> children = entityCollection.Entities.ToArray();
                    foreach (Entity child in children)
                    {
                        if (child.EntitySet != null)
                        {
                            child.EntitySet.Detach(child);
                        }
                    }
                }
            }

            protected override void VisitEntityRef(IEntityRef entityRef, Entity parent, PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any())
                {
                    Entity child = null;
                    if (entityRef == null)
                    {
                        // If the EntityRef hasn't been accesssed before, it might
                        // not be initialized yet. In this case we need to access
                        // the property directly.
                        child = (Entity)propertyInfo.GetValue(parent, null);
                    }
                    else
                    {
                        child = entityRef.Entity;
                    }

                    if (child != null && child.EntitySet != null)
                    {
                        child.EntitySet.Detach(child);
                    }
                }
            }
        }
    }
        
    /// <summary>
    /// Represents a collection of <see cref="Entity"/> instances, providing change tracking and other services.
    /// </summary>
    /// <typeparam name="TEntity">The type of <see cref="Entity"/> this set will contain</typeparam>
    public sealed class EntitySet<TEntity> : EntitySet, IEnumerable<TEntity>, ICollection<TEntity>, ICollectionViewFactory where TEntity : Entity
    {
        /// <summary>
        /// Initializes a new instance of the EntitySet class
        /// </summary>
        public EntitySet()
            : base(typeof(TEntity))
        {
        }

        /// <summary>
        /// Creates the storage list for the set.
        /// </summary>
        /// <returns>The created storage list instance.</returns>
        protected override IList CreateList()
        {
            return new List<TEntity>();
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <returns>The created entity instance.</returns>
        protected override Entity CreateEntity()
        {
            if (typeof(TEntity).IsAbstract)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Cannot_Create_Abstract_Entity, typeof(TEntity)));
            }
            TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
            return entity;
        }

        /// <summary>
        /// Event raised whenever an <see cref="Entity"/> is added to this collection
        /// </summary>
        public event EventHandler<EntityCollectionChangedEventArgs<TEntity>> EntityAdded;

        /// <summary>
        /// Event raised whenever an <see cref="Entity"/> is removed to this collection
        /// </summary>
        public event EventHandler<EntityCollectionChangedEventArgs<TEntity>> EntityRemoved;

        /// <summary>
        /// Returns an enumerator
        /// </summary>
        /// <returns>The enumerator</returns>
        public new IEnumerator<TEntity> GetEnumerator()
        {
            return ((IList<TEntity>)List).GetEnumerator();
        }

        /// <summary>
        /// Attaches the specified <see cref="Entity"/> to this <see cref="EntitySet"/> in an
        /// unmodified state, also recursively attaching all unattached entities reachable via
        /// associations.
        /// </summary>
        /// <param name="entity">The entity to attach</param>
        public void Attach(TEntity entity)
        {
            base.Attach(entity);
        }

        /// <summary>
        /// Detaches the <see cref="Entity"/> from this <see cref="EntitySet"/>. If the entity
        /// is not in this set, an <see cref="InvalidOperationException"/> will be thrown. If the entity
        /// is the root of a compositional hierarchy, all child entities will also be detached.
        /// </summary>
        /// <param name="entity">The entity to detach</param>
        public void Detach(TEntity entity)
        {
            base.Detach(entity);
        }

        /// <summary>
        /// Add the specified entity to the set, also recursively adding all unattached
        /// entities reachable via associations.
        /// </summary>
        /// <remarks><paramref name="entity"/> needs to be of type <typeparamref name="TEntity"/>, and cannot be a subclass.</remarks>
        /// <param name="entity">The entity to add</param>
        public void Add(TEntity entity)
        {
            base.Add(entity);
        }

        /// <summary>
        /// Removes the specified entity from the set. If the entity is the root
        /// of a compositional hierarchy, all child entities will also be removed.
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        public void Remove(TEntity entity)
        {
            base.Remove(entity);
        }

        /// <summary>
        /// Method called whenever the set changes.
        /// </summary>
        /// <param name="action">The change action</param>
        /// <param name="affectedObject">For Reset events, this will be a collection of removed entities. For all other
        /// events, this will be the single affected entity.</param>
        /// <param name="index">The affected index</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedAction action, object affectedObject, int index)
        {
            if (action == NotifyCollectionChangedAction.Add && this.EntityAdded != null)
            {
                this.EntityAdded(this, new EntityCollectionChangedEventArgs<TEntity>((TEntity)affectedObject));
            }
            else if (action == NotifyCollectionChangedAction.Remove && this.EntityRemoved != null)
            {
                this.EntityRemoved(this, new EntityCollectionChangedEventArgs<TEntity>((TEntity)affectedObject));
            }
            else if (action == NotifyCollectionChangedAction.Reset && this.EntityRemoved != null)
            {
                foreach (TEntity removedEntity in ((IEnumerable)affectedObject).Cast<TEntity>())
                {
                    this.EntityRemoved(this, new EntityCollectionChangedEventArgs<TEntity>(removedEntity));
                }
            }

            base.OnCollectionChanged(action, affectedObject, index);
        }

        #region IEnumerable<TEntity> Members
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region ICollection<TEntity> Members
        void ICollection<TEntity>.CopyTo(TEntity[] array, int arrayIndex)
        {
            ((IList<TEntity>)List).CopyTo(array, arrayIndex);
        }

        bool ICollection<TEntity>.Contains(TEntity item)
        {
            return base.Contains(item);
        }

        bool ICollection<TEntity>.Remove(TEntity item)
        {
            try
            {
                // Ordinary remove throws on error, so if it did not then we can return true
                Remove(item);
                return true;
            }
            catch (InvalidOperationException ioe)
            {
                // If the entiy was not part of the set return false
                if (ioe.Message == Resource.EntitySet_EntityNotInSet)
                    return false;
                else
                    throw;
            }
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
        /// <see cref="IList"/> proxy that makes the <see cref="EntitySet"/> usable in the default collection views.
        /// All operations implemented against the proxy are passed through to the source <see cref="EntitySet"/>.
        /// </summary>
        /// <remarks>
        /// This proxy does not support a full set of list operations. However, the subset it does support
        /// is sufficient for interaction with the ListCollectionView.
        /// </remarks>
        /// <typeparam name="T">The entity type of this proxy</typeparam>
        private class ListCollectionViewProxy<T> : IList, IEnumerable<T>, INotifyCollectionChanged, ICollectionChangedListener where T : Entity
        {
            private readonly object _syncRoot = new object();
            private readonly EntitySet<T> _source;
            private readonly WeakCollectionChangedListener _weakCollectionChangedLister;

            internal ListCollectionViewProxy(EntitySet<T> source)
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

                this.Source.Add(entity);
                return this.IndexOf(entity);
            }

            public void Clear()
            {
                this.Source.Clear();
            }

            public bool Contains(object value)
            {
                return this.IndexOf(value) >= 0;
            }

            public int IndexOf(object value)
            {
                return this.Source.List.IndexOf(value);
            }

            public void Insert(int index, object value)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.CurrentCulture, Resource.IsNotSupported, "Insert"));
            }

            public bool IsFixedSize
            {
                get { return !(this.Source.CanAdd || this.Source.CanRemove); }
            }

            public bool IsReadOnly
            {
                get { return !(this.Source.CanAdd || this.Source.CanRemove); }
            }

            public void Remove(object value)
            {
                T entity = (T)value;
                if (entity == null)
                {
                    return;
                }

                this.Source.Remove(entity);
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
                        throw new ArgumentOutOfRangeException("index");
                    }
                    return this.Source.List[index];
                }
                set
                {
                    throw new NotSupportedException(
                        string.Format(CultureInfo.CurrentCulture, Resource.IsNotSupported, "Indexed setting"));
                }
            }

            public void CopyTo(Array array, int index)
            {
                this.Source.List.CopyTo(array, index);
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

            private EntitySet<T> Source
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
    }
}
