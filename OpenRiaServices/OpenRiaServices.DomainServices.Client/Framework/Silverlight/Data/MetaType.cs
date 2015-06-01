using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using OpenRiaServices.DomainServices.Server.Data;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Class providing additional "meta" information for a Type.
    /// <remarks>
    /// Consider adding any commonly accessed or computed information about a Type
    /// to this class, to improve performance and code factoring.
    /// MetaType must be fully readonly before GetMetaType returns. While it is cached per thread,
    /// it may be cached on types elsewhere providing multithreaded access to visible properties.
    /// </remarks>
    /// </summary>
    [DebuggerDisplay("Type = {_type.Name}")]
    internal sealed class MetaType
    {
        /// <summary>
        /// We're using TLS for performance to avoid taking locks. In multithreaded
        /// scenarios this means that there may be multiple MetaType caches around
        /// but that shouldn't be a problem.
        /// </summary>
        [ThreadStatic]
        private static Dictionary<Type, MetaType> _metaTypes;

        private bool _isComplex;
        private bool _hasComplexMembers;
        private bool _requiresValidation;
        private bool _hasComposition;
        private bool _shouldRoundtripOriginal;
        private Type _type;
        private IEnumerable<Type> _childTypes = new List<Type>();
        private Dictionary<string, MetaMember> _metaMembers = new Dictionary<string, MetaMember>();
        private MetaMember _versionMember;

        private IDictionary<string, EntityActionAttribute> _customUpdateMethods = new Dictionary<string, EntityActionAttribute>();

        /// <summary>
        /// Returns the MetaType for the specified Type.
        /// </summary>
        /// <param name="type">The Type to provide the MetaType for.</param>
        /// <returns>The constructed MetaType.</returns>
        public static MetaType GetMetaType(Type type)
        {
            Debug.Assert(!TypeUtility.IsPredefinedType(type), "Should never attempt to create a MetaType for a base type.");

            MetaType metaType = null;
            if (!MetaTypes.TryGetValue(type, out metaType))
            {
                metaType = new MetaType(type);
                MetaTypes[type] = metaType;
            }
            return metaType;
        }

        private MetaType(Type type)
        {
            _customUpdateMethods = (from method in type.GetMethods(MemberBindingFlags)
                                    let attributes = method.GetCustomAttributes(typeof(EntityActionAttribute), false)
                                    where attributes.Length == 1
                                    select (EntityActionAttribute)attributes[0]
                                   ).ToDictionary(cua => cua.Name, cua => cua);

            IEnumerable<PropertyInfo> properties = type.GetProperties(MemberBindingFlags).Where(p => p.GetIndexParameters().Length == 0).OrderBy(p => p.Name);

            this._isComplex = TypeUtility.IsComplexType(type);

            bool hasOtherRoundtripMembers = false;
            foreach (PropertyInfo property in properties)
            {
                MetaMember metaMember = new MetaMember(this, property);
                metaMember.IsCollection = TypeUtility.IsSupportedCollectionType(property.PropertyType);
                if ((property.GetGetMethod() != null) && (property.GetSetMethod() != null))
                {
                    bool isPredefinedType = TypeUtility.IsPredefinedType(property.PropertyType);
                    bool isComplex = TypeUtility.IsSupportedComplexType(property.PropertyType);
                    metaMember.IsDataMember = isPredefinedType || isComplex;
                    metaMember.IsComplex = isComplex;

                    if (isComplex)
                    {
                        this._hasComplexMembers = true;
                    }
                }

                if (property.GetGetMethod() != null)
                {
                    metaMember.RequiresValidation = property.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0;
                }

                if (property.GetCustomAttributes(typeof(AssociationAttribute), false).Length > 0)
                {
                    metaMember.IsAssociationMember = true;
                }

                bool isKeyMember = property.GetCustomAttributes(typeof(KeyAttribute), false).Length > 0;
                if (isKeyMember)
                {
                    metaMember.IsKeyMember = true;
                }

                if (property.GetCustomAttributes(typeof(CompositionAttribute), false).Length > 0)
                {
                    this._hasComposition = true;
                }

                if (MetaType.IsRoundtripMember(metaMember))
                {
                    if (property.GetCustomAttributes(typeof(TimestampAttribute), false).Length != 0 &&
                        property.GetCustomAttributes(typeof(ConcurrencyCheckAttribute), false).Length != 0)
                    {
                        // Look for a concurrency version member. There should be only one
                        // (DomainService validation ensures this).
                        this._versionMember = metaMember;
                    }
                    else if (!isKeyMember)
                    {
                        // Look for non-key, non-timestamp roundtripped members. We can skip key members,
                        // since they're read only and cannot be modified by the client anyways.
                        hasOtherRoundtripMembers = true;
                    }
                    metaMember.IsRoundtripMember = true;
                }

                metaMember.IsMergable = MetaType.IsMergeableMember(metaMember);
                

                metaMember.EditableAttribute = (EditableAttribute)property.GetCustomAttributes(typeof(EditableAttribute), false).SingleOrDefault();
                this._metaMembers.Add(property.Name, metaMember);
            }

            if ((this._versionMember != null) && !hasOtherRoundtripMembers)
            {
                this._shouldRoundtripOriginal = false;
            }
            else
            {
                this._shouldRoundtripOriginal = true;
            }

            if (this._hasComposition)
            {
                this._childTypes = type
                        .GetProperties(MemberBindingFlags)
                        .Where(p => p.GetCustomAttributes(typeof(CompositionAttribute), false).Any())
                        .Select(p => TypeUtility.GetElementType(p.PropertyType)).ToArray();
            }

            this._type = type;

            this.CalculateAttributesRecursive(type, new HashSet<Type>());
        }

        /// <summary>
        /// Gets the correct property binding flags to use for data members.
        /// </summary>
        internal static BindingFlags MemberBindingFlags
        {
            get
            {
                return BindingFlags.Instance | BindingFlags.Public;
            }
        }

        public MetaMember this[string memberName]
        {
            get
            {
                MetaMember mm = null;
                if (this._metaMembers.TryGetValue(memberName, out mm))
                {
                    return mm;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="EntityActionAttribute"/> for the custom update method with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>the EntityActionAttribute for the custom method; or <c>null</c> if no method was found</returns>
        public EntityActionAttribute GetEntityAction(string name)
        {
            EntityActionAttribute res;
            _customUpdateMethods.TryGetValue(name, out res);
            return res;
        }

        /// <summary>
        /// Gets <see cref="EntityActionAttribute" /> for all custom update method on the MetaType.
        /// </summary>
        /// <returns>Meta information about all entity actions on the type</returns>
        public IEnumerable<EntityActionAttribute> GetEntityActions()
        {
            return _customUpdateMethods.Values;
        }

        private static bool IsRoundtripMember(MetaMember metaMember)
        {
            bool isRoundtripEntity = metaMember.Member.DeclaringType.GetCustomAttributes(typeof(RoundtripOriginalAttribute), true).Length > 0;

            return metaMember.IsDataMember && !metaMember.IsAssociationMember &&
                       (isRoundtripEntity || metaMember.Member.GetCustomAttributes(typeof(RoundtripOriginalAttribute), false).Length != 0);
        }

        private static bool IsMergeableMember(MetaMember metaMember)
        {
            return metaMember.IsDataMember && !metaMember.IsAssociationMember &&
                   (metaMember.Member.GetCustomAttributes(typeof (MergeAttribute), true).Length == 0 ||
                   ((MergeAttribute)metaMember.Member.GetCustomAttributes(typeof (MergeAttribute), true).GetValue(0)).IsMergeable);
        }


        private static Dictionary<Type, MetaType> MetaTypes
        {
            get
            {
                if (_metaTypes == null)
                {
                    _metaTypes = new Dictionary<Type, MetaType>();
                }
                return _metaTypes;
            }
        }

        /// <summary>
        /// Gets the underlying CLR type for this MetaType
        /// </summary>
        public Type Type
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is a complex type.
        /// </summary>
        public bool IsComplex
        {
            get
            {
                return this._isComplex;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type has complex members. This does include
        /// complex object collections.
        /// </summary>
        public bool HasComplexMembers
        {
            get
            {
                return this._hasComplexMembers;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the original should be roundtripped for this Type.
        /// </summary>
        public bool ShouldRoundtripOriginal
        {
            get
            {
                return this._shouldRoundtripOriginal;
            }
        }

        /// <summary>
        /// Gets the concurrency version member for this type. May be null.
        /// </summary>
        public MetaMember VersionMember
        {
            get
            {
                return this._versionMember;
            }
        }

        /// <summary>
        /// Gets the collection of members for this Type.
        /// </summary>
        public IEnumerable<MetaMember> Members
        {
            get
            {
                return this._metaMembers.Values;
            }
        }

        /// <summary>
        /// Gets the collection of key members for this entity Type.
        /// </summary>
        public IEnumerable<PropertyInfo> KeyMembers
        {
            get
            {
                return this._metaMembers.Values.Where(m => m.IsKeyMember).Select(m => m.Member);
            }
        }

        /// <summary>
        /// Gets the collection of data members for this Type.
        /// </summary>
        public IEnumerable<MetaMember> DataMembers
        {
            get
            {
                return this._metaMembers.Values.Where(m => m.IsDataMember);
            }
        }

        /// <summary>
        /// Gets the collection of members that should be roundtripped in
        /// the original entity.
        /// </summary>
        public IEnumerable<PropertyInfo> RoundtripMembers
        {
            get
            {
                return this._metaMembers.Values.Where(m => m.IsRoundtripMember).Select(m => m.Member);
            }
        }

        /// <summary>
        /// Gets the collection of association members for this entity Type.
        /// </summary>
        public IEnumerable<PropertyInfo> AssociationMembers
        {
            get
            {
                return this._metaMembers.Values.Where(m => m.IsAssociationMember).Select(m => m.Member);
            }
        }

        public IEnumerable<String> MergableMembers
        {
            get
            {
                return this._metaMembers.Values.Where(m => m.IsMergable).Select(m => m.Member.Name);
            }
        }

        /// <summary>
        /// Gets the collection of child types this entity Type composes.
        /// </summary>
        public IEnumerable<Type> ChildTypes
        {
            get
            {
                return this._childTypes;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Type has any Type or member level
        /// validation attributes applied. The check is recursive through any complex
        /// type members.
        /// </summary>
        public bool RequiresValidation
        {
            get
            {
                return this._requiresValidation;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Type has any members marked with
        /// CompositionAttribute.
        /// </summary>
        public bool HasComposition
        {
            get
            {
                return this._hasComposition;
            }
        }

        /// <summary>
        /// Gets the Type level validation errors for the underlying Type.
        /// </summary>
        public IEnumerable<ValidationAttribute> ValidationAttributes
        {
            get
            {
                return this._type.GetCustomAttributes(true).OfType<ValidationAttribute>();
            }
        }

        /// <summary>
        /// This recursive function visits every property in the type tree. For each property,
        /// we inspect the attributes and set meta attributes as needed.
        /// </summary>
        /// <param name="type">The root type to calculate attributes for.</param>
        /// <param name="visited">Visited set for recursion.</param>
        private void CalculateAttributesRecursive(Type type, HashSet<Type> visited)
        {
            if (!visited.Add(type))
            {
                return;
            }

            if (!this._requiresValidation)
            {
                this._requiresValidation = type.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0;
            }

            // visit all data members
            IEnumerable<PropertyInfo> properties = type.GetProperties(MemberBindingFlags).Where(p => p.GetIndexParameters().Length == 0).OrderBy(p => p.Name);
            foreach (PropertyInfo property in properties)
            {
                if (!this._requiresValidation)
                {
                    this._requiresValidation = property.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0;
                }

                // for complex members we must drill in recursively
                if (TypeUtility.IsSupportedComplexType(property.PropertyType))
                {
                    Type elementType = TypeUtility.GetElementType(property.PropertyType);
                    this.CalculateAttributesRecursive(elementType, visited);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether entity actions for code using code gen before version 4.4.0
        /// has been discovered.
        /// </summary>
        internal bool IsLegacyEntityActionsDiscovered { get; set; }

        /// <summary>
        /// Add's a EntityAction with the given name and property names.
        /// This method is only used when discovering EntityActions for code which has used the code gen 
        /// before version 4.4.0.
        /// </summary>
        /// <param name="name">The name of the entity action.</param>
        /// <param name="canInvokePropertyName">Name of the can invoke property.</param>
        /// <param name="isInvokedPropertyName">Name of the is invoked property.</param>
        internal void TryAddLegacyEntityAction(string name, string canInvokePropertyName, string isInvokedPropertyName)
        {
            if (!_customUpdateMethods.ContainsKey(name))
            {
                _customUpdateMethods.Add(name, new EntityActionAttribute(name)
                {
                    CanInvokePropertyName = canInvokePropertyName,
                    IsInvokedPropertyName = isInvokedPropertyName,
                });
            }
        }
    }

    /// <summary>
    /// This class caches all the interesting attributes of a method.
    /// </summary>
    [DebuggerDisplay("Name = {Method.Name}")]
    internal sealed class MetaMethod
    {
        public MetaMethod(MetaType metaType, MethodInfo methodInfo)
        {
            this.Method = methodInfo;
            this.MetaType = metaType;
        }

        public MetaType MetaType
        {
            get;
            private set;
        }

        public MethodInfo Method
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// This class caches all the interesting attributes of an property.
    /// </summary>
    [DebuggerDisplay("Name = {Member.Name}")]
    internal sealed class MetaMember
    {
        public MetaMember(MetaType metaType, PropertyInfo propertyInfo)
        {
            this.Member = propertyInfo;
            this.MetaType = metaType;
        }

        public MetaType MetaType
        {
            get;
            private set;
        }

        public EditableAttribute EditableAttribute
        {
            get;
            set;
        }

        public PropertyInfo Member
        {
            get;
            private set;
        }

        public bool IsAssociationMember
        {
            get;
            set;
        }

        public bool IsDataMember
        {
            get;
            set;
        }

        public bool IsKeyMember
        {
            get;
            set;
        }

        public bool IsRoundtripMember
        {
            get;
            set;
        }

        public bool IsComplex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this member is a supported collection type.
        /// </summary>
        public bool IsCollection
        {
            get;
            set;
        }

        /// <summary>
        /// Returns <c>true</c> if the member has a property validator.
        /// </summary>
        /// <remarks>The return value does not take into account whether or not the member requires
        /// type validation.</remarks>
        public bool RequiresValidation
        {
            get;
            set;
        }

        public object GetValue(object instance)
        {
            // TODO : In the future as a performance optimization we should emit a delegate
            // to invoke the getter, rather than use reflection.
            return this.Member.GetValue(instance, null);
        }

        public bool IsMergable
        {
            get; 
            set;
        }
    }
}
