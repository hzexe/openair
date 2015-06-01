using System;

namespace OpenRiaServices.DomainServices
{
    /// <summary>
    /// Attribute used to indicate that an association references entities belonging to an external
    /// DomainContext.
    /// </summary>
    /// <remarks>
    /// When applied to an entity association member, this attribute indicates that the framework should not
    /// create a corresponding EntitySet in the generated client-side code.  Consumers of the
    /// client-side property will need to add a DomainContext reference to the appropriate DomainContext
    /// containing the external entity type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ExternalReferenceAttribute : Attribute { }
}
