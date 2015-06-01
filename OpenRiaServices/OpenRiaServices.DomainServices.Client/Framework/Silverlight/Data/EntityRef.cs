using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents an reference to an associated entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the associated <see cref="Entity"/></typeparam>
    public sealed class EntityRef<TEntity> : IEntityRef where TEntity : Entity
    {
        private AssociationAttribute _assocAttribute;
        private Entity _parent;
        private EntitySet _sourceSet;
        private Func<TEntity, bool> _entityPredicate;
        private TEntity _entity;
        private bool _hasAssignedEntity;
        private string _memberName;
        private bool _hasLoadedEntity;
        private bool _isComposition;

        /// <summary>
        /// Initializes a new instance of the EntityRef class
        /// </summary>
        /// <param name="parent">The entity that this association is a member of</param>
        /// <param name="memberName">The name of this EntityRef member on the parent entity</param>
        /// <param name="entityPredicate">The function used to filter the associated entity.</param>
        public EntityRef(Entity parent, string memberName, Func<TEntity, bool> entityPredicate)
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
            this._memberName = memberName;

            PropertyInfo propInfo = this._parent.GetType().GetProperty(memberName, OpenRiaServices.DomainServices.Client.MetaType.MemberBindingFlags);
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

            this._parent.SetEntityRef(memberName, this);

            this._parent.PropertyChanged += this.ParentEntityPropertyChanged;
        }

        /// <summary>
        /// Gets or sets the associated <see cref="Entity"/>
        /// </summary>
        public TEntity Entity
        {
            get
            {
                // if we have assigned a value, or the cached entity is still valid,
                // return it
                if (this._hasAssignedEntity || (this._entity != null && this._entityPredicate(this._entity)))
                {
                    return this._entity;
                }

                // We need to query the entity set if we are attached
                this._entity = null;
                if (this._parent.EntitySet != null)
                {
                    // Since this is the first time the entity has been returned, we don't
                    // need to send a property change notification.
                    EntitySet set = this._parent.EntitySet.EntityContainer.GetEntitySet(typeof(TEntity));
                    this._entity = this.GetSingleMatch(set);

                    if (this._entity != null && this._isComposition)
                    {
                        // if this is a compositional association, set the
                        // entity's parent
                        this._entity.SetParent(this._parent, this._assocAttribute);
                    }

                    // record the fact that we've queried for the entity
                    this._hasLoadedEntity = true;

                    // once we have returned a value we need to begin monitoring the 
                    // set so we can raise property change notifications when the 
                    // referenced entity is added or removed
                    this.MonitorEntitySet();
                }

                return this._entity;
            }
            set
            {
                if (value != null && this.SourceSet != null)
                {
                    this.SourceSet.EntityContainer.CheckCrossContainer(value);
                }

                bool entityChanged = this._entity != value;
                if (this.SourceSet != null && value != null)
                {
                    if (!this.SourceSet.IsAttached(value))
                    {
                        // if the entity is unattached, we infer it as an Add on its entity set
                        value.IsInferred = true;
                        this.SourceSet.Add(value);
                    }
                    else if (this._isComposition && value.EntityState == EntityState.Deleted)
                    {
                        // if a deleted entity is added to a compositional association,
                        // the delete should be undone
                        this.SourceSet.Add(value);
                    }
                }

                if (this._isComposition && entityChanged)
                {
                    if (value != null)
                    {
                        value.SetParent(this._parent, this._assocAttribute);
                    }
                    else
                    {
                        // when a composed entity is removed from its EntityRef,
                        // its inferred as a delete
                        if (this._sourceSet != null && this._sourceSet.IsAttached(this._entity))
                        {
                            this._sourceSet.Remove(this._entity);
                        }
                    }

                    this._parent.OnChildUpdate();
                }

                // Once a value is explicitly assigned externally,
                // we record that fact and return that value for now on.
                this._hasAssignedEntity = true;

                // we set directly here w/o raising a change notification,
                // since it's the job of generated code to raise notifications
                // when the member is directly set.
                this._entity = value;
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
        /// Set the entity value and raise a property changed notification if required.
        /// All internal assignments should be made through this method.
        /// </summary>
        /// <param name="value">The new entity instance</param>
        private void SetValue(TEntity value)
        {
            if (this._entity != value)
            {
                this._entity = value;

                if (this._entity != null && this._isComposition)
                {
                    // if this is a compositional association, set the
                    // entity's parent
                    this._entity.SetParent(this._parent, this._assocAttribute);
                }

                this._parent.RaisePropertyChanged(this._memberName);
            }
        }

        /// <summary>
        /// When filtering entities during query execution against the source set we don't want
        /// to include New entities, to ensure that we don't get false positives in cases where 
        /// the entity's FK members are auto-genned on the server or haven't been set yet.
        /// </summary>
        /// <param name="entity">The entity to filter</param>
        /// <returns>A <see cref="Boolean"/> value indicating whether or not the <paramref name="entity"/> should be filtered.</returns>
        private bool Filter(TEntity entity)
        {
            return entity.EntityState != EntityState.New && this._entityPredicate(entity);
        }

        /// <summary>
        /// Search the specified entity collection for a single match. If there is more than one match
        /// null will be returned. More than one match may occur in some update scenarios as entity FK
        /// values transition. However for unmodified data, there should only be a single match, and more
        /// than one represents invalid data (FKs should be unique).
        /// </summary>
        /// <param name="entities">The collection of entities to search.</param>
        /// <returns>The entity or null.</returns>
        private TEntity GetSingleMatch(IEnumerable entities)
        {
            TEntity entity = null;
            foreach (TEntity currEntity in entities.OfType<TEntity>().Where(this.Filter))
            {
                if (entity != null)
                {
                    return null;
                }
                entity = currEntity;
            }
            return entity;
        }

        /// <summary>
        /// Subscribe to the source set's CollectionChanged event, so we can monitor additions to and
        /// removals from the set so we can update the status of our reference and raise the required
        /// property change notifications.
        /// </summary>
        private void MonitorEntitySet()
        {
            if (this._parent.EntitySet != null)
            {
                // its expensive to monitor the source set for changes, so we only monitor when
                // we actually have a loaded or assigned entity
                if (this._hasLoadedEntity || this._hasAssignedEntity)
                {
                    if (this._sourceSet != null)
                    {
                        // Make sure we unsubscribe from any sets we may have already subscribed to (e.g. in case 
                        // of inferred adds). If we didn't already subscribe, this will be a no-op.
                        ((INotifyCollectionChanged)this._sourceSet).CollectionChanged -= this.SourceSet_CollectionChanged;
                        this._sourceSet.RegisterAssociationCallback(this._assocAttribute, this.OnEntityAssociationUpdated, false);
                    }

                    // parent was attached
                    this._sourceSet = this._parent.EntitySet.EntityContainer.GetEntitySet(typeof(TEntity));
                    ((INotifyCollectionChanged)this._sourceSet).CollectionChanged += this.SourceSet_CollectionChanged;
                    this._sourceSet.RegisterAssociationCallback(this._assocAttribute, this.OnEntityAssociationUpdated, true);
                }
            }
            else if ((this._parent.EntitySet == null) && (this._sourceSet != null))
            {
                // parent was detached
                ((INotifyCollectionChanged)this._sourceSet).CollectionChanged -= this.SourceSet_CollectionChanged;
                this._sourceSet.RegisterAssociationCallback(this._assocAttribute, this.OnEntityAssociationUpdated, false);
                this._sourceSet = null;
            }
        }

        /// <summary>
        /// Whenever the source set changes, we need to run our predicate against the
        /// added/removed entities and if we get any matches we propagate update our cached
        /// reference and raise the property changed notification for this EntityRef property
        /// on the parent.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The collection changed event arguments.</param>
        private void SourceSet_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (this._parent.EntityState != EntityState.New &&
                args.Action == NotifyCollectionChangedAction.Add)
            {
                TEntity newEntity = this.GetSingleMatch(args.NewItems);
                if ((newEntity != null) && (newEntity != this._entity))
                {
                    // if the referenced Entity has been added to the source EntitySet,
                    // we cache it here and raise the notification
                    this.SetValue(newEntity);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                if (this._entity != null && args.OldItems.OfType<TEntity>().Contains(this._entity))
                {
                    // if the referenced Entity has been removed from the source EntitySet,
                    // we need to clear out our cached reference and raise the notification
                    this.SetValue(null);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                if (this._entity != null)
                {
                    // if the source EntitySet has been reset, if we've loaded
                    // the referenced entity, clear out our cached value and raise 
                    // the notification
                    this.SetValue(null);
                }
            }
        }

        /// <summary>
        /// Callback for when an entity in the source set changes such that we need to reevaluate
        /// our reference. This could be because an FK member for the association
        /// has changed, or when the entity state transitions to Unmodified.
        /// </summary>
        /// <param name="entity">The entity that has changed</param>
        private void OnEntityAssociationUpdated(Entity entity)
        {
            if (entity.EntityState == EntityState.New && entity.IsMergingState)
            {
                // We don't want to perform dynamic updates when merging store state
                // into new entities.
                return;
            }

            TEntity typedEntity = entity as TEntity;

            if (typedEntity != null && (this._hasLoadedEntity || this._hasAssignedEntity))
            {
                if (this._entity != typedEntity && this._parent.EntityState != EntityState.New && this.Filter(typedEntity))
                {
                    // Set the entity. When setting, we use the stronger Filter to
                    // filter out New entities.
                    this.SetValue(typedEntity);
                }
                else if (this._entity == typedEntity && !this._entityPredicate(typedEntity))
                {
                    // We were pointing at the entity but it is no longer a match,
                    // so we need to remove our reference.
                    // Here we use the predicate directly, since even if the referenced
                    // entity is New if it no longer matches the reference should be cleared.
                    this.SetValue(null);
                }
            }
        }

        /// <summary>
        /// Called whenever the parent entity's <see cref="EntitySet"/> membership changes,
        /// allowing us to navigate to our source set for this collection.
        /// </summary>
        private void OnEntitySetChanged()
        {
            this.MonitorEntitySet();
        }

        /// <summary>
        /// PropertyChanged handler for the parent entity
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The property changed event arguments.</param>
        private void ParentEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // only need to reset if we have a cached value
            if (!this._hasLoadedEntity && !this._hasAssignedEntity)
            {
                return;
            }

            // Reset the loaded entity as needed.
            if (this._assocAttribute.ThisKeyMembers.Contains(e.PropertyName))
            {
                // a key member for this association was updated
                // so we need to reset
                this._entity = null;
                this._hasAssignedEntity = false;
                this._hasLoadedEntity = false;

                this._parent.RaisePropertyChanged(this._memberName);
            }
        }

        #region IEntityRef Members
        AssociationAttribute IEntityRef.Association
        {
            get
            {
                return this._assocAttribute;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this EntityRef has been loaded or
        /// has had a value assigned to it.
        /// </summary>
        bool IEntityRef.HasValue
        {
            get
            {
                return this._entity != null;
            }
        }

        Entity IEntityRef.Entity
        {
            get
            {
                return this.Entity;
            }
        }

        Func<Entity, bool> IEntityRef.Filter
        {
            get
            {
                return (Entity e) => this._entityPredicate((TEntity)e);
            }
        }
        #endregion
    }

    /// <summary>
    /// Internal interface providing loosely typed access to <see cref="EntityRef&lt;TEntity&gt;"/> members needed
    /// by the framework.
    /// </summary>
    // TODO : Consider making this interface (or a subset of it) public
    internal interface IEntityRef
    {
        /// <summary>
        /// Gets the AssociationAttribute for this reference.
        /// </summary>
        AssociationAttribute Association
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this EntityRef has been loaded or
        /// has had a value assigned to it.
        /// </summary>
        bool HasValue
        {
            get;
        }

        /// <summary>
        /// Gets the referenced entity loading it if it hasn't been loaded yet.
        /// To avoid the deferred load, inspect the HasValue property first.
        /// </summary>
        Entity Entity
        {
            get;
        }

        /// <summary>
        /// Gets the underlying filter method for the EntityRef.
        /// </summary>
        Func<Entity, bool> Filter
        {
            get;
        }
    }
}
