namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Attribute applied to an association member to indicate that the
    /// association is a compositional relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class CompositionAttribute : Attribute
    {
    }
}
