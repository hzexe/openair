using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents a cache of Entities in the form of a collection of <see cref="EntitySet"/>s.
    /// </summary>
    public abstract class EntityContainer : IRevertibleChangeTracking, INotifyPropertyChanged
    {
        private IDictionary<Type, EntitySet> _entitySets;
        private IDictionary<Type, EntitySet> _referencedEntitySets;
        private IDictionary<string, Type> _entityTypeMap;
        private int _dirtySetCount;
        private IDictionary<Type, Type> _entityRootTypes;
        private ValidationContext _validationContext;

        /// <summary>
        /// Protected constructor since this is an abstract class
        /// </summary>
        protected EntityContainer()
        {
            this._entitySets = new Dictionary<Type, EntitySet>();
            this._referencedEntitySets = new Dictionary<Type, EntitySet>();
            this._entityTypeMap = new Dictionary<string, Type>();
            this._entityRootTypes = new Dictionary<Type, Type>();
        }

        /// <summary>
        /// Gets a value indicating whether this EntityContainer currently has any pending changes
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return this._entitySets.Any(p => ((IRevertibleChangeTracking)p.Value).IsChanged);
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="EntitySet"/>s in this <see cref="EntityContainer"/>.
        /// </summary>
        public IEnumerable<EntitySet> EntitySets
        {
            get
            {
                // return all entity sets
                foreach (EntitySet entitySet in this._entitySets.Values)
                {
                    yield return entitySet;
                }
            }
        }

        /// <summary>
        /// Gets or sets the optional <see cref="ValidationContext"/> to use for all
        /// validation operations invoked by each <see cref="Entity"/> within this
        /// <see cref="EntityContainer"/>.
        /// </summary>
        /// <value>
        /// This value is inherited from the <see cref="DomainContext"/> that
        /// creates this container.
        /// </value>
        internal ValidationContext ValidationContext
        {
            get { return this._validationContext; }
            set { this._validationContext = value; }
        }

        /// <summary>
        /// Verifies that the specified entity is not already attached to a
        /// different EntityContainer. Cross container operations are not
        /// supported.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <exception cref="InvalidOperationException">If the entity is attached
        /// to a different EntityContainer.</exception>
        internal void CheckCrossContainer(Entity entity)
        {
            EntityContainer container = entity.LastSet != null ? entity.LastSet.EntityContainer : null;

            if (container != null && container != this)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityContainer_CrossContainerAttach, entity));
            }
        }

        /// <summary>
        /// Adds a reference to an external <see cref="EntitySet"/>.
        /// </summary>
        /// <param name="entitySet">A <see cref="EntitySet"/> to register as an external reference.</param>
        public void AddReference(EntitySet entitySet)
        {
            if (entitySet == null)
            {
                throw new ArgumentNullException("entitySet");
            }

            Type entityType = entitySet.EntityType;
            if (this._entitySets.ContainsKey(entityType) || this._referencedEntitySets.ContainsKey(entityType))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resource.EntityContainer_EntitySetAlreadyExists,
                        entityType));
            }

            this._referencedEntitySets.Add(entityType, entitySet);
        }

        /// <summary>
        /// Clear the contents of all <see cref="EntitySet"/>s in this container
        /// </summary>
        public void Clear()
        {
            foreach (var setEntry in this._entitySets)
            {
                setEntry.Value.Clear();
            }
        }

        /// <summary>
        /// Create an <see cref="EntitySet"/> in this container for the specified entity type
        /// </summary>
        /// <typeparam name="TEntity">The Entity type</typeparam>
        protected void CreateEntitySet<TEntity>() where TEntity : Entity
        {
            CreateEntitySet<TEntity>(EntitySetOperations.None);
        }

        /// <summary>
        /// Create an <see cref="EntitySet"/> in this container for the specified entity type
        /// </summary>
        /// <typeparam name="TEntity">The Entity type</typeparam>
        /// <param name="supportedOperations">The operations supported for the Entity type</param>
        protected void CreateEntitySet<TEntity>(EntitySetOperations supportedOperations) where TEntity : Entity
        {
            EntitySet set = new EntitySet<TEntity>();
            this.AddEntitySet(set, supportedOperations);
        }

        /// <summary>
        /// Adds an <see cref="EntitySet"/> to this container with the specified <paramref name="supportedOperations"/>.
        /// </summary>
        /// <param name="set">The <see cref="EntitySet"/> to add to the container.</param>
        /// <param name="supportedOperations">The <see cref="EntitySetOperations"/> supported by this entity set.</param>
        internal void AddEntitySet(EntitySet set, EntitySetOperations supportedOperations)
        {
            set.Initialize(this, supportedOperations);

            this._entitySets[set.EntityType] = set;
            this._entityTypeMap[set.EntityType.Name] = set.EntityType;
        }

        /// <summary>
        /// Event handler for PropertyChanged events on the EntitySets in this container
        /// </summary>
        /// <param name="entitySet">The EntitySet</param>
        /// <param name="propertyName">The property that has changed</param>
        internal void SetPropertyChanged(EntitySet entitySet, string propertyName)
        {
            if (string.CompareOrdinal(propertyName, "HasChanges") == 0)
            {
                if (entitySet.HasChanges)
                {
                    if (this._dirtySetCount++ == 0)
                    {
                        // the set has changes and is the first dirty list
                        // in this container, so raise the change events
                        this.RaisePropertyChanged("HasChanges");
                    }
                }
                else if (this._dirtySetCount > 0)
                {
                    if (--this._dirtySetCount == 0)
                    {
                        // if the set has just moved to HasChanges == false and it
                        // is the last dirty set in the this container, raise
                        // the change events
                        this.RaisePropertyChanged("HasChanges");
                    }
                }
            }
        }

        /// <summary>
        /// Return the <see cref="EntitySet"/> for the specified <see cref="Entity"/> Type. If there is no
        /// <see cref="EntitySet"/> for the specified type, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type</typeparam>
        /// <returns>The EntitySet</returns>
        public EntitySet<TEntity> GetEntitySet<TEntity>() where TEntity : Entity
        {
            EntitySet set = this.GetEntitySet(typeof(TEntity));
            EntitySet<TEntity> typedSet = set as EntitySet<TEntity>;
            if (typedSet == null)
            {
                Type rootEntityType = this.GetRootEntityType(typeof(TEntity));

                // It's possible 'set' is 'EntitySet<BaseClass>' instead of 'EntitySet<DerivedClass>'. If that's the case, throw.
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityContainer_CannotRetrieveEntitySetForDerivedEntity, typeof(TEntity).Name, rootEntityType.Name));
            }

            return typedSet;
        }

        /// <summary>
        /// Return the <see cref="EntitySet"/> for the specified <see cref="Entity"/> Type. If there is no
        /// <see cref="EntitySet"/> for the specified type, an exception is thrown.
        /// </summary>
        /// <param name="entityType">The Entity type</param>
        /// <returns>The EntitySet</returns>
        public EntitySet GetEntitySet(Type entityType)
        {
            EntitySet entitySet;
            if (!this.TryGetEntitySet(entityType, out entitySet))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.EntityContainerDoesntContainEntityType, entityType));
            }

            return entitySet;
        }

        /// <summary>
        /// Try to get the set for the specified <see cref="Entity"/> type, returning false if not found
        /// </summary>
        /// <param name="entityType">The Entity type</param>
        /// <param name="entitySet">The EntitySet if present, null otherwise</param>
        /// <returns>True if an EntitySet for the Type was found, false otherwise</returns>
        public bool TryGetEntitySet(Type entityType, out EntitySet entitySet)
        {
            Type rootType = this.GetRootEntityType(entityType);

            if (rootType != null &&
                (this._entitySets.TryGetValue(rootType, out entitySet) ||
                 this._referencedEntitySets.TryGetValue(rootType, out entitySet)))
            {
                return true;
            }

            entitySet = null;
            return false;
        }

        /// <summary>
        /// Returns an <see cref="EntityChangeSet"/> containing the current set of pending changes
        /// </summary>
        /// <returns>The current set of pending changes</returns>
        public EntityChangeSet GetChanges()
        {
            List<Entity> addedEntities = new List<Entity>();
            List<Entity> modifiedEntities = new List<Entity>();
            List<Entity> removedEntities = new List<Entity>();

            foreach (KeyValuePair<Type, EntitySet> entitySetItem in this._entitySets)
            {
                EntitySet set = entitySetItem.Value;

                foreach (Entity entity in set.InterestingEntities)
                {
                    if (entity.EntityState == EntityState.New)
                    {
                        addedEntities.Add(entity);
                    }
                    else if (entity.EntityState == EntityState.Modified)
                    {
                        modifiedEntities.Add(entity);
                    }
                    else if (entity.EntityState == EntityState.Deleted)
                    {
                        removedEntities.Add(entity);
                    }
                }
            }

            EntityChangeSet changeSet = new EntityChangeSet(addedEntities.AsReadOnly(), modifiedEntities.AsReadOnly(), removedEntities.AsReadOnly());
            Debug.Assert(this.HasChanges == !changeSet.IsEmpty, "Invariant : these states should be in sync");
            return changeSet;
        }

        /// <summary>
        /// Determines whether the specified entity has any children
        /// that are currently in a modified state.
        /// </summary>
        /// <param name="entity">The parent entity to check.</param>
        /// <returns>True if there are child updates, false otherwise.</returns>
        internal bool HasChildChanges(Entity entity)
        {
            foreach (Type childType in entity.MetaType.ChildTypes)
            {
                // foreach set, enumerate all tracked entities to
                // discover any that are modified
                EntitySet set = this.GetEntitySet(childType);
                foreach (Entity e in set.InterestingEntities)
                {
                    if (e.Parent == entity && e.EntityState != EntityState.Unmodified)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Accepts or rejects all child changes for the specified entity.
        /// </summary>
        /// <param name="entity">The parent entity.</param>
        /// <param name="acceptChanges"><c>True</c> if child changes should be accepted,
        /// <c>false</c> if they should be rejected.</param>
        internal void CompleteChildChanges(Entity entity, bool acceptChanges)
        {
            foreach (Type childType in entity.MetaType.ChildTypes)
            {
                EntitySet set = this.GetEntitySet(childType);
                foreach (Entity e in set.InterestingEntities.ToArray())
                {
                    if (e.Parent == entity && e.EntityState != EntityState.Unmodified)
                    {
                        if (acceptChanges)
                        {
                            ((IRevertibleChangeTracking)e).AcceptChanges();
                        }
                        else
                        {
                            ((IRevertibleChangeTracking)e).RejectChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Accept all changes that have been made to entities or <see cref="EntitySet"/>s
        /// in this container.
        /// </summary>
        protected void AcceptChanges()
        {
            foreach (KeyValuePair<Type, EntitySet> entitySetItem in this._entitySets)
            {
                EntitySet set = entitySetItem.Value;
                ((IRevertibleChangeTracking)set).AcceptChanges();
            }
            Debug.Assert(!this.HasChanges, "EntityContainer.HasChanges should be false");
        }

        /// <summary>
        /// Revert all changes that have been made to entities or <see cref="EntitySet"/>s
        /// in this container.
        /// </summary>
        protected void RejectChanges()
        {
            foreach (KeyValuePair<Type, EntitySet> entitySetItem in this._entitySets)
            {
                EntitySet set = entitySetItem.Value;
                ((IRevertibleChangeTracking)set).RejectChanges();
            }
            Debug.Assert(!this.HasChanges, "EntityContainer.HasChanges should be false");
        }

        /// <summary>
        /// Load the specified collection of entities into this <see cref="EntityContainer"/>.
        /// </summary>
        /// <param name="entities">The entities to load</param>
        /// <returns>The set of entities loaded. In cases where entities in the set
        /// are already cached locally, the return set will contain the cached instances.</returns>
        public IEnumerable LoadEntities(IEnumerable entities)
        {
            return this.LoadEntities(entities, LoadBehavior.KeepCurrent);
        }

        /// <summary>
        /// Load the specified collection of entities into this <see cref="EntityContainer"/>.
        /// </summary>
        /// <param name="entities">The entities to load</param>
        /// <param name="loadBehavior">The <see cref="LoadBehavior"/> to use</param>
        /// <returns>The set of entities loaded. In cases where entities in the set
        /// are already cached locally, the return set will contain the cached instances.</returns>
        public IEnumerable LoadEntities(IEnumerable entities, LoadBehavior loadBehavior)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            // group the entities by type, and load each group into it's entity
            // list (ensuring we don't modify the relative ordering of each sequence 
            // of results per type).  We group by root entity type so that all derived
            // entities belong to the same group.
            List<Entity> loadedEntities = new List<Entity>();
            var entityGroups = entities.Cast<Entity>().GroupBy(p => (this.GetEntitySet(p.GetType())));
            foreach (var group in entityGroups)
            {
                EntitySet set = group.Key;
                loadedEntities.AddRange(set.LoadEntities(group, loadBehavior));
            }

            return loadedEntities;
        }

        internal Type ResolveEntityType(string typeName)
        {
            Debug.Assert(String.IsNullOrEmpty(typeName) == false, "typeName is null or empty");
            Debug.Assert(this._entityTypeMap.ContainsKey(typeName), "typeName is not found in entity type map");

            Type entityType = null;
            this._entityTypeMap.TryGetValue(typeName, out entityType);

            return entityType;
        }

        /// <summary>
        /// Returns the root entity type for a given entity type.
        /// </summary>
        /// <remarks>
        /// The root type is the least derived type in the entity's hierarchy.
        /// The entity lists are identified by the root entity type but can hold
        /// any entity in the hierarchy.  This function computes the root that
        /// should be used to dereference an entity list from an entity.
        /// <para>
        /// This function builds an entity to root map lazily.
        /// </para>
        /// </remarks>
        /// <param name="entityType">The type whose root is needed</param>
        /// <returns>The root of the hierarchy or null if <paramref name="entityType"/> was not derived from one of the known
        /// entity types for this container.</returns>
        private Type GetRootEntityType(Type entityType)
        {
            Type rootType = null;
            if (!this._entityRootTypes.TryGetValue(entityType, out rootType))
            {
                for (Type type = entityType; type != null; type = type.BaseType)
                {
                    if (this._entitySets.ContainsKey(type) || this._referencedEntitySets.ContainsKey(type))
                    {
                        rootType = type;
                        this._entityRootTypes[entityType] = rootType;
                        break;
                    }
                }
            }

            return rootType;
        }

        #region IRevertibleChangeTracking Members

        /// <summary>
        /// Reject all changes made to this container
        /// </summary>
        void IRevertibleChangeTracking.RejectChanges()
        {
            this.RejectChanges();
        }

        #endregion

        #region IChangeTracking Members

        /// <summary>
        /// Gets a value indicating whether this container currently has any pending changes
        /// </summary>
        bool IChangeTracking.IsChanged
        {
            get
            {
                return this.HasChanges;
            }
        }

        /// <summary>
        /// Accept all changes made to this container
        /// </summary>
        void IChangeTracking.AcceptChanges()
        {
            this.AcceptChanges();
        }

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
