using System;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Attribute used by code generation to declare that a method on an entity is an EntityAction.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class EntityActionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityActionAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the custom method.</param>
        public EntityActionAttribute(string name)
            : this(name, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityActionAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the custom method.</param>
        /// <param name="allowMultipleInvocations">if set to <c>true</c> [allow multiple invocations].</param>
        public EntityActionAttribute(string name, bool allowMultipleInvocations)
        {
            this.Name = name;
            this.CanInvokePropertyName = string.Format("Can{0}", name);
            this.IsInvokedPropertyName = string.Format("Is{0}Invoked", name);
            this.AllowMultipleInvocations = allowMultipleInvocations;
        }

        /// <summary>
        /// Gets the name of the custom method.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the name of the property indicating if the property can be invoked or not.
        /// </summary>
        /// <value>
        /// The name of the property indicating if the property can be invoked or not.
        /// </value>
        public string CanInvokePropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of property indicating if there is any pending invocations of the method or not.
        /// </summary>
        /// <value>
        /// The name of property indicating if there is any pending invocations of the method or not.
        /// </value>
        public string IsInvokedPropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the method is allowed to be called multiple times during the same submit.
        /// </summary>
        /// <value>
        /// <c>true</c> if multiple invocations is allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultipleInvocations { get; set; }
    }
}
