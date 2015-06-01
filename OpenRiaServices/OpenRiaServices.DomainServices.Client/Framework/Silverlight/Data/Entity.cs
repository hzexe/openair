using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Base class for all entity types.
    /// </summary>
    [DataContract]
    public abstract partial class Entity : IEditableObject, INotifyPropertyChanged, IRevertibleChangeTracking
    {
        private Action _setChangedCallback;
        private EditSession _editSession;
        private IList<EntityAction> _customMethodInvocations;
        private EntityConflict _conflict;
        private EntitySet _lastSet;
        private EntitySet _entitySet;
        private EntityState _entityState = EntityState.Detached;
        private IDictionary<string, object> _originalValues;
        private IDictionary<string, IEntityRef> _entityRefs;
        private PropertyChangedEventHandler _propChangedHandler;
        private EntityValidationResultCollection _validationErrors;
        private bool _isApplyingState;
        private bool _isDeserializing;
        private bool _isInferred;
        private bool _isMerging;
        private bool _isSubmitting;

        private bool _trackChanges;
        private Entity _parent;
        private AssociationAttribute _parentAssociation;
        private bool _hasChildChanges;
        private Dictionary<string, ComplexObject> _trackedInstances;
        private MetaType _metaType;


        /// <summary>
        /// Protected constructor since this is an abstract class
        /// </summary>
        protected Entity()
        {
        }

        /// <summary>
        /// Gets the map of child ComplexObject instances currently being
        /// tracked by this entity.
        /// </summary>
        private Dictionary<string, ComplexObject> TrackedInstances
        {
            get
            {
                if (this._trackedInstances == null)
                {
                    this._trackedInstances = new Dictionary<string, ComplexObject>();
                }
                return this._trackedInstances;
            }
        }

        /// <summary>
        /// Gets the parent of this entity, if this entity is part of
        /// a composition relationship.
        /// </summary>
        internal Entity Parent
        {
            get
            {
                return this._parent;
            }
        }

        /// <summary>
        /// Gets the parent association for this entity.
        /// </summary>
        internal AssociationAttribute ParentAssociation
        {
            get
            {
                return this._parentAssociation;
            }
        }

        /// <summary>
        /// Gets the MetaType for this entity.
        /// </summary>
        internal MetaType MetaType
        {
            get
            {
                if (this._metaType == null)
                {
                    var metaType  = MetaType.GetMetaType(this.GetType());
                    this._metaType = metaType;

                    if (!metaType.IsLegacyEntityActionsDiscovered)
                    {
                        // Trigger entities using old code generation to call UpdateActionState for all Entity Actions
#pragma warning disable 618
                        this.OnActionStateChanged();
#pragma warning restore 618
                        metaType.IsLegacyEntityActionsDiscovered = true;
                    }
                }

                return this._metaType;
            }
        }

        /// <summary>
        /// Sets the parent association info for this entity.
        /// </summary>
        /// <remarks>
        /// Since a Type can have multiple compositions of the same Type, to
        /// identify a parent association, we must track both the parent instance
        /// AND the association.
        /// </remarks>
        /// <param name="parent">The parent.</param>
        /// <param name="association">The parent association.</param>
        internal void SetParent(Entity parent, AssociationAttribute association)
        {
            if (this._parent != parent)
            {
                if (parent == this)
                {
                    throw new InvalidOperationException(Resource.Entity_ChildCannotBeItsParent);
                }

                this._parent = parent;
            }

            this._parentAssociation = association;
        }

        /// <summary>
        /// Gets conflict information for this entity after a submit
        /// operation. Returns null if there are no conflicts.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(AutoGenerateField = false)]
        public EntityConflict EntityConflict
        {
            get
            {
                return this._conflict;
            }
            internal set
            {
                if (this._conflict != value)
                {
                    this._conflict = value;
                    this.RaisePropertyChanged("EntityConflict");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is currently an uncommitted
        /// edit session in progress for this entity. This is the case when
        /// BeginEdit has been called, but EndEdit/CancelEdit have not.
        /// </summary>
        internal bool IsEditing
        {
            get
            {
                return this._editSession != null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current entity state
        /// has been inferred
        /// </summary>
        internal bool IsInferred
        {
            get
            {
                return this._isInferred;
            }
            set
            {
                this._isInferred = value;
            }
        }

        /// <summary>
        /// Gets the collection of validation errors for this entity. 
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(AutoGenerateField = false)]
        public ICollection<ValidationResult> ValidationErrors
        {
            get
            {
                if (this._validationErrors == null)
                {
                    this._validationErrors = new EntityValidationResultCollection(this);
                }

                return this._validationErrors;
            }
        }

        /// <summary>
        /// Gets the collection of validation errors as a <see cref="ValidationResultCollection"/>.
        /// </summary>
        internal ValidationResultCollection ValidationResultCollection
        {
            get
            {
                // Use the property getter so that the lazy instantiation occurs
                return (ValidationResultCollection)this.ValidationErrors;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this entity has any validation errors.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(AutoGenerateField = false)]
        public bool HasValidationErrors
        {
            get
            {
                return this.ValidationErrors.Any();
            }
        }

        /// <summary>
        /// Gets the current state of this <see cref="Entity"/>
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(AutoGenerateField = false)]
        public EntityState EntityState
        {
            get
            {
                return this._entityState;
            }
            private set
            {
                if (this._entityState != value)
                {
                    bool hasChangesChanged = (this._entityState == EntityState.Modified && value == EntityState.Unmodified)
                                             || (this._entityState == EntityState.Unmodified && value == EntityState.Modified);
                    this._entityState = value;
                    this.RaisePropertyChanged("EntityState");

                    // track or untrack this Entity as required
                    EntitySet entitySet = this.LastSet;
                    if (entitySet != null)
                    {
                        bool isInteresting = this._entityState != EntityState.Unmodified
                            && this._entityState != EntityState.Detached;
                        entitySet.TrackAsInteresting(this, isInteresting);
                    }

                    if (hasChangesChanged)
                    {
                        this.RaisePropertyChanged("HasChanges");

                        // if we're a composed entity we need to notify our parent
                        // that our state has changed
                        if (this.Parent != null && this._trackChanges)
                        {
                            this.Parent.OnChildUpdate();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method is called whenever a child entity changes state.
        /// We must transition our own state accordingly.
        /// </summary>
        internal void OnChildUpdate()
        {
            if (this._trackChanges && this.EntityState != EntityState.Deleted)
            {
                bool prevHasChanges = this.HasChanges;

                // determine if there are currently any modified entities that we
                // are the parent of
                this._hasChildChanges = this.LastSet.EntityContainer.HasChildChanges(this);

                if (!prevHasChanges && this._hasChildChanges)
                {
                    // if we were unmodified and a child has changed,
                    // transition to modified
                    this.EntityState = EntityState.Modified;
                }
                else if (prevHasChanges && !this._hasChildChanges)
                {
                    // if we were modified and the last child change has been reverted
                    // and we don't have any actual changes of our own, transition back
                    // to Unmodified
                    if (!this.EntityActions.Any() && !this.HasPropertyChanges)
                    {
                        this.EntityState = EntityState.Unmodified;
                    }
                }
            }

            if (this.EntitySet != null)
            {
                // when a child has been changed in any way, the parent becomes
                // interesting
                this.EntitySet.TrackAsInteresting(this, true);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this entity has had any data
        /// member modifications.
        /// </summary>
        internal bool HasPropertyChanges
        {
            get
            {
                return this._originalValues != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this entity currently has any
        /// changes to composed children.
        /// </summary>
        internal bool HasChildChanges
        {
            get
            {
                return this._hasChildChanges;
            }
        }

        /// <summary>
        /// Gets the collection of properties that are currently modified
        /// </summary>
        internal IEnumerable<PropertyInfo> ModifiedProperties
        {
            get
            {
                return this.MetaType.DataMembers.Select(p => p.Member).Where(this.PropertyHasChanged);
            }
        }

        /// <summary>
        /// Gets the <see cref="EntitySet"/> this <see cref="Entity"/> is a member of. The value will be null
        /// if the entity is Detached or has been removed from the set.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        protected internal EntitySet EntitySet
        {
            get
            {
                return this._entitySet;
            }
            internal set
            {
                var previousValue = _entitySet;
                this._entitySet = value;
                if (value != null)
                {
                    // save the last non-null set value 
                    this._lastSet = value;
                }

                // Perform any action state updates required as a result of
                // attaching/detaching this entity.

                // If EntitySet changes between null and not null then the CanInvokeXXX might have changed
                // value (unless entity is deleted or submitting in which case it will always be false both before and after)
                if ((previousValue == null && value != null) || (previousValue != null && value == null)
                    && (EntityState != EntityState.Deleted && !this.IsSubmitting))
                {
                    RaiseCanInvokeChanged();
                }

                if (this._setChangedCallback != null)
                {
                    // invoke all registered callbacks (this is a multicast delegate)
                    this._setChangedCallback();
                }
            }
        }

        private void RaiseCanInvokeChanged()
        {
            foreach (var customMethod in MetaType.GetEntityActions())
            {
                this.RaisePropertyChanged(customMethod.CanInvokePropertyName);
            }
        }

        /// <summary>
        /// Gets the last set that this entity was attached to, possibly
        /// returning null. Generally you want to use EntitySet rather than
        /// this member, however in some cases (e.g. for deleted entities)
        /// EntitySet will be null and we need to get back to the set.
        /// </summary>
        internal EntitySet LastSet
        {
            get
            {
                return this.EntitySet != null ? this.EntitySet : this._lastSet;
            }
        }

        /// <summary>
        /// Gets or sets the custom method invocation on this entity (if any)
        /// while bypassing lots of the validation (this is only used by the old tests)
        /// </summary>
        [Obsolete("Use EntityActions instead")]
        internal EntityAction CustomMethodInvocation
        {
            get
            {
                if (this._customMethodInvocations == null)
                    return null;
                else
                    return _customMethodInvocations.SingleOrDefault();
            }
            set
            {
                if (CustomMethodInvocation != value)
                {
                    bool wasReadOnly = this.IsReadOnly;

                    UndoAllEntityActions(preventRaiseReadOnly: true);

                    if (value != null)
                    {
                        // Many of the old tests uses invalid method names, construct a dummy EntityActionAttribute
                        // if no exists
                        var customMethod = MetaType.GetEntityAction(value.Name);
                        if (customMethod == null)
                            customMethod = new EntityActionAttribute(value.Name, false);

                        InvokeActionCore(value, customMethod);
                    }
                        

                    if (value != null && this._entityState == EntityState.Unmodified)
                    {
                        // invoking on an unmodified entity makes it modified, but invoking
                        // on an entity in any other state should not change state
                        this.EntityState = EntityState.Modified;
                    }

                    if (wasReadOnly != this.IsReadOnly)
                    {
                        this.RaisePropertyChanged("IsReadOnly");
                    }
                }
            }
        }

        /// <summary>
        /// Undo all currently queued entity actions
        /// </summary>
        /// <param name="preventRaiseReadOnly">if set to <c>true</c> then PropertyChange event for IsReadOnly is never raised.</param>
        private void UndoAllEntityActions(bool preventRaiseReadOnly = false)
        {
            bool wasReadOnly = this.IsReadOnly;

            while (this._customMethodInvocations != null && this._customMethodInvocations.Count > 0)
                this.UndoAction(this._customMethodInvocations[0]);

            if (IsReadOnly != wasReadOnly && !preventRaiseReadOnly)
            {
                RaisePropertyChanged("IsReadOnly");
            }
        }

        /// <summary>
        /// Gets the original values of this Entity's properties before modification
        /// </summary>
        internal IDictionary<string, object> OriginalValues
        {
            get
            {
                return this._originalValues;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Entity"/> currently has any pending changes.
        /// If this <see cref="Entity"/> has compositional associations and any children have changes
        /// <c>true</c> will be returned.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(AutoGenerateField = false)]
        public bool HasChanges
        {
            get
            {
                return this._entityState == EntityState.Modified;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this entity is currently being deserialized.
        /// </summary>
        protected internal bool IsDeserializing
        {
            get
            {
                return this._isDeserializing;
            }
        }

        internal bool IsApplyingState
        {
            get
            {
                return this._isApplyingState;
            }
        }

        protected internal bool IsMergingState
        {
            get { return this._isMerging; }
            private set
            {
                this._isMerging = value;

                MetaType metaType = MetaType.GetMetaType(this.GetType());

                foreach (MetaMember metaMember in metaType.DataMembers.Where(f => f.IsComplex && !f.IsCollection))
                {
                    PropertyInfo propertyInfo = metaMember.Member;
                    ComplexObject propertyValue = propertyInfo.GetValue(this, null) as ComplexObject;
                    if (propertyValue != null)
                    {
                        propertyValue.IsMergingState = this._isMerging;
                    }

                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this entity is currently in a read-only state.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public bool IsReadOnly
        {
            get
            {
                // whenever we are applying state we temporarily remove read-only to
                // permit the property setters to be called
                if (this.IsApplyingState)
                {
                    return false;
                }

                // if we're attached as Unmodified and the set doesn't support edits
                // we're read-only
                if (this.EntitySet != null && this.EntityState != EntityState.New && !this.EntitySet.CanEdit)
                {
                    return true;
                }

                // if we are currently submitting, we are read-only (regardless of errors being present)
                if (this.IsSubmitting)
                {
                    return true;
                }

                // if we have invoked a custom method and no errors or conflicts are present, we are read-only
                bool hasErrors = (this.HasValidationErrors || this.EntityConflict != null);
                return (EntityActions.Any() && !hasErrors);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is part of a submit in progress.
        /// </summary>
        internal bool IsSubmitting
        {
            get
            {
                return this._isSubmitting;
            }
            set
            {
                if (this._isSubmitting != value)
                {
                    bool wasReadOnly = this.IsReadOnly;
                    this._isSubmitting = value;
                    if (wasReadOnly != this.IsReadOnly)
                    {
                        this.RaisePropertyChanged("IsReadOnly");
                    }

                    foreach (var entityAction in MetaType.GetEntityActions())
                    {
                        // Check if CanInvoke might have changed
                        if (this.EntityState != EntityState.Deleted 
                            && this.EntitySet != null)
                        {
                            if (entityAction.AllowMultipleInvocations || !this.IsActionInvoked(entityAction.Name))
                                RaisePropertyChanged(entityAction.CanInvokePropertyName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Indicates whether the specified action has been invoked.
        /// </summary>
        /// <param name="name">The name of the action corresponding to a custom method.</param>
        /// <returns>True if the custom method has been invoked.</returns>
        protected bool IsActionInvoked(string name)
        {
            return this.EntityActions.Any(p => p.Name == name);
        }

        /// <summary>
        /// Called whenever an entity is loaded into an <see cref="EntitySet"/>.
        /// </summary>
        /// <param name="isInitialLoad">True when the entity is being loaded into the set for the first time, false otherwise.</param>
        internal protected virtual void OnLoaded(bool isInitialLoad)
        {
        }

        /// <summary>
        /// Gets the original state for this entity.
        /// </summary>
        /// <returns>The entity in its original state if the entity has had property modifications, null otherwise.</returns>
        public Entity GetOriginal()
        {
            if (this.OriginalValues == null)
            {
                return null;
            }

            Entity original = (Entity)Activator.CreateInstance(this.GetType());
            original.ApplyState(this.OriginalValues);
            return original;
        }

        /// <summary>
        /// Resets internal entity state. For example, when the entity is 
        /// being attached (or reattached) to a set.
        /// </summary>
        internal void Reset()
        {
            this._originalValues = null;
            this._editSession = null;
            this._trackChanges = false;
            UndoAllEntityActions();
            if (this._validationErrors != null)
            {
                this._validationErrors.Clear();
            }
            this.EntityConflict = null;
            this.EntitySet = null;
            this._lastSet = null;
            this.EntityState = EntityState.Detached;
            this._hasChildChanges = false;
        }

        /// <summary>
        /// Begin change tracking this entity
        /// </summary>
        internal void StartTracking()
        {
            this._trackChanges = true;
        }

        /// <summary>
        /// Stop change tracking this entity
        /// </summary>
        internal void StopTracking()
        {
            this._trackChanges = false;
        }

        /// <summary>
        /// Accept the current changes to this <see cref="Entity"/> applying the proper
        /// state transitions. If this <see cref="Entity"/> has compositional associations,
        /// any changes made to those associations or the child entities themselves will also
        /// be accepted. 
        /// </summary>
        protected void AcceptChanges()
        {
            if (this.EntityState == EntityState.Unmodified ||
                this.EntityState == EntityState.Detached)
            {
                // if we're detached or have no changes, noop after
                // closing any in progress edit session
                this._editSession = null;
                return;
            }

            EntitySet entitySet = this.LastSet;

            // Accept any child changes. Note, we must accept child changes before setting our own
            // state to Unmodified. This avoids a situation where we get notifications from child
            // entities that cause our state to flip back to Modified.
            if (entitySet != null)
            {
                // accept any child changes
                entitySet.EntityContainer.CompleteChildChanges(this, true);
            }

            if (this.EntityState == EntityState.New)
            {
                this.EntityState = EntityState.Unmodified;
                if (entitySet != null)
                {
                    entitySet.AddToCache(this);

                    // currently associations do not include New entities when loaded
                    // unless those entities have been explicitly added to the collection.
                    // Therefore, once the entity transitions from New to Unmodified, we need
                    // to process association updates.
                    if (this.IsEditing)
                    {
                        // only update if we're editing, in which case the updates
                        // have been deferred
                        entitySet.UpdateRelatedAssociations(this, "EntityState");
                    }
                }
                this.StartTracking();
            }
            else if (this.EntityState == EntityState.Modified)
            {
                this.EntityState = EntityState.Unmodified;
                this._originalValues = null;
            }
            else if (this.EntityState == EntityState.Deleted)
            {
                this.StopTracking();
                if (entitySet != null)
                {
                    entitySet.RemoveFromCache(this);
                }
                // move back to the default state
                this.EntityState = EntityState.Detached;
            }

            if (entitySet != null)
            {
                // remove from the interesting entities set
                entitySet.TrackAsInteresting(this, false);
            }

            // need to end any in progress edit session
            this._editSession = null;

            // clear all custom method invocations
            this.UndoAllEntityActions();

            if (this._validationErrors != null)
            {
                this._validationErrors.Clear();
            }
            this.EntityConflict = null;
            this.IsInferred = false;
            this._hasChildChanges = false;
            Debug.Assert(!this.HasChanges, "Entity.HasChanges should be false");
        }

        /// <summary>
        /// Revert all property changes made to this entity back to their original values. This method
        /// does not revert <see cref="EntitySet"/> Add/Remove operations, so if this <see cref="Entity"/>
        /// is New or Deleted, this method does nothing. This method also reverts any pending custom
        /// method invocations on the entity. If this <see cref="Entity"/> has compositional associations,
        /// any changes made to those associations or the child entities themselves will be reverted.
        /// </summary>
        protected void RejectChanges()
        {
            if (this.EntityState == EntityState.Unmodified ||
                this.EntityState == EntityState.Detached)
            {
                // if we're detached or have no changes, noop after
                // closing any in progress edit session
                this._editSession = null;
                return;
            }

            EntitySet entitySet = this.LastSet;

            // Reject any child changes. Note, we must reject child changes before setting our own
            // state to Unmodified. This avoids a situation where we get notifications from child
            // entities that cause our state to flip back to Modified.
            if (entitySet != null)
            {
                entitySet.EntityContainer.CompleteChildChanges(this, false);
            }

            if (this._entityState == EntityState.Modified || this.Parent != null)
            {
                if (entitySet != null)
                {
                    // undo any compositional child updates by undoing
                    // the appropriate set operation
                    if (this.Parent != null)
                    {
                        if (this.EntityState == EntityState.Deleted)
                        {
                            entitySet.Add(this);
                        }
                        else if (this.EntityState == EntityState.New)
                        {
                            entitySet.Remove(this);
                            this.EntityState = EntityState.Detached;
                        }
                    }
                }

                if (this._originalValues != null)
                {
                    this.ApplyState(this._originalValues);
                    this._originalValues = null;
                }

                if (entitySet != null && this.EntityState == EntityState.Modified)
                {
                    this.EntityState = EntityState.Unmodified;
                    entitySet.TrackAsInteresting(this, false);
                }
            }

            // need to end any in progress edit session
            this._editSession = null;

            // Reseting custom method invocation needs to be outside here 
            // since invocation could be combined with other operations
            // which result in a non-Modified state
            UndoAllEntityActions();

            // Empty out the error collections
            if (this._validationErrors != null)
            {
                this._validationErrors.Clear();
            }
            this.EntityConflict = null;
            this._hasChildChanges = false;
            Debug.Assert(!this.HasChanges, "Entity.HasChanges should be false");
        }

        internal void InitializeNew()
        {
            this.EntityState = EntityState.New;
        }

        internal void MarkDeleted()
        {
            this.EntityState = EntityState.Deleted;
            bool wasReadOnly = this.IsReadOnly;

            // Delete operation overrides invocation
            var previouslyInvoked = _customMethodInvocations;
            _customMethodInvocations = null;

            foreach (var customMethodInfo in MetaType.GetEntityActions())
            {
                bool wasInvoked = previouslyInvoked != null 
                    && previouslyInvoked.Any(action => action.Name == customMethodInfo.Name);

                // For the current implementation, we always raise the CanInvoke change notification
                RaisePropertyChanged(customMethodInfo.CanInvokePropertyName);
                if (wasInvoked)
                    RaisePropertyChanged(customMethodInfo.IsInvokedPropertyName);
            }

            if (wasReadOnly != this.IsReadOnly)
                RaisePropertyChanged("IsReadOnly");
        }

        internal void UndoDelete()
        {
            Debug.Assert(EntityState == EntityState.Deleted, "Can only call UndoDelete for a Deleted entity");
            Debug.Assert(EntitySet == null, "Should not have set EntitySet (setting EntitySet will call RaiseCanInvokeChanged)");

            // When undoing a delete, we need to revert back to
            // the previous modification state
            if (this._originalValues != null)
            {
                this.EntityState = EntityState.Modified;
            }
            else
            {
                this.EntityState = EntityState.Unmodified;
            }
        }

        internal void MarkUnmodified()
        {
            this.EntityState = EntityState.Unmodified;
        }

        internal void MarkDetached()
        {
            this.EntityState = EntityState.Detached;
        }

        internal IDictionary<string, object> ExtractState()
        {
            return ObjectStateUtility.ExtractState(this);
        }

        /// <summary>
        /// Gets the map of EntityRef member name to IEntityRef instance.
        /// </summary>
        private IDictionary<string, IEntityRef> EntityRefs
        {
            get
            {
                if (this._entityRefs == null)
                {
                    this._entityRefs = new Dictionary<string, IEntityRef>();
                }
                return this._entityRefs;
            }
        }

        /// <summary>
        /// Associates and caches the provided <see cref="IEntityRef"/> for the
        /// specified EntityRef member.
        /// </summary>
        /// <remarks>This method is called when an EntityRef field is initialized,
        /// and allows us access to the field w/o resorting to private reflection.</remarks>
        /// <param name="memberName">The name of the EntityRef member.</param>
        /// <param name="entityRef">The <see cref="IEntityRef"/> to associate.</param>
        internal void SetEntityRef(string memberName, IEntityRef entityRef)
        {
            this.EntityRefs[memberName] = entityRef;
        }

        /// <summary>
        /// Updates the original values with those of the specified
        /// entity. This method is used during refresh loading scenarios
        /// and conflict resolution to update original with the latest
        /// store values.
        /// </summary>
        /// <param name="entityStateToApply">IDictionary with the new original state.</param>
        internal void UpdateOriginalValues(IDictionary<string, object> entityStateToApply)
        {
            Debug.Assert(this._originalValues != null, "Should only call UpdateOriginalValues if the entity has original values.");
            this._originalValues = new Dictionary<string, object>(entityStateToApply);
        }

        /// <summary>
        /// Returns the <see cref="IEntityRef"/> corresponding to the specified
        /// EntityRef association member name.
        /// </summary>
        /// <param name="memberName">The name of the association member to get
        /// the <see cref="IEntityRef"/> for. If the EntityRef hasn't been initialized
        /// yet, null is returned.</param>
        /// <returns>The <see cref="IEntityRef"/> if the reference has been initialized,
        /// null otherwise.</returns>
        internal IEntityRef GetEntityRef(string memberName)
        {
            IEntityRef entityRef = null;
            this.EntityRefs.TryGetValue(memberName, out entityRef);
            return entityRef;
        }

        /// <summary>
        /// Apply the specified state to this entity instance using the RefreshCurrent
        /// merge strategy and normal change tracking.
        /// </summary>
        /// <param name="entityStateToApply">The state to apply</param>
        internal void ApplyState(IDictionary<string, object> entityStateToApply)
        {
            this.ApplyState(entityStateToApply, LoadBehavior.RefreshCurrent);
        }

        internal void ApplyState(IDictionary<string, object> entityStateToApply, LoadBehavior loadBehavior)
        {
            if (loadBehavior == LoadBehavior.KeepCurrent)
            {
                return;
            }

            try
            {
                // We use this state to suppress validation during property sets
                this._isApplyingState = true;

                ObjectStateUtility.ApplyState(this, entityStateToApply, this._originalValues, loadBehavior);
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }
                throw;
            }
            finally
            {
                this._isApplyingState = false;
            }
        }

        /// <summary>
        /// Updates the original values with those of the specified
        /// entity. This method is used during refresh loading scenarios
        /// and conflict resolution to update original with the latest
        /// store values.
        /// </summary>
        /// <param name="entity">Entity with the new original state.</param>
        internal void UpdateOriginalValues(Entity entity)
        {
            Debug.Assert(this._originalValues != null, "Should only call UpdateOriginalValues if the entity has original values.");
            this._originalValues = entity.ExtractState();
        }

        /// <summary>
        /// Merge differs from ApplyState in that its the merge of an entire
        /// entity as opposed to an arbitrary set (possibly subset) of values.
        /// Change tracking is suspended for the entity during the merge.
        /// </summary>
        /// <param name="otherEntity">The entity to merge into the current instance</param>
        /// <param name="loadBehavior">The load behavior to use</param>
        internal void Merge(Entity otherEntity, LoadBehavior loadBehavior)
        {
            //This is redundant with main merge, but it prevents doing undeeded ExtractStates
            if (loadBehavior == LoadBehavior.KeepCurrent)
            {
                return;
            }
            
            IDictionary<string, object> otherState = otherEntity.ExtractState();
            Merge(otherState, loadBehavior);
        }

        /// <summary>
        /// Merge differs from ApplyState in that its the merge of an entire
        /// entity as opposed to an arbitrary set (possibly subset) of values.
        /// Change tracking is suspended for the entity during the merge.
        /// </summary>
        /// <param name="otherEntity">The entity to merge into the current instance</param>
        /// <param name="loadBehavior">The load behavior to use</param>
        internal void Merge(IDictionary<string, object> otherState, LoadBehavior loadBehavior)
        {
            if (loadBehavior == LoadBehavior.KeepCurrent)
            {
                return;
            }

            // set this flag so change tracking and edit enforcement
            // is suspended during the merge
            this.IsMergingState = true;
            
            this.ApplyState(otherState, loadBehavior);

            // after the merge update our original state based on the
            // load behavior selected
            if (this._originalValues != null)
            {
                if (loadBehavior == LoadBehavior.RefreshCurrent)
                {
                    // when refreshing, the entity should no longer be
                    // considered modfied, so we wipe out original values
                    this._originalValues = null;

                    if (this.EntityState == EntityState.Modified 
                        && (this._customMethodInvocations == null || this._customMethodInvocations.Count == 0)
                        && !this.HasChildChanges)
                    {
                        // we've just reverted all property edits. If we're not modified for
                        // any other reason, transition to Unmodified
                        this.EntityState = EntityState.Unmodified;
                    }
                }
                else
                {
                    this._originalValues = otherState;
                }
            }

            this.IsMergingState = false;
            this.OnLoaded(false);
        }

        /// <summary>
        /// Returns true if the specified property has been modified
        /// </summary>
        /// <param name="prop">The property to check</param>
        /// <returns>true if the specified property has been modified</returns>
        private bool PropertyHasChanged(PropertyInfo prop)
        {
            return ObjectStateUtility.PropertyHasChanged(this, this._originalValues, prop);
        }

        /// <summary>
        /// Called when an <see cref="Entity"/> property has changed.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            // if we're in an edit session, we want to postpone association updates
            // until the edits are commited (EndEdit is called). Note: we only want
            // to process association updates here when we're attached.
            if (!this.IsEditing && this.EntitySet != null)
            {
                this.EntitySet.UpdateRelatedAssociations(this, e.PropertyName);
            }

            if (this._propChangedHandler != null)
            {
                this._propChangedHandler(this, e);
            }
        }

        /// <summary>
        /// Called from a property setter to notify the framework that an
        /// <see cref="Entity"/> data member has changed. This method performs 
        /// any required change tracking and state transitions.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void RaiseDataMemberChanged(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            this.OnDataMemberChanged();

            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

            // Note, RaiseDataMemberChanged is called on every property update. We need to avoid as
            // much overhead as possible.
            if (this.MetaType.HasComplexMembers)
            {
                // if the property is a complex type, we need to detach from any current
                // instance and attach to the new instance
                MetaMember metaMember = this.MetaType[propertyName];
                if (metaMember != null && metaMember.IsComplex && !metaMember.IsCollection)
                {
                    this.AttachComplexObjectInstance(metaMember);
                }
            }
        }

        /// <summary>
        /// When a complex property on this instance changes, this method performs
        /// the necessary attach/detach operations for the new instance.
        /// </summary>
        /// <param name="metaMember">The complex member.</param>
        private void AttachComplexObjectInstance(MetaMember metaMember)
        {
            // First check if the parent has an existing instance attached for this
            // property and detach if necessary.
            string memberName = metaMember.Member.Name;
            ComplexObject prevInstance = null;
            if (this.TrackedInstances.TryGetValue(memberName, out prevInstance))
            {
                prevInstance.Detach();
                this.TrackedInstances.Remove(memberName);
            }

            ComplexObject newInstance = (ComplexObject)metaMember.GetValue(this);
            if (newInstance != null)
            {
                // Attach to the new instance
                newInstance.Attach(this, memberName, this.OnDataMemberChanging, this.OnDataMemberChanged, this.OnMemberValidationChanged);
                this.TrackedInstances[memberName] = newInstance;

                // If the instance has validation errors, we need to sync them into our parent. This
                // needs to be done as a merge operation, since the parent may already have property
                // level errors for this member that must be retained.
                if (newInstance.HasValidationErrors)
                {
                    foreach (ValidationResult error in ValidationUtilities.ApplyMemberPath(newInstance.ValidationErrors, memberName))
                    {
                        this.ValidationResultCollection.Add(error);
                    }
                }
            }
        }

        private void OnMemberValidationChanged(string propertyName, IEnumerable<ValidationResult> validationResults)
        {
            this.ValidationResultCollection.ReplaceErrors(propertyName, validationResults);
        }

        private void OnDataMemberChanged()
        {
            if (this.EntitySet != null)
            {
                // During merge operations, we want to suspend change tracking
                if (!this._isMerging && this._trackChanges && EntityState != EntityState.Modified)
                {
                    this.EntityState = EntityState.Modified;
                }
            }
        }

        /// <summary>
        /// Called from a property setter to notify the framework that an
        /// <see cref="Entity"/> member has changed. This method does not
        /// perform any change tracking operations.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected internal void RaisePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called from a property setter to notify the framework that an
        /// <see cref="Entity"/> data member is about to be changed. This
        /// method performs any required change tracking and state transition
        /// operations.
        /// </summary>
        /// <param name="propertyName">The name of the property that is changing</param>
        protected void RaiseDataMemberChanging(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            this.OnDataMemberChanging();

            if (this.IsEditing)
            {
                this._editSession.OnDataMemberUpdate(propertyName);
            }
        }


        private void OnDataMemberChanging()
        {
            EntitySet set = this.LastSet;
            if (set != null)
            {
                // During merge operations, we want to suspend change tracking
                // and editability enforcement
                if (!this._isMerging)
                {
                    // if we're currently attached and the entity is not New,
                    // we need to throw if Edit is not supported
                    if (this.EntityState != EntityState.New)
                    {
                        set.EnsureEditable(EntitySetOperations.Edit);
                    }

                    if (this._trackChanges && this._originalValues == null)
                    {
                        this._originalValues = this.ExtractState();
                    }
                }
            }
        }

#if SILVERLIGHT
        /// <summary>
        /// Validate whether the specified value is valid for the specified property
        /// of the current Entity.
        /// </summary>
        /// <remarks>
        /// This method evaluates all the <see cref="ValidationAttribute"/>s associated with the specified property, accumulating
        /// the validation errors and surfacing them through the <see cref="ValidationErrors"/> property.  It also verifies
        /// the property is not read-only.
        /// <para>All validation logic is bypassed if this entity is currently being deserialized.</para>
        /// </remarks>
        /// <param name="propertyName">The name of the property to validate.  This name cannot be <c>null</c> or empty.</param>
        /// <param name="value">The value to test. It may be <c>null</c> if <c>null</c> is valid for the given property.</param>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="propertyName"/> is <c>null</c> or empty.</exception>
        /// <exception cref="InvalidOperationException"> is thrown if this property is marked with <see cref="EditableAttribute"/> 
        /// configured to prevent editing.</exception>
#else
        /// <summary>
        /// Validate whether the specified value is valid for the specified property
        /// of the current Entity.
        /// </summary>
        /// <remarks>
        /// This method evaluates all the <see cref="ValidationAttribute"/>s associated with the specified property
        /// and throws a <see cref="ValidationException"/> for the first <see cref="ValidationAttribute"/> that signals
        /// a validation error.  It also verifies the property is not read-only.
        /// <para>All validation logic is bypassed if this entity is currently being deserialized.</para>
        /// </remarks>
        /// <param name="propertyName">The name of the property to validate.  This name cannot be <c>null</c> or empty.</param>
        /// <param name="value">The value to test. It may be <c>null</c> if <c>null</c> is valid for the given property.</param>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="propertyName"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ValidationException"> is thrown if this value is not valid for the specified property.</exception>
        /// <exception cref="InvalidOperationException"> is thrown if this property is marked with <see cref="EditableAttribute"/> 
        /// configured to prevent editing.</exception>
#endif
        protected void ValidateProperty(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            if (this.IsDeserializing || this.IsApplyingState)
            {
                return;
            }

            // if we're currently attached and the entity is not New,
            // we need to throw if Edit is not supported
            if (this.EntitySet != null && this.EntityState != EntityState.New)
            {
                this.EntitySet.EnsureEditable(EntitySetOperations.Edit);
            }

            if (this.IsReadOnly)
            {
                throw new InvalidOperationException(Resource.Entity_ReadOnly);
            }

            MetaMember metaMember = this.MetaType[propertyName];
            if (metaMember == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resource.Property_Does_Not_Exist, this.MetaType.Type.Name, propertyName), "propertyName");
            }

            if (metaMember.EditableAttribute != null && !metaMember.EditableAttribute.AllowEdit &&
                !(metaMember.EditableAttribute.AllowInitialValue && (this.EntityState == EntityState.Detached || this.EntityState == EntityState.New)))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.Property_Is_ReadOnly, propertyName));
            }

            ComplexObject complexObject = value as ComplexObject;
            if (complexObject != null && complexObject.IsAttached)
            {
                throw new InvalidOperationException(Resource.ComplexType_InstancesCannotBeShared);
            }

            if (this.MetaType.RequiresValidation)
            {
                ValidationContext validationContext = this.CreateValidationContext();
                validationContext.MemberName = propertyName;
                this.ValidateProperty(validationContext, value);
            }
        }

#if SILVERLIGHT
        /// <summary>
        /// Validate whether the specified property value is valid for the specified <see cref="ValidationContext"/>.
        /// </summary>
        /// <remarks>
        /// This method evaluates all the <see cref="ValidationAttribute"/>s associated with the property
        /// indicated as the <see cref="ValidationContext.MemberName"/>, accumulating the validation errors
        /// and surfacing them through the <see cref="ValidationErrors"/> property.
        /// </remarks>
        /// <param name="validationContext">
        /// The <see cref="ValidationContext"/> representing the validation to be performed.
        /// <para>
        /// <see cref="ValidationContext"/>.<see cref="ValidationContext.MemberName"/> must indicate
        /// the name of the property to validate.
        /// </para>
        /// </param>
        /// <param name="value">The value to test. It may be <c>null</c> if <c>null</c> is valid for the given property.</param>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="validationContext"/> is <c>null</c>.</exception>
#else
        /// <summary>
        /// Validate whether the specified property value is valid for the specified <see cref="ValidationContext"/>.
        /// </summary>
        /// <remarks>
        /// This method evaluates all the <see cref="ValidationAttribute"/>s associated with the property
        /// indicated as the <see cref="ValidationContext.MemberName"/>, and throws a
        /// <see cref="ValidationException"/> for the first <see cref="ValidationAttribute"/> that signals
        /// a validation error.
        /// </remarks>
        /// <param name="validationContext">
        /// The <see cref="ValidationContext"/> representing the validation to be performed.
        /// <para>
        /// <see cref="ValidationContext"/>.<see cref="ValidationContext.MemberName"/> must indicate
        /// the name of the property to validate.
        /// </para>
        /// </param>
        /// <param name="value">The value to test. It may be <c>null</c> if <c>null</c> is valid for the given property.</param>
        /// <exception cref="ValidationException"> is thrown if this value is not valid for the specified property.</exception>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="validationContext"/> is null</exception>
#endif
        protected virtual void ValidateProperty(ValidationContext validationContext, object value)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException("validationContext");
            }

#if SILVERLIGHT
            List<ValidationResult> validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(value, validationContext, validationResults);

            // Replace the errors for this property
            this.ValidationResultCollection.ReplaceErrors(validationContext.MemberName, validationResults);
#else
            Validator.ValidateProperty(value, validationContext);
            this.ValidationResultCollection.ReplaceErrors(validationContext.MemberName, Enumerable.Empty<ValidationResult>());
#endif
        }

        /// <summary>
        /// Get the <see cref="ValidationContext"/> to be used for validation invoked
        /// from this <see cref="Entity"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="ValidationContext"/> instance, using the
        /// <see cref="EntityContainer.ValidationContext"/> as the parent context
        /// if available.
        /// </returns>
        private ValidationContext CreateValidationContext()
        {
            // Get the validation context from the entity container if available,
            // otherwise create a new context.
            ValidationContext parentContext = null;

            if (this.EntitySet != null && this.EntitySet.EntityContainer != null)
            {
                parentContext = this.EntitySet.EntityContainer.ValidationContext;
            }

            return ValidationUtilities.CreateValidationContext(this, parentContext);
        }

        /// <summary>
        /// Method called when the invoked action state changes for this entity. 
        /// </summary>
        /// <remarks>
        /// This method is called when the state of CanInvoke changes.
        /// </remarks>
        [Obsolete("OnActionStateChanged is no longer used, make sure to update your version of OpenRiaServices Code Generation")]
        protected virtual void OnActionStateChanged()
        {
            // no op if not overriden
        }

        /// <summary>
        /// Called within the context of an <see cref="OnActionStateChanged"/> override, this
        /// method will raise the appropriate property changed notifications for the properties
        /// corresponding to a custom method.
        /// </summary>
        /// <param name="name">The custom method name.</param>
        /// <param name="canInvokePropertyName">The name of the "CanInvoke" guard property for the
        /// custom method.</param>
        /// <param name="isInvokedPropertyName">The name of the "IsInvoked" property for the
        /// custom method.</param>
        [Obsolete("UpdateActionState is no longer used, make sure to update your version of OpenRiaServices Code Generation")]
        protected void UpdateActionState(string name, string canInvokePropertyName, string isInvokedPropertyName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(canInvokePropertyName))
            {
                throw new ArgumentNullException("canInvokePropertyName");
            }
            if (string.IsNullOrEmpty(isInvokedPropertyName))
            {
                throw new ArgumentNullException("isInvokedPropertyName");
            }

            var metaType = MetaType;
            if (!metaType.IsLegacyEntityActionsDiscovered)
            {
                metaType.TryAddLegacyEntityAction(name, canInvokePropertyName, isInvokedPropertyName);
            }
            else
            {
                // For the current implementation, we always raise the CanInvoke change notification
                this.RaisePropertyChanged(canInvokePropertyName);
            }
        }

        /// <summary>
        /// Begin editing this entity
        /// </summary>
        protected void BeginEdit()
        {
            if (!this.IsEditing)
            {
                this._editSession = EditSession.Begin(this);
            }
        }

        /// <summary>
        /// Cancel any edits made to this entity since the last call
        /// to BeginEdit
        /// </summary>
        protected void CancelEdit()
        {
            if (!this.IsEditing)
            {
                return;
            }

            // Cancel and close out the editing session, after capturing the set of modified properties
            // for notifications below.
            string[] modifiedProperties = this._editSession.ModifiedProperties.ToArray();
            this._editSession.Cancel();
            this._editSession = null;

            this.UpdateRelatedAssociations(modifiedProperties);
        }

        /// <summary>
        /// Commit the edits made to this entity since the last call
        /// to BeginEdit
        /// </summary>
        protected void EndEdit()
        {
            if (!this.IsEditing)
            {
                return;
            }

#if SILVERLIGHT
            // Validate the entity itself (cross-field validation happens here)
            List<ValidationResult> validationResults = new List<ValidationResult>();
            ValidationUtilities.TryValidateObject(this, this.CreateValidationContext(), validationResults);

            // Replace all errors for this entity
            ValidationUtilities.ApplyValidationErrors(this, validationResults);
#else
            // Validate the entity itself (cross-field validation happens here)
            // TODO : The desktop version of the framework doesn't currently do
            // deep validation.
            Validator.ValidateObject(this, this.CreateValidationContext(), /*validateAllProperties*/ true);
#endif
            // Close out the editing session, after capturing the set of modified properties
            // for notifications below.
            string[] modifiedProperties = this._editSession.ModifiedProperties.ToArray();
            this._editSession = null;

            this.UpdateRelatedAssociations(modifiedProperties);
        }

        /// <summary>
        /// Notify our EntitySet of the specified property notifications so related
        /// associations can be updated. We only want to do this if we're currently
        /// attached.
        /// <remarks>
        /// </remarks>
        /// When in an edit session, association updates are postponed until either
        /// EndEdit or CancelEdit are called. At that point all updates will be
        /// processed by this method.
        /// </summary>
        /// <param name="modifiedProperties">The collection of modified properties.</param>
        private void UpdateRelatedAssociations(IEnumerable<string> modifiedProperties)
        {
            if (this.EntitySet != null)
            {
                foreach (string modifiedMember in modifiedProperties)
                {
                    this.EntitySet.UpdateRelatedAssociations(this, modifiedMember);
                }
            }
        }

        /// <summary>
        /// Called to register an action to be invoked for this entity during SubmitChanges.
        /// <see cref="InvalidOperationException"/> is thrown if the custom method invocation
        /// violates any framework constraints. <see cref="ValidationException"/> is thrown if
        /// any validation on this object, the custom method or the specified parameters fails.
        /// </summary>
        /// <param name="actionName">The name of the action to invoke</param>
        /// <param name="parameters">The parameters values to invoke the specified action with</param>
        protected internal void InvokeAction(string actionName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Parameter_NullOrEmpty, "actionName"));
            }

            // verify that the action can currently be invoked
            if (this.IsSubmitting)
            {
                throw new InvalidOperationException(Resource.Entity_InvokeWhileSubmitting);
            }

            // custom methods cannot be called on deleted entities
            if (this._entityState == EntityState.Deleted)
            {
                throw new InvalidOperationException(Resource.Entity_InvokeOnDeletedEntity);
            }

            // custom methods cannot be called on detached entities
            if (this.EntityState == EntityState.Detached)
            {
                throw new InvalidOperationException(Resource.Entity_InvokeOnDetachedEntity);
            }

            // Validate the entity itself (cross-field validation happens here)
            Validator.ValidateObject(this, this.CreateValidationContext(), /*validateAllProperties*/ true);

            // call validation helper to loop through the validation attributes
            // on the method itself as well as all the parameters
            ValidationUtilities.ValidateCustomUpdateMethodCall(actionName, this.CreateValidationContext(), parameters);

            var customMethodInfo = MetaType.GetEntityAction(actionName);
            if (customMethodInfo == null)
                throw new InvalidOperationException(string.Format(Resource.Entity_NoEntityActionWithName, actionName));

            // record invocation on the entity, which does proper state transition and raising property changed events
            InvokeActionCore(new EntityAction(actionName, parameters), customMethodInfo);
        }

        internal void InvokeActionCore(EntityAction action, EntityActionAttribute customMethodInfo)
        {
            bool wasReadOnly = this.IsReadOnly;
            bool wasInvoked = this.IsActionInvoked(action.Name);

            if (wasInvoked && customMethodInfo.AllowMultipleInvocations == false)
                throw new InvalidOperationException(Resources.MethodCanOnlyBeInvokedOnce);

            if (_customMethodInvocations == null)
                _customMethodInvocations = new List<EntityAction>();

            this._customMethodInvocations.Add(action);

            if (this._entityState == EntityState.Unmodified)
            {
                // invoking on an unmodified entity makes it modified, but invoking
                // on an entity in any other state should not change state
                this.EntityState = EntityState.Modified;
            }

            if (wasReadOnly != this.IsReadOnly)
            {
                this.RaisePropertyChanged("IsReadOnly");
            }

            // Perform any action state updates required

            this.RaisePropertyChanged(customMethodInfo.CanInvokePropertyName);

            if (!wasInvoked)
                this.RaisePropertyChanged(customMethodInfo.IsInvokedPropertyName);
        }

        /// <summary>
        /// Undoes a previously invoked action.
        /// </summary>
        /// <param name="action">The action to undo.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        /// <exception cref="System.InvalidOperationException">A custom method cannot be undone on an entity that is part of a change-set that is in the process of being submitted</exception>
        /// <exception cref="System.ArgumentException">If the action does not belong to this Entity's<see cref="EntityActions"/> </exception>
        internal protected void UndoAction(EntityAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            // verify that the action can currently be invoked
            if (this.IsSubmitting)
                throw new InvalidOperationException(Resource.Entity_UndoInvokeWhileSubmitting);

            var removed = _customMethodInvocations != null && this._customMethodInvocations.Remove(action);
            if (!removed)
                throw new ArgumentException(Resource.Entity_UndoInvokeOnlyForInvokedActions);

            var customUpdate = MetaType.GetEntityAction(action.Name);
            Debug.Assert(customUpdate != null, "EntityAction have valid name since it is part of EntityActions");
            RaisePropertyChanged(customUpdate.CanInvokePropertyName);

            // If no additional invocations are recorded, then raise an update
            if (!_customMethodInvocations.Any(item => item.Name == action.Name))
                RaisePropertyChanged(customUpdate.IsInvokedPropertyName);
        }

        /// <summary>
        /// Gets a value indicating whether the specified action can currently be invoked. 
        /// </summary>
        /// <param name="name">The name of the action corresponding to a custom method</param>
        /// <returns>True if the action can currently be invoked</returns>
        protected internal bool CanInvokeAction(string name)
        {
            // custom methods cannot be invoked if the entity is deleted, or if the entity is unattached.
            bool canInvoke = this.EntityState != EntityState.Deleted 
                             && this.EntitySet != null
                             && !this.IsSubmitting;

            if (canInvoke)
            {
                // return false if method name is invalid 
                //  or the action is already invoked and multiple invocations are not allowd
                var customMethod = MetaType.GetEntityAction(name);
                if (customMethod == null 
                        || (customMethod.AllowMultipleInvocations == false && IsActionInvoked(name)))
                    canInvoke = false;
            }

            return canInvoke;
        }

        /// <summary>
        /// Gets the list of custom method invocations pending for this entity.
        /// </summary>
        /// <remarks>
        /// Currently only a single pending invocation is supported for an entity.
        /// </remarks>
        /// <returns>Collection of custom method invocations pending for this entity</returns>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(AutoGenerateField = false)]
        public IEnumerable<EntityAction> EntityActions
        {
            get
            {
                if (this._customMethodInvocations != null)
                {
                    return new ReadOnlyCollection<EntityAction>(this._customMethodInvocations);
                }
                else
                {
                    return Enumerable.Empty<EntityAction>();
                }
            }
        }

        /// <summary>
        /// Return the entity identity, suitable for hashing. If the entity
        /// does not yet have an identity, null will be returned.
        /// </summary>
        /// <returns>The identity for this entity</returns>
        public virtual object GetIdentity()
        {
            // for identity purposes, we need to make sure values are always ordered
            PropertyInfo[] keyMembers = this.MetaType.KeyMembers.OrderBy(p => p.Name).ToArray();

            // build the key value array, returning null immediately if any
            // values are null
            object[] keyValues = new object[keyMembers.Length];
            for (int i = 0; i < keyMembers.Length; i++)
            {
                object keyValue = keyMembers[i].GetValue(this, null);
                if (keyValue == null)
                {
                    return null;
                }
                keyValues[i] = keyValue;
            }

            if (keyValues.Length == 1)
            {
                return keyValues[0];
            }
            else
            {
                return EntityKey.Create(keyValues);
            }
        }

        /// <summary>
        /// Returns a string representation of the current entity.
        /// </summary>
        /// <returns>A string representation of the current entity</returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            PropertyInfo[] keyMembers = this.MetaType.KeyMembers.ToArray();
            foreach (PropertyInfo keyMember in keyMembers.OrderBy(m => m.Name))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                object keyValue = keyMember.GetValue(this, null);
                sb.Append(keyValue != null ? keyValue.ToString() : "null");
            }
            string keyText = keyMembers.Length > 1 ? "{" + sb.ToString() + "}" : sb.ToString();
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0} : {1}", GetType().Name, keyText);
        }

        /// <summary>
        /// Register a callback that will be called any time this entity is
        /// added or removed from an entity set
        /// </summary>
        /// <param name="callback">The callback to register.</param>
        internal void RegisterSetChangedCallback(Action callback)
        {
            this._setChangedCallback = (Action)Delegate.Combine(this._setChangedCallback, callback);
        }

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        /// Event raised whenever an <see cref="Entity"/> property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                this._propChangedHandler = (PropertyChangedEventHandler)Delegate.Combine(this._propChangedHandler, value);
            }
            remove
            {
                this._propChangedHandler = (PropertyChangedEventHandler)Delegate.Remove(this._propChangedHandler, value);
            }
        }
        #endregion

        #region IEditableObject Members

        /// <summary>
        /// Begin editing this entity
        /// </summary>
        void IEditableObject.BeginEdit()
        {
            this.BeginEdit();
        }

        /// <summary>
        /// Cancel the edits made to this entity since the last call
        /// to BeginEdit
        /// </summary>
        void IEditableObject.CancelEdit()
        {
            this.CancelEdit();
        }

        /// <summary>
        /// Commit the edits made to this entity since the last call
        /// to BeginEdit
        /// </summary>
        void IEditableObject.EndEdit()
        {
            this.EndEdit();
        }

        #endregion

        #region IRevertibleChangeTracking Members

        /// <summary>
        /// Revert all property changes made to this entity back to their original values. This method
        /// does not revert <see cref="EntitySet"/> Add/Remove operations, so if this <see cref="Entity"/>
        /// is New or Deleted, this method does nothing. This method also reverts any pending custom
        /// method invocations on the entity. If this <see cref="Entity"/> has compositional associations,
        /// any changes made to those associations or the child entities themselves will be reverted.
        /// </summary>
        void IRevertibleChangeTracking.RejectChanges()
        {
            this.RejectChanges();
        }

        #endregion

        #region IChangeTracking Members

        /// <summary>
        /// Accept all changes made to this <see cref="Entity"/>. If this <see cref="Entity"/>
        /// has compositional associations, any changes made to those associations or the child
        /// entities themselves will be accepted. 
        /// </summary>
        void IChangeTracking.AcceptChanges()
        {
            this.AcceptChanges();
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Entity"/> currently has any pending changes.
        /// If this <see cref="Entity"/> has compositional associations and any children have changes
        /// <c>true</c> will be returned.
        /// </summary>
        bool IChangeTracking.IsChanged
        {
            get
            {
                return this.HasChanges;
            }
        }

        #endregion

        /// <summary>
        /// Method called after this entity has been deserialized
        /// </summary>
        /// <param name="context">The serialization context</param>
        [OnDeserialized]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnDeserialized(StreamingContext context)
        {
            this._isDeserializing = false;
        }

        /// <summary>
        /// Method called when this entity is being deserialized
        /// </summary>
        /// <param name="context">The serialization context</param>
        [OnDeserializing]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnDeserializing(StreamingContext context)
        {
            this._isDeserializing = true;
        }

        /// <summary>
        /// Recursively verifies that this instance nor any of its child instances are
        /// currently in an edit session. This check differs from IsEditing in that it
        /// is recursive through child complex objects.
        /// </summary>
        internal void VerifyNotEditing()
        {
            if (this.IsEditing)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.Entity_UncommittedChanges, this));
            }

            foreach (ComplexObject complexObject in this.TrackedInstances.Values)
            {
                complexObject.VerifyNotEditing();
            }
        }

        #region Nested Types
        /// <summary>
        /// Class used to manage entity edit sessions.
        /// </summary>
        private class EditSession
        {
            private Entity _entity;
            private EntityState _lastState;
            private IDictionary<string, object> _snapshot;
            private EntityAction[] _customMethodInvocations;
            private ValidationResult[] _validationErrors;
            private List<string> _modifiedProperties;

            private EditSession(Entity entity)
            {
                this._entity = entity;
                this._lastState = entity.EntityState;
                this._customMethodInvocations = entity._customMethodInvocations != null ? entity._customMethodInvocations.ToArray() : null;
                this._validationErrors = entity.ValidationErrors.ToArray();
                this._modifiedProperties = new List<string>();
            }

            /// <summary>
            /// Begins an edit session for the specified entity.
            /// </summary>
            /// <param name="entity">The entity to begin editing on.</param>
            /// <returns>The edit session.</returns>
            public static EditSession Begin(Entity entity)
            {
                return new EditSession(entity);
            }

            /// <summary>
            /// Cancels the edit session, reverting all changes made to the
            /// entity since the session began.
            /// </summary>
            public void Cancel()
            {
                if (this._lastState == EntityState.Unmodified)
                {
                    // the entity was Unmodified before the edit session began
                    // so we need to revert back to Unmodified 
                    this._entity.RejectChanges();
                }
                else
                {
                    // revert any data member modifications
                    if (this._snapshot != null)
                    {
                        this._entity.ApplyState(this._snapshot);
                    }

                    // revert any added custom method invocation
                    if (this._entity._customMethodInvocations != null)
                    {
                        IEnumerable<EntityAction> addedInvokations = this._entity._customMethodInvocations;

                        if (this._customMethodInvocations != null)
                            addedInvokations = addedInvokations.Where(item => !this._customMethodInvocations.Contains(item));

                        foreach (var method in addedInvokations.ToList())
                            this._entity.UndoAction(method);
                    }

                    // add any "uninvoked" methods
                    if (this._customMethodInvocations != null)
                    {
                        IEnumerable<EntityAction> removedInvocations = this._customMethodInvocations;

                        if (this._entity._customMethodInvocations != null)
                            removedInvocations = removedInvocations.Where(item => !this._entity._customMethodInvocations.Contains(item));

                        foreach (var method in removedInvocations.ToArray())
                        {
                            this._entity.InvokeActionCore(method, _entity.MetaType.GetEntityAction(method.Name));
                        }
                    }
                }

                // revert the validation errors
                ValidationUtilities.ApplyValidationErrors(this._entity, this._validationErrors);
            }

            /// <summary>
            /// Called whenever a data member on the entity is modified, to allow
            /// the session to take a state snapshot.
            /// </summary>
            /// <param name="memberName">The name of the member that was updated.</param>
            public void OnDataMemberUpdate(string memberName)
            {
                if (this._snapshot == null)
                {
                    // we're currently in an Edit session, so we need to take a snapshot
                    this._snapshot = this._entity.ExtractState();
                }

                // keep track of members that were modified
                if (!this._modifiedProperties.Contains(memberName))
                {
                    this._modifiedProperties.Add(memberName);
                }
            }

            /// <summary>
            /// Returns the set properties modified in this edit session.
            /// </summary>
            public IEnumerable<string> ModifiedProperties
            {
                get
                {
                    return this._modifiedProperties;
                }
            }
        }

        /// <summary>
        /// Custom result collection for Entity that knows how to handle hierarchical validation
        /// operations.
        /// </summary>
        private class EntityValidationResultCollection : ValidationResultCollection
        {
            private Entity _entity;

            public EntityValidationResultCollection(Entity entity)
                : base(entity)
            {
                this._entity = entity;
            }

            protected override void OnCollectionChanged()
            {
                this._entity.RaisePropertyChanged("ValidationErrors");
            }

            protected override void OnHasErrorsChanged()
            {
                this._entity.RaisePropertyChanged("HasValidationErrors");
            }

#if SILVERLIGHT
            protected override void OnPropertyErrorsChanged(string propertyName)
            {
                this._entity.RaiseValidationErrorsChanged(propertyName);
            }
#endif
        }
        #endregion
    }
}

namespace OpenRiaServices.DomainServices.Client.EntityExtensions
{
    public static class EntityExtensions
    {
        public static IDictionary<string, object> ExtractState(this Entity targetEntity)
        {
            return targetEntity.ExtractState();
        }
        public static void ExtractState(this Entity targetEntity, IDictionary<string, object> entityStateToApply)
        {
            targetEntity.ApplyState(entityStateToApply);
        }
        public static void ExtractState(this Entity targetEntity, IDictionary<string, object> entityStateToApply, LoadBehavior loadBehavior)
        {
            targetEntity.ApplyState(entityStateToApply, loadBehavior);
        }

        public static void UpdateOriginalValues(this Entity targetEntity, Entity entity)
        {
            targetEntity.UpdateOriginalValues(entity);
        }
        public static void UpdateOriginalValues(this Entity targetEntity, IDictionary<string, object> entityStateToApply)
        {
            targetEntity.UpdateOriginalValues(entityStateToApply);
        }
        public static void Merge(this Entity targetEntity, Entity otherEntity, LoadBehavior loadBehavior)
        {
            targetEntity.Merge(otherEntity, loadBehavior);
        }

        public static void Merge(this Entity targetEntity, IDictionary<string,object> otherState, LoadBehavior loadBehavior)
        {
            targetEntity.Merge(otherState, loadBehavior);
        }

        public static EntitySet<TEntity> GetEntitySet<TEntity>(this TEntity entity) where TEntity : Entity
        {
            return entity.EntitySet as EntitySet<TEntity>;
        }
    }
}