using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Base class for all complex objects.
    /// </summary>
    [DataContract]
    public abstract partial class ComplexObject : INotifyPropertyChanged, IEditableObject
    {
        private PropertyChangedEventHandler _propChangedHandler;
        private ComplexObjectValidationResultCollection _validationErrors;
        private Dictionary<string, ComplexObject> _trackedInstances;
        private Action _onDataMemberChanging;
        private Action _onDataMemberChanged;
        private Action<string, IEnumerable<ValidationResult>> _onMemberValidationChanged;
        private EditSession _editSession;
        private object _parent;
        private string _parentPropertyName;
        private bool _isDeserializing;
        private MetaType _metaType;

        /// <summary>
        /// Gets the map of child ComplexObject instances currently being
        /// tracked by this instance.
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
        /// Gets the parent entity if this complex type instance is hosted
        /// by an entity. May return null.
        /// </summary>
        private Entity Entity
        {
            get
            {
                object currParent = this._parent;
                while (currParent != null)
                {
                    Entity entity = currParent as Entity;
                    if (entity != null)
                    {
                        return entity;
                    }
                    else
                    {
                        currParent = ((ComplexObject)currParent)._parent;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the MetaType for this complex type.
        /// </summary>
        internal MetaType MetaType
        {
            get
            {
                if (this._metaType == null)
                {
                    this._metaType = MetaType.GetMetaType(this.GetType());
                }

                return this._metaType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this complex type instance is currently
        /// attached to a parent.
        /// </summary>
        internal bool IsAttached
        {
            get
            {
                return this._parent != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently being deserialized.
        /// </summary>
        protected internal bool IsDeserializing
        {
            get
            {
                return this._isDeserializing;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any validation errors.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public bool HasValidationErrors
        {
            get
            {
                return this.ValidationErrors.Any();
            }
        }

        /// <summary>
        /// Gets the collection of validation errors for this instance. 
        /// </summary>
        [Display(AutoGenerateField = false)]
        public ICollection<ValidationResult> ValidationErrors
        {
            get
            {
                if (this._validationErrors == null)
                {
                    this._validationErrors = new ComplexObjectValidationResultCollection(this);
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
        /// Attach this complex type instance to the specified parent.
        /// </summary>
        /// <param name="parent">The parent of the complex type.</param>
        /// <param name="propertyName">The containing property name.</param>
        /// <param name="onDataMemberChanging">The callback to call when a data member is changing on this complex type instance.</param>
        /// <param name="onDataMemberChanged">The callback to call when a data member has changed on this complex type instance.</param>
        /// <param name="onMemberValidationChanged">The callback to call when validation has changed for a property.</param>
        internal void Attach(object parent, string propertyName, Action onDataMemberChanging, Action onDataMemberChanged, Action<string, IEnumerable<ValidationResult>> onMemberValidationChanged)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
            if (onDataMemberChanging == null)
            {
                throw new ArgumentNullException("onDataMemberChanging");
            }
            if (onDataMemberChanged == null)
            {
                throw new ArgumentNullException("onDataMemberChanged");
            }
            if (onMemberValidationChanged == null)
            {
                throw new ArgumentNullException("onMemberValidationChanged");
            }
            Debug.Assert(typeof(Entity).IsAssignableFrom(parent.GetType()) || typeof(ComplexObject).IsAssignableFrom(parent.GetType()), "Parent must be an Entity or ComplexObject.");

            this.CheckForCycles(parent);

            this._parent = parent;
            this._parentPropertyName = propertyName;
            this._onDataMemberChanging = onDataMemberChanging;
            this._onDataMemberChanged = onDataMemberChanged;
            this._onMemberValidationChanged = onMemberValidationChanged;
        }

        /// <summary>
        /// Detach this complex type instance from its parent.
        /// </summary>
        internal void Detach()
        {
            this._parent = null;
            this._parentPropertyName = null;
            this._onDataMemberChanging = null;
            this._onDataMemberChanged = null;
            this._onMemberValidationChanged = null;
        }

        /// <summary>
        /// Event raised whenever a <see cref="ComplexObject"/> property has changed.
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this._propChangedHandler += value; }
            remove { this._propChangedHandler -= value; }
        }

        /// <summary>
        /// Called from a property setter to notify the framework that a
        /// <see cref="ComplexObject"/> property has changed. This method does not
        /// perform any change tracking operations.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called when a <see cref="ComplexObject"/> property has changed.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (this._propChangedHandler != null)
            {
                this._propChangedHandler(this, e);
            }
        }

        /// <summary>
        /// Called from a property setter to notify the framework that a
        /// <see cref="ComplexObject"/> data member has changed. This method performs 
        /// any required change tracking and state transitions.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void RaiseDataMemberChanged(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            // Note, RaiseDataMemberChanged is called on every property update. We need to avoid as
            // much overhead as possible.
            if (this.MetaType.HasComplexMembers)
            {
                // if the property is a complex type, we need to attach to the
                // instance
                MetaMember metaMember = this.MetaType[propertyName];
                if (metaMember != null && metaMember.IsComplex && !metaMember.IsCollection)
                {
                    this.AttachComplexObjectInstance(metaMember);
                }
            }

            this.OnDataMemberChanged();

            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
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

        /// <summary>
        /// Called from a property setter to notify the framework that a
        /// <see cref="ComplexObject"/> data member is about to be changed. This
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
        }

        /// <summary>
        /// This method relays a property changed notification up the containment
        /// hierarchy.
        /// </summary>
        private void OnDataMemberChanged()
        {
            if (this.IsAttached)
            {
                this._onDataMemberChanged();
            }
        }

        /// <summary>
        /// This method relays a property changing notification up the containment
        /// hierarchy.
        /// </summary>
        private void OnDataMemberChanging()
        {
            if (this.IsEditing)
            {
                this._editSession.OnDataMemberUpdate();
            }

            if (this.IsAttached)
            {
                this._onDataMemberChanging();
            }
        }

        /// <summary>
        /// This method performs all required operations when validation has just been performed for
        /// the specified property.
        /// </summary>
        /// <param name="propertyName">The property that was validated.</param>
        /// <param name="validationResults">The validation results.</param>
        private void OnMemberValidationChanged(string propertyName, IEnumerable<ValidationResult> validationResults)
        {
            // first apply the results to our own collection
            this.ValidationResultCollection.ReplaceErrors(propertyName, validationResults);

            // notify our parent
            this.NotifyParentMemberValidationChanged(propertyName, validationResults);
        }

        private void NotifyParentMemberValidationChanged(string propertyName, IEnumerable<ValidationResult> validationResults)
        {
            if (this.IsAttached)
            {
                // notify our parent of the errors, appending the property path to the results
                IEnumerable<ValidationResult> parentValidationResults = ValidationUtilities.ApplyMemberPath(validationResults, this._parentPropertyName);
                string propertyPath = this._parentPropertyName;
                if (!string.IsNullOrEmpty(propertyName))
                {
                    propertyPath = propertyPath + "." + propertyName;
                }
                this._onMemberValidationChanged(propertyPath, parentValidationResults);
            }
        }

#if SILVERLIGHT
        /// <summary>
        /// Validate whether the specified value is valid for the specified property
        /// of the current ComplexObject.
        /// </summary>
        /// <remarks>
        /// This method evaluates all the <see cref="ValidationAttribute"/>s associated with the specified property, accumulating
        /// the validation errors and surfacing them through the <see cref="ValidationErrors"/> property.  It also verifies
        /// the property is not read-only.
        /// <para>All validation logic is bypassed if this instance is currently being deserialized.</para>
        /// </remarks>
        /// <param name="propertyName">The name of the property to validate.  This name cannot be <c>null</c> or empty.</param>
        /// <param name="value">The value to test. It may be <c>null</c> if <c>null</c> is valid for the given property.</param>
        /// <exception cref="ArgumentNullException"> is thrown if <paramref name="propertyName"/> is <c>null</c> or empty.</exception>
        /// <exception cref="InvalidOperationException"> is thrown if this property is marked with <see cref="EditableAttribute"/> 
        /// configured to prevent editing.</exception>
#else
        /// <summary>
        /// Validate whether the specified value is valid for the specified property
        /// of the current ComplexObject.
        /// </summary>
        /// <remarks>
        /// This method evaluates all the <see cref="ValidationAttribute"/>s associated with the specified property
        /// and throws a <see cref="ValidationException"/> for the first <see cref="ValidationAttribute"/> that signals
        /// a validation error.  It also verifies the property is not read-only.
        /// <para>All validation logic is bypassed if this instance is currently being deserialized.</para>
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

            // if this instance is currently being deserialized, or if it is hosted by an
            // entity that is being deserialized or is having state applied to it, skip
            // validation
            Entity rootEntity = this.Entity;
            if (this.IsDeserializing || (rootEntity != null && (rootEntity.IsDeserializing || rootEntity.IsApplyingState)))
            {
                return;
            }

            // if we're currently attached and the entity is not New,
            // we need to throw if Edit is not supported
            if (rootEntity != null)
            {
                if (rootEntity.EntitySet != null && rootEntity.EntityState != EntityState.New)
                {
                    rootEntity.EntitySet.EnsureEditable(EntitySetOperations.Edit);
                }

                if (rootEntity.IsReadOnly)
                {
                    throw new InvalidOperationException(Resource.Entity_ReadOnly);
                }
            }

            MetaMember metaMember = this.MetaType[propertyName];
            if (metaMember == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resource.Property_Does_Not_Exist, this.MetaType.Type.Name, propertyName), "propertyName");
            }

            if (rootEntity != null && metaMember.EditableAttribute != null && !metaMember.EditableAttribute.AllowEdit &&
                !(metaMember.EditableAttribute.AllowInitialValue && (rootEntity.EntityState == EntityState.Detached || rootEntity.EntityState == EntityState.New)))
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

            // Process the validation the errors for this property
            this.OnMemberValidationChanged(validationContext.MemberName, validationResults);
#else
            Validator.ValidateProperty(value, validationContext);
            this.OnMemberValidationChanged(validationContext.MemberName, Enumerable.Empty<ValidationResult>());
#endif
        }

        /// <summary>
        /// Get the <see cref="ValidationContext"/> to be used for validation invoked
        /// from this <see cref="ComplexObject"/>.
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
            Entity rootEntity = this.Entity;
            if (rootEntity != null && rootEntity.EntitySet != null && rootEntity.EntitySet.EntityContainer != null)
            {
                parentContext = rootEntity.EntitySet.EntityContainer.ValidationContext;
            }

            return ValidationUtilities.CreateValidationContext(this, parentContext);
        }

        /// <summary>
        /// Method called after this ComplexObject has been deserialized
        /// </summary>
        /// <param name="context">The serialization context</param>
        [OnDeserialized]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnDeserialized(StreamingContext context)
        {
            this._isDeserializing = false;
        }

        /// <summary>
        /// Method called when this ComplexObject is being deserialized
        /// </summary>
        /// <param name="context">The serialization context</param>
        [OnDeserializing]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnDeserializing(StreamingContext context)
        {
            this._isDeserializing = true;
        }

        /// <summary>
        /// This method will throw if setting the parent for the current instance
        /// would result in a cyclic reference.
        /// </summary>
        /// <param name="candidateParent">The candidate parent</param>
        private void CheckForCycles(object candidateParent)
        {
            // Walk up the containment chain searching for this instance.
            // If found, setting the parent of this instance to the candidate
            // would result in a cycle.
            object currParent = candidateParent;
            while (currParent != null)
            {
                if (currParent == this)
                {
                    throw new InvalidOperationException(Resource.CyclicReferenceError);
                }

                ComplexObject complexParent = currParent as ComplexObject;
                if (complexParent != null)
                {
                    currParent = complexParent._parent;
                }
                else
                {
                    // we can stop once we reach a null or
                    // a parent entity
                    break;
                }
            }
        }

        /// <summary>
        /// Recursively verifies that this instance nor any of its child instances are
        /// currently in an edit session.
        /// </summary>
        internal void VerifyNotEditing()
        {
            if (this.IsEditing)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.Entity_UncommittedChanges, this.Entity));
            }

            foreach (ComplexObject complexObject in this.TrackedInstances.Values)
            {
                complexObject.VerifyNotEditing();
            }
        }

        #region IEditableObject Members

        /// <summary>
        /// Gets a value indicating whether there is currently an uncommitted
        /// edit session in progress for this instance. This is the case when
        /// BeginEdit has been called, but EndEdit/CancelEdit have not.
        /// </summary>
        internal bool IsEditing
        {
            get
            {
                return this._editSession != null;
            }
        }

        protected internal bool IsMergingState { get; set; }

        /// <summary>
        /// Begin editing this instance
        /// </summary>
        void IEditableObject.BeginEdit()
        {
            this.BeginEdit();
        }

        /// <summary>
        /// Cancel the edits made to this instance since the last call
        /// to BeginEdit
        /// </summary>
        void IEditableObject.CancelEdit()
        {
            this.CancelEdit();
        }

        /// <summary>
        /// Commit the edits made to this instance since the last call
        /// to BeginEdit
        /// </summary>
        void IEditableObject.EndEdit()
        {
            this.EndEdit();
        }

        /// <summary>
        /// Begin editing this instance
        /// </summary>
        protected void BeginEdit()
        {
            if (!this.IsEditing)
            {
                this._editSession = EditSession.Begin(this);
            }
        }

        /// <summary>
        /// Cancel any edits made to this instance since the last call
        /// to BeginEdit
        /// </summary>
        protected void CancelEdit()
        {
            if (!this.IsEditing)
            {
                return;
            }

            this._editSession.Cancel();
            this._editSession = null;
        }

        /// <summary>
        /// Commit the edits made to this instance since the last call
        /// to BeginEdit
        /// </summary>
        protected void EndEdit()
        {
            if (!this.IsEditing)
            {
                return;
            }

#if SILVERLIGHT
            // Validate the instance itself (cross-field validation happens here)
            List<ValidationResult> validationResults = new List<ValidationResult>();
            ValidationUtilities.TryValidateObject(this, this.CreateValidationContext(), validationResults);

            // Replace all errors for this instance and notify our parent
            ValidationUtilities.ApplyValidationErrors(this, validationResults);
            this.NotifyParentMemberValidationChanged(null, validationResults);
#else
            // Validate the instance itself (cross-field validation happens here)
            // TODO : The desktop version of the framework doesn't currently do
            // deep validation.
            Validator.ValidateObject(this, this.CreateValidationContext(), /*validateAllProperties*/ true);
#endif
            this._editSession = null;
        }
        #endregion

        #region Nested Types
        /// <summary>
        /// Class used to manage edit sessions.
        /// </summary>
        private class EditSession
        {
            private ComplexObject _instance;
            private IDictionary<string, object> _snapshot;
            private ValidationResult[] _validationErrors;

            private EditSession(ComplexObject instance)
            {
                this._instance = instance;
                this._validationErrors = instance.ValidationErrors.ToArray();
            }

            /// <summary>
            /// Begins an edit session for the specified instance.
            /// </summary>
            /// <param name="instance">The instance to begin editing on.</param>
            /// <returns>The edit session.</returns>
            public static EditSession Begin(ComplexObject instance)
            {
                return new EditSession(instance);
            }

            /// <summary>
            /// Cancels the edit session, reverting all changes made to the
            /// entity since the session began.
            /// </summary>
            public void Cancel()
            {
                // revert any data member modifications
                if (this._snapshot != null)
                {
                    ObjectStateUtility.ApplyState(this._instance, this._snapshot);
                }

                // Revert the validation errors and notify our parent
                ValidationUtilities.ApplyValidationErrors(this._instance, this._validationErrors);
                this._instance.NotifyParentMemberValidationChanged(null, this._validationErrors);
            }

            /// <summary>
            /// Called whenever a data member on the instance is modified, to allow
            /// the session to take a state snapshot.
            /// </summary>
            public void OnDataMemberUpdate()
            {
                if (this._snapshot == null)
                {
                    // we're currently in an Edit session, so we need to take a snapshot
                    this._snapshot = ObjectStateUtility.ExtractState(this._instance);
                }
            }
        }

        /// <summary>
        /// Custom result collection for ComplexObjects that knows how to handle hierarchical validation
        /// operations.
        /// </summary>
        private class ComplexObjectValidationResultCollection : ValidationResultCollection
        {
            private ComplexObject _complexObject;

            public ComplexObjectValidationResultCollection(ComplexObject complexObject)
                : base(complexObject)
            {
                this._complexObject = complexObject;
            }

            /// <summary>
            /// When a result is manually added to the collection, we must add it
            /// to our parents collection.
            /// </summary>
            /// <param name="item">The result to add</param>
            protected override void OnAdd(ValidationResult item)
            {
                if (this._complexObject._parent != null)
                {
                    // transform the error by adding our parent property to the member names
                    item = ValidationUtilities.ApplyMemberPath(item, this._complexObject._parentPropertyName);

                    // add the result to our parents error collection
                    ICollection<ValidationResult> resultCollection = GetValidationResults(this._complexObject._parent);
                    resultCollection.Add(item);
                }
            }

            /// <summary>
            /// When a result is manually removed from the collection, we must remove 
            /// it from our parents collection.
            /// </summary>
            /// <param name="item">The result to remove</param>
            protected override void OnRemove(ValidationResult item)
            {
                if (this._complexObject._parent != null)
                {
                    // transform the error by adding our parent property to the member names
                    item = ValidationUtilities.ApplyMemberPath(item, this._complexObject._parentPropertyName);

                    // search (by value) for the item in our parents error collection
                    ICollection<ValidationResult> resultCollection = GetValidationResults(this._complexObject._parent);
                    ValidationResultEqualityComparer comparer = new ValidationResultEqualityComparer();
                    item = resultCollection.FirstOrDefault(p => comparer.Equals(p, item));

                    if (item != null)
                    {
                        resultCollection.Remove(item);
                    }
                }
            }

            private static ICollection<ValidationResult> GetValidationResults(object parent)
            {
                ComplexObject complexParent = parent as ComplexObject;
                if (complexParent != null)
                {
                    return complexParent.ValidationErrors;
                }
                else
                {
                    return ((Entity)parent).ValidationErrors;
                }
            }

            /// <summary>
            /// When the collection is cleared, we must call base and notify
            /// our parent that our results have been cleared.
            /// </summary>
            protected override void OnClear()
            {
                base.OnClear();

                this._complexObject.NotifyParentMemberValidationChanged(null, Enumerable.Empty<ValidationResult>());
            }

            protected override void OnCollectionChanged()
            {
                this._complexObject.RaisePropertyChanged("ValidationErrors");
            }

            protected override void OnHasErrorsChanged()
            {
                this._complexObject.RaisePropertyChanged("HasValidationErrors");
            }

#if SILVERLIGHT
            protected override void OnPropertyErrorsChanged(string propertyName)
            {
                this._complexObject.RaiseValidationErrorsChanged(propertyName);
            }
#endif
        }
        #endregion
    }
#if SILVERLIGHT
    public abstract partial class ComplexObject : INotifyDataErrorInfo
    {
        private EventHandler<DataErrorsChangedEventArgs> _errorsChangedHandler;
        /// <summary>
        /// Raises the event whenever validation errors have changed for a property.
        /// </summary>
        /// <param name="propertyName">The property whose errors have changed.</param>
        private void RaiseValidationErrorsChanged(string propertyName)
        {
            if (this._errorsChangedHandler != null)
            {
                this._errorsChangedHandler(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// Explicitly implement the <see cref="INotifyDataErrorInfo.ErrorsChanged"/> event.
        /// </summary>
        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add { this._errorsChangedHandler += value; }
            remove { this._errorsChangedHandler -= value; }
        }
        /// <summary>
        /// Get the errors for the specified property, or the type-level
        /// errors if <paramref name="propertyName"/> is <c>null</c> of empty.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to get errors for.  When <c>null</c> or empty,
        /// errors that apply to at the entity level will be returned.
        /// </param>
        /// <returns>
        /// The list of errors for the specified <paramref name="propertyName"/>,
        /// or type-level errors when <paramref name="propertyName"/> is
        /// <c>null</c> or empty.
        /// </returns>
        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            IEnumerable<ValidationResult> results;
            if (string.IsNullOrEmpty(propertyName))
            {
                // If the property name is null or empty, then we want to include errors
                // where the member names array is empty, or where the member names array
                // contains a null or empty string.
                results = this.ValidationResultCollection.Where(e => !e.MemberNames.Any() || e.MemberNames.Contains(propertyName));
            }
            else
            {
                // Otherwise, only return the errors that contain the property name
                results = this.ValidationResultCollection.Where(e => e.MemberNames.Contains(propertyName));
            }
            // Prevent deferred enumeration
            return results.ToArray();
        }
        /// <summary>
        /// Gets a value indicating whether or not the entity presently has errors.
        /// </summary>
        bool INotifyDataErrorInfo.HasErrors
        {
            get { return this.ValidationResultCollection.Count > 0; }
        }
    }
#endif
}
