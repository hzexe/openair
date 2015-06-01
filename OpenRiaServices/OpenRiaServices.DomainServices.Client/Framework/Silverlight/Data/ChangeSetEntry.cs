using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents a domain operation to be performed on an entity. This is the message
    /// type passed between <see cref="DomainClient"/> and DomainService both for sending operations to
    /// the DomainService as well as for returning operation results back to the <see cref="DomainClient"/>.
    /// </summary>
    [DataContract(Namespace = "DomainServices")]
    [DebuggerDisplay("Operation = {Operation}, Type = {Entity.GetType().Name}")]
    public sealed class ChangeSetEntry
    {
        private Entity _clientEntity;
        private Entity _entity;
        private Entity _originalEntity;
        private Entity _storeEntity;
        private EntityOperationType _operationType;
        private int _id;
        private bool _hasMemberChanges;

        /// <summary>
        /// Gets a value indicating whether any errors were encountered 
        /// during processing of the operation.
        /// </summary>
        internal bool HasError
        {
            get
            {
                return this.HasConflict || (this.ValidationErrors != null && this.ValidationErrors.Any());
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ChangeSetEntry"/> contains conflicts.
        /// </summary>
        public bool HasConflict
        {
            get
            {
                return (this.IsDeleteConflict || (this.ConflictMembers != null && this.ConflictMembers.Any()));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSetEntry"/> class
        /// </summary>
        /// <param name="entity">The entity being operated on</param>
        /// <param name="id">The client ID for the entity, used to correlate server results
        /// with their client entity instances.</param>
        /// <param name="operationType">The operation to be performed</param>
        public ChangeSetEntry(Entity entity, int id, EntityOperationType operationType)
        {
            this._entity = entity;
            this._id = id;
            this._operationType = operationType;
        }

        /// <summary>
        /// Gets or sets the <see cref="Entity"/> being operated on. After a submit operation
        /// has completed, this member may contain updated entity state returned from the
        /// DomainService.
        /// </summary>
        [DataMember]
        public Entity Entity
        {
            get
            {
                return this._entity;
            }
            set
            {
                this._entity = value;
            }
        }

        /// <summary>
        /// Gets or sets the original state of the entity being operated on
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Entity OriginalEntity
        {
            get
            {
                return this._originalEntity;
            }
            set
            {
                this._originalEntity = value;

                if (value != null)
                {
                    this._hasMemberChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the state of the entity in the data store
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Entity StoreEntity
        {
            get
            {
                return this._storeEntity;
            }
            set
            {
                this._storeEntity = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity that was originally submitted <see cref="Entity"/>. Used to correlate 
        /// an <see cref="ChangeSetEntry"/> result back to its originally submitted client entity instance.
        /// </summary>
        public Entity ClientEntity
        {
            get
            {
                return this._clientEntity ?? this._entity;
            }
            set
            {
                this._clientEntity = value;
            }
        }

        /// <summary>
        /// Gets or sets the client Id for the entity. This value is used to correlate server
        /// results with their client entity instances.
        /// </summary>
        [DataMember]
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entity for
        /// this operation has property modifications.
        /// <remarks>Note that even if OriginalEntity hasn't been
        /// set, in the case of entities using a timestamp member
        /// for concurrency, the entity may still be modified. This
        /// flag allows us to distinguish that case from an Update
        /// operation that represents a custom method invocation only.
        /// </remarks>
        /// </summary>
        [DataMember]
        public bool HasMemberChanges
        {
            get
            {
                return this._hasMemberChanges;
            }
            set
            {
                this._hasMemberChanges = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EntityOperationType"/> for this <see cref="ChangeSetEntry"/>
        /// </summary>
        [DataMember]
        public EntityOperationType Operation
        {
            get
            {
                return this._operationType;
            }
            set
            {
                this._operationType = value;
            }
        }

        /// <summary>
        /// Gets or sets the custom methods invoked on the entity, as a set
        /// of method name / parameter set pairs.
        /// </summary>
        [DataMember]
        public IList<Serialization.KeyValue<string, object[]>> EntityActions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the validation errors encountered during the processing of the operation. 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<ValidationResultInfo> ValidationErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of members in conflict. The <see cref="StoreEntity"/> property
        /// contains the current store value for each member in conflict.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<string> ConflictMembers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the conflict is a delete conflict, meaning the
        /// entity no longer exists in the store.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsDeleteConflict
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of IDs of the associated entities for
        /// each association of the Entity
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, int[]> Associations
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of IDs for each association of the <see cref="OriginalEntity"/>
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, int[]> OriginalAssociations
        {
            get;
            set;
        }
        ///// <summary>
        ///// Gets string identity of Entity
        ///// </summary>
        //[DataMember(EmitDefaultValue = false)]
        //public string Identity
        //{
        //    get
        //    {
        //        return Entity != null ? Entity.GetIdentity().ToString() : null;
        //    }
        //}
    }
}
