using System;

namespace OpenRiaServices.DomainServices
{
#if SILVERLIGHT
     /// <summary>
    /// A tagging attribute used to categorize a Type as being of a particular domain.
    /// </summary>
#else
    /// <summary>
    /// A tagging attribute used to categorize a Type as being of a particular domain.
    /// </summary>
    /// <remarks>This attribute will move through the metadata pipeline, so the corresponding
    /// generated types will also have this attribute. This attribute also allows for specification 
    /// of a <see cref="CodeProcessor"/> which will be used during generation of client types.
    /// </remarks>
    /// <seealso cref="CodeProcessor"/>
#endif //SILVERLIGHT
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DomainIdentifierAttribute : Attribute
    {
        private string _name;

        /// <summary>
        /// Constructor that accepts the domain identifier name.
        /// </summary>
        /// <param name="name">Name of the domain identifier. This string has no framework-level semantics.  It is entirely
        /// up to the application developer to decide what constitutes uniqueness or whether it is case-sensitive.
        /// It cannot be null or empty.</param>
        public DomainIdentifierAttribute(string name)
        {
            this._name = name;
        }

        /// <summary>
        /// Gets the name of the domain identifier.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown from the getter if <see cref="Name"/>
        /// is null or empty.</exception>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this._name))
                {
                    throw new InvalidOperationException("Name cannot be null.");
                }

                return this._name;
            }
            internal set
            {
                this._name = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the attributed type represents an application service.
        /// </summary>
        public bool IsApplicationService { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets the <see cref="CodeProcessor"/> type
        /// </summary>
        public Type CodeProcessor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a unique identifier for this DomainIdentifierAttribute.
        /// </summary>
        public override object TypeId
        {
            get
            {
                return this;
            }
        }
#endif //!SILVERLIGHT
    }
}
