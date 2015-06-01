using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Visits all EntityCollection/EntityRef members of an entity, allowing
    /// subclasses to perform work in overridden methods.
    /// </summary>
    internal class EntityVisitor
    {
        /// <summary>
        /// Visit members of the entity, calling the corresponding visit methods for each
        /// </summary>
        /// <param name="entity">The entity to visit</param>
        public virtual void Visit(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            foreach (PropertyInfo association in entity.MetaType.AssociationMembers)
            {
                if (typeof(IEntityCollection).IsAssignableFrom(association.PropertyType))
                {
                    this.VisitEntityCollection((IEntityCollection)association.GetValue(entity, null), association);
                }
                else
                {
                    // Access the EntityRef - might be null if the ref hasn't been
                    // accessed yet
                    IEntityRef entityRef = entity.GetEntityRef(association.Name);
                    this.VisitEntityRef(entityRef, entity, association);
                }
            }
        }

        /// <summary>
        /// Visit the specified <see cref="IEntityCollection"/>.
        /// </summary>
        /// <param name="entityCollection">The <see cref="IEntityCollection"/>.</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the collection member.</param>
        protected virtual void VisitEntityCollection(IEntityCollection entityCollection, PropertyInfo propertyInfo)
        {
        }

        /// <summary>
        /// Visit an <see cref="EntityRef&lt;TEntity&gt;"/> member
        /// </summary>
        /// <param name="entityRef">The EntityRef to visit.</param>
        /// <param name="parent">The parent of the reference member</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the reference member</param>
        protected virtual void VisitEntityRef(IEntityRef entityRef, Entity parent, PropertyInfo propertyInfo)
        {
        }
    }
}
