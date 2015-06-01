using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="Entity"/> changes
    /// </summary>
    public sealed class EntityChangeSet : IEnumerable<Entity>
    {
        private ReadOnlyCollection<Entity> _addedEntities;
        private ReadOnlyCollection<Entity> _removedEntities;
        private ReadOnlyCollection<Entity> _modifiedEntities;
        private IEnumerable<ChangeSetEntry> _changeSetEntries;

        internal EntityChangeSet(
            ReadOnlyCollection<Entity> addedEntities,
            ReadOnlyCollection<Entity> modifiedEntities,
            ReadOnlyCollection<Entity> removedEntities)
        {
            if (addedEntities == null)
            {
                throw new ArgumentNullException("addedEntities");
            }
            if (modifiedEntities == null)
            {
                throw new ArgumentNullException("modifiedEntities");
            }
            if (removedEntities == null)
            {
                throw new ArgumentNullException("removedEntities");
            }

            this._addedEntities = addedEntities;
            this._removedEntities = removedEntities;
            this._modifiedEntities = modifiedEntities;
        }

        /// <summary>
        /// Gets the collection of added Entities
        /// </summary>
        public ReadOnlyCollection<Entity> AddedEntities
        {
            get
            {
                return this._addedEntities;
            }
            internal set
            {
                this._addedEntities = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this changeset has any changes
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ((this._addedEntities == null) || (this._addedEntities.Count == 0)) &&
                       ((this._removedEntities == null) || (this._removedEntities.Count == 0)) &&
                       ((this._modifiedEntities == null) || (this._modifiedEntities.Count == 0));
            }
        }

        /// <summary>
        /// Gets the collection of modified entities
        /// </summary>
        public ReadOnlyCollection<Entity> ModifiedEntities
        {
            get
            {
                return this._modifiedEntities;
            }
            internal set
            {
                this._modifiedEntities = value;
            }
        }

        /// <summary>
        /// Gets the collection of removed entities
        /// </summary>
        public ReadOnlyCollection<Entity> RemovedEntities
        {
            get
            {
                return this._removedEntities;
            }
            internal set
            {
                this._removedEntities = value;
            }
        }

        /// <summary>
        /// Returns a textual summary of this change set
        /// </summary>
        /// <returns>A textual summary of the change set</returns>
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "{{Added = {0}, Modified = {1}, Removed = {2}}}",
                this._addedEntities.Count,
                this._modifiedEntities.Count,
                this._removedEntities.Count);
        }

        /// <summary>
        /// Gets the collection of <see cref="ChangeSetEntry"/> items for this changeset.
        /// </summary>
        /// <returns>A collection of <see cref="ChangeSetEntry"/> items.</returns>
        public IEnumerable<ChangeSetEntry> GetChangeSetEntries()
        {
            if (this._changeSetEntries == null)
            {
                this._changeSetEntries = ChangeSetBuilder.Build(this);
            }
            return this._changeSetEntries;
        }

        /// <summary>
        /// Validate the changeset.
        /// </summary>
        /// <param name="validationContext">The ValidationContext to use.</param>
        /// <returns>True if the changeset is valid, false otherwise.</returns>
        internal bool Validate(ValidationContext validationContext)
        {
            bool success = true;

            foreach (Entity entity in this)
            {
                entity.VerifyNotEditing();
                bool entityRequiresValidation = entity.MetaType.RequiresValidation;
                var entityActions = entity.EntityActions;

                if (!entityRequiresValidation && !entityActions.Any())
                {
                    continue;
                }

                if (entity.EntityState == EntityState.Deleted)
                {
                    // skip validation for Deleted entities here since the entity is going to be deleted anyway
                    continue;
                }

                // first validate the entity
                List<ValidationResult> validationResults = new List<ValidationResult>();
                if (entityRequiresValidation)
                {
                    ValidationUtilities.TryValidateObject(entity, validationContext, validationResults);
                }

                // validate any Custom Method invocations
                foreach(var customMethod in entityActions)
                {
                    // validate the method call
                    object[] parameters = customMethod.HasParameters ? customMethod.Parameters.ToArray() : null;
                    ValidationContext customMethodValidationContext = ValidationUtilities.CreateValidationContext(entity, validationContext);
                    ValidationUtilities.TryValidateCustomUpdateMethodCall(customMethod.Name, customMethodValidationContext, parameters, validationResults);
                }

                if (validationResults.Count > 0)
                {
                    // replace the validation errors for the entity
                    IEnumerable<ValidationResult> entityErrors = validationResults.Select(err => new ValidationResult(err.ErrorMessage, err.MemberNames)).Distinct(new ValidationResultEqualityComparer()).ToList().AsReadOnly();
                    ValidationUtilities.ApplyValidationErrors(entity, entityErrors);
                    success = false;
                }
                else
                {
                    // clear the errors for the entity
                    entity.ValidationErrors.Clear();
                }
            }

            return success;
        }

        #region IEnumerable<Entity> Members

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            return this.AddedEntities.Concat(this.ModifiedEntities).Concat(this.RemovedEntities).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Entity>)this).GetEnumerator();
        }

        #endregion
    }
}
