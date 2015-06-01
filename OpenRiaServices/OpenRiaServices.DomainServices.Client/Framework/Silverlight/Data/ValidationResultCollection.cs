using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// An <see cref="ICollection{ValidationResult}"/> implementation that serves as a base class
    /// for custom validation error collections. Derived classes can override the several protected
    /// virtual methods to receive notifications when the collection state changes.
    /// </summary>
    internal abstract class ValidationResultCollection : ICollection<ValidationResult>
    {
        #region Member Fields

        private List<ValidationResult> _results;
        private bool _hasErrors;
        private IEnumerable<string> _propertiesInError;
        private object _parent;
        private MetaType _parentMetaType;

        #endregion Member Fields

        #region All Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent instance hosting this collection.</param>
        internal ValidationResultCollection(object parent)
        {
            this._results = new List<ValidationResult>();
            this._hasErrors = false;
            this._propertiesInError = Enumerable.Empty<string>();
            this._parent = parent;

            if (this._parent != null)
            {
                this._parentMetaType = MetaType.GetMetaType(this._parent.GetType());
            }
        }

        #endregion All Constructors

        #region Methods

        /// <summary>
        /// Perform a wholesale replacement of the errors in the collection, only raising
        /// one set of notifications.
        /// </summary>
        /// <param name="newResults">The new errors that will replace the collection.</param>
        internal void ReplaceErrors(IEnumerable<ValidationResult> newResults)
        {
            if (this.Count > 0 || newResults.Any())
            {
                // Clear without notification yet, we'll notify at the end.
                this._results.Clear();
                this._results.AddRange(newResults);

                // Force the properties of the new results to receive notifications
                this.OnCollectionChanged(GetPropertiesInError(newResults));
            }
        }

        /// <summary>
        /// Perform a wholesale replacement of the errors that affect a given property, only
        /// raising one set of notifications
        /// </summary>
        /// <param name="propertyName">The property to replace the errors for.</param>
        /// <param name="newResults">The new errors for the property.</param>
        internal void ReplaceErrors(string propertyName, IEnumerable<ValidationResult> newResults)
        {
            // First determine the set of affected member names. We have to take nested member paths
            // into account.
            List<string> affectedMembers = this.SelectMany(p => p.MemberNames).Where(p => (p != null) && p.StartsWith(propertyName + ".", StringComparison.Ordinal)).ToList();
            affectedMembers.Add(propertyName);

            // See if there are existing errors for the property
            IEnumerable<ValidationResult> existingErrors = this.Where(r => r.MemberNames.Intersect(affectedMembers).Any());

            if (existingErrors.Count() > 0 || newResults.Count() > 0)
            {
                // Capture the existing errors that are unrelated to the specified property, making sure
                // to enumerate the results before we clear our items
                IEnumerable<ValidationResult> otherErrors = this.Except(existingErrors).ToArray();

                // Replace the collection with the other errors plus the new results for this property
                // Clear without notification yet, we'll notify at the end.
                this._results.Clear();

                // Add back the union of the other errors and our new results
                this._results.AddRange(otherErrors);
                this._results.AddRange(newResults);

                // Force the properties of the new results to receive notifications, ensuring that the
                // affected members are included in that list
                this.OnCollectionChanged(GetPropertiesInError(newResults).Union(affectedMembers));
            }
        }

        /// <summary>
        /// Post-processing for when the collection has been changed.  Raise notifications as necessary.
        /// </summary>
        /// <param name="propertiesAffected">
        /// The properties directly affected by this change.  Error change notifications will always
        /// be raised for these properties.
        /// </param>
        private void OnCollectionChanged(IEnumerable<string> propertiesAffected)
        {
            bool origHasErrors = this._hasErrors;
            IEnumerable<string> origPropertiesInError = this._propertiesInError;

            // Determine our new state
            this._hasErrors = (this.Count > 0);
            this._propertiesInError = GetPropertiesInError(this).Distinct();

            // Call the notification method if the 'HasErrors' bit has changed
            if (this._hasErrors != origHasErrors)
            {
                this.OnHasErrorsChanged();
            }

            // Find what properties were: in error but aren't any longer, newly in error, affected by the change
            IEnumerable<string> noLongerInError = origPropertiesInError.Except(this._propertiesInError);
            IEnumerable<string> newlyInError = this._propertiesInError.Except(origPropertiesInError);

            // Get the combined list of properties affected.  The Union gives a Distinct result.
            IEnumerable<string> allPropertiesAffected = noLongerInError.Union(newlyInError).Union(propertiesAffected);

            // For each property affected, call the errors changed method
            foreach (string propertyName in allPropertiesAffected)
            {
                this.OnPropertyErrorsChanged(propertyName);
            }

            // Call the final collection changed notification method
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Get the list of properties currently in error, including <c>null</c> for entity-level validation errors.
        /// </summary>
        /// <returns>A list of properties in error. This list is not guaranteed to be distinct.</returns>
        /// <param name="errors">The errors to scan for the list of properties.</param>
        private static IEnumerable<string> GetPropertiesInError(IEnumerable<ValidationResult> errors)
        {
            IEnumerable<string> propertiesInError = Enumerable.Empty<string>();

            if (errors != null)
            {
                propertiesInError = errors.SelectMany(e => e.MemberNames);

                if (errors.Any(e => !e.MemberNames.Any()))
                {
                    propertiesInError = propertiesInError.Union(new string[] { null });
                }
            }

            // Be sure to enumerate the results to prevent delayed execution!
            return propertiesInError.ToArray();
        }

        /// <summary>
        /// This method is called whenever the collection has changed. Overrides do
        /// not need to call base.
        /// </summary>
        protected virtual void OnCollectionChanged()
        {
        }

        /// <summary>
        /// This method is called when the error state of the collection
        /// has changed. Overrides do not need to call base.
        /// </summary>
        protected virtual void OnHasErrorsChanged()
        {
        }

        /// <summary>
        /// This method is called when the set of validation results for the specified
        /// property has changed. Overrides do not need to call base.
        /// </summary>
        /// <param name="propertyName">The name of the property whose validation results have changed</param>
        protected virtual void OnPropertyErrorsChanged(string propertyName)
        {
        }

        #endregion Methods

        #region ICollection<ValidationResult> Members

        /// <summary>
        /// Add a <see cref="ValidationResult"/> to the collection.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(ValidationResult item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            IEnumerable<string> propertiesAffected = item.MemberNames;

            // If there are no members affected, then force the entity-level
            // change event to occur.  Otherwise, notifications will be raised
            // for the member names specified on the result, which could still
            // include null/empty members for entity-level errors.
            if (!propertiesAffected.Any())
            {
                propertiesAffected = new string[] { null };
            }

            this._results.Add(item);

            this.OnAdd(item);

            this.OnCollectionChanged(propertiesAffected);
        }

        protected virtual void OnAdd(ValidationResult item)
        {
        }

        /// <summary>
        /// Clear the items in the collection.
        /// </summary>
        public void Clear()
        {
            this.OnClear();

            if (this.Count > 0)
            {
                this._results.Clear();

                // There are no properties directly affected by this action, so use an empty enumerable
                this.OnCollectionChanged(Enumerable.Empty<string>());
            }
        }

        /// <summary>
        /// Recursively clears all validation errors in this collection and in the collections
        /// of any children.
        /// </summary>
        protected virtual void OnClear()
        {
            if (this._parent == null || !this._parentMetaType.HasComplexMembers)
            {
                return;
            }

            foreach (MetaMember metaMember in this._parentMetaType.DataMembers.Where(p => p.IsComplex))
            {
                object complexMemberValue = metaMember.GetValue(this._parent);
                if (complexMemberValue == null)
                {
                    continue;
                }

                if (!metaMember.IsCollection)
                {
                    ComplexObject complexObject = (ComplexObject)complexMemberValue;
                    complexObject.ValidationErrors.Clear();
                }
                else
                {
                    foreach (ComplexObject child in (IEnumerable)complexMemberValue)
                    {
                        child.ValidationErrors.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the current collection contains the specified <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the collection contains the item, otherwise <c>false</c>.</returns>
        public bool Contains(ValidationResult item)
        {
            return this._results.Contains(item);
        }

        /// <summary>
        /// Copy this collection to the specified <paramref name="array"/> using
        /// the specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to use for copying to the array.</param>
        public void CopyTo(ValidationResult[] array, int arrayIndex)
        {
            this._results.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the current count of validation results in the collection.
        /// </summary>
        public int Count
        {
            get { return this._results.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether or not the collection is read-only.
        /// </summary>
        /// <value>Always <c>false</c>.</value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the removal was successful, otherwise <c>false</c>.</returns>
        public bool Remove(ValidationResult item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            IEnumerable<string> propertiesAffected = item.MemberNames;

            if (this._results.Remove(item))
            {
                // If there are no members affected, then force the entity-level
                // change event to occur.  Otherwise, notifications will be raised
                // for the member names specified on the result, which could still
                // include null/empty members for entity-level errors.
                if (!propertiesAffected.Any())
                {
                    propertiesAffected = new string[] { null };
                }

                this.OnRemove(item);

                this.OnCollectionChanged(propertiesAffected);
                return true;
            }

            return false;
        }

        protected virtual void OnRemove(ValidationResult item)
        { 
        }

        #endregion

        #region IEnumerable<ValidationResult> Members

        /// <summary>
        /// Gets the enumerator for this collection of validation results.
        /// </summary>
        /// <returns>An enumerator of <see cref="ValidationResult"/> objects.</returns>
        public IEnumerator<ValidationResult> GetEnumerator()
        {
            return this._results.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the enumerator for this collection of validation results.
        /// </summary>
        /// <returns>An enumerator of <see cref="ValidationResult"/> objects.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
