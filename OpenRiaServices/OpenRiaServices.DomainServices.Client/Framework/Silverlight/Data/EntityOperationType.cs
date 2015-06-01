using System.Runtime.Serialization;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Enumeration of the types of operations that can be performed on an <see cref="Entity"/>
    /// </summary>
    [DataContract(Namespace = "DomainServices")]
    public enum EntityOperationType
    {
        /// <summary>
        /// Indicates that no operation is to be performed on the <see cref="Entity"/>
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// Indicates an insert operation for a new <see cref="Entity"/>
        /// </summary>
        [EnumMember]
        Insert = 2,

        /// <summary>
        /// Indicates an update operation for an existing <see cref="Entity"/>
        /// </summary>
        [EnumMember]
        Update = 3,

        /// <summary>
        /// Indicates a delete operation for an existing <see cref="Entity"/>
        /// </summary>
        [EnumMember]
        Delete = 4
    }
}