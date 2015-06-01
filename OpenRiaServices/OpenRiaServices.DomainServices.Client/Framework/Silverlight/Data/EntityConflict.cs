using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents an entity conflict.
    /// </summary>
    public sealed class EntityConflict
    {
        private bool _isDeleted;
        private Entity _currentEntity;
        private Entity _originalEntity;
        private Entity _storeEntity;
        private ReadOnlyCollection<string> _propertyNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityConflict"/> class.
        /// </summary>
        /// <param name="currentEntity">The entity that the user tried to save.</param>
        /// <param name="storeEntity">The entity that represents the current state of the entity in the store.</param>
        /// <param name="propertyNames">The names of properties that are in conflict.</param>
        /// <param name="isDeleted">Whether the entity no longer exists in the store.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="isDeleted"/> equals <c>false</c> and 
        /// <paramref name="storeEntity"/>, <paramref name="currentEntity"/> or <paramref name="propertyNames"/> 
        /// is null.</exception>
        internal EntityConflict(Entity currentEntity, Entity storeEntity, IEnumerable<string> propertyNames, bool isDeleted)
        {
            if (currentEntity == null)
            {
                throw new ArgumentNullException("currentEntity");
            }
            this._currentEntity = currentEntity;
            this._isDeleted = isDeleted;

            if (!isDeleted)
            {
                if (storeEntity == null)
                {
                    throw new ArgumentNullException("storeEntity");
                }

                if (propertyNames == null)
                {
                    throw new ArgumentNullException("propertyNames");
                }
                
                this._storeEntity = storeEntity;
                this._propertyNames = new ReadOnlyCollection<string>(propertyNames.ToList());
            }
        }

        /// <summary>
        /// Gets the current <see cref="Entity"/> instance.
        /// </summary>
        public Entity CurrentEntity
        {
            get 
            { 
                return this._currentEntity; 
            }
        }

        /// <summary>
        /// Gets the original <see cref="Entity"/> instance. May be null
        /// if the entity is not modified, or if timestamp concurrency is used.
        /// </summary>
        public Entity OriginalEntity
        {
            get
            {
                if (this._originalEntity == null)
                {
                    IDictionary<string, object> originalValues = this._currentEntity.OriginalValues;
                    if (originalValues != null)
                    {
                        // if the entity is modified, create an original instance
                        this._originalEntity = (Entity)Activator.CreateInstance(this.CurrentEntity.GetType());
                        this._originalEntity.ApplyState(originalValues);
                    }
                }
                return this._originalEntity;
            }
        }

        /// <summary>
        /// Gets the store <see cref="Entity"/> instance. May be null
        /// if the entity no longer exists in the store, in which case
        /// <see cref="IsDeleted"/> will be true.
        /// </summary>
        public Entity StoreEntity
        {
            get
            {
                return this._storeEntity;
            }
        }

        /// <summary>
        /// Gets a collection of the property names in conflict.
        /// </summary>
        public IEnumerable<string> PropertyNames
        {
            get
            {
                if (this._isDeleted)
                {
                    throw new InvalidOperationException(Resource.EntityConflict_IsDeleteConflict);
                }

                return this._propertyNames;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entity no longer exists in the store.
        /// </summary>
        public bool IsDeleted
        {
            get
            {
                return this._isDeleted;
            }
        }

        /// <summary>
        /// Resolves the conflict by updating the entity's original state with the current store state.
        /// If the entity has a timestamp concurrency member, the current store timestamp will be copied
        /// to the entity instance. <see cref="Entity.EntityConflict"/> is also cleared on <see cref="CurrentEntity"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Resolve"/> is called when <see cref="IsDeleted"/>
        /// is <c>true</c>.</exception>
        public void Resolve()
        {
            if (this.CurrentEntity.EntityConflict == null)
            {
                // the conflict has already been resolved
                return;
            }

            if (this.IsDeleted)
            {
                throw new InvalidOperationException(Resource.EntityConflict_CannotResolveDeleteConflict);
            }

            // update the entity's original state
            this.CurrentEntity.UpdateOriginalValues(this.StoreEntity);

            // if the entity has a concurrency timestamp member, we must sync that
            // timestamp value into the current entity
            MetaMember versionMember = this._currentEntity.MetaType.VersionMember;
            if (versionMember != null)
            {
                object storeValue = versionMember.GetValue(this._storeEntity);

                // since Timestamp values are readonly, we must use ApplyState to set the value
                this._currentEntity.ApplyState(new Dictionary<string, object> { { versionMember.Member.Name, storeValue } });
            }

            // clear the conflict now that it is resolved
            this.CurrentEntity.EntityConflict = null;
        }
    }
}
