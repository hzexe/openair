using System;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Event arguments for strongly typed add/remove notifications for collections
    /// containing entities.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Entity"/> type</typeparam>
    public class EntityCollectionChangedEventArgs<TEntity> : EventArgs
    {
        private TEntity _entity;

        /// <summary>
        /// Initializes a new instance of the EntityCollectionChangedEventArgs class
        /// </summary>
        /// <param name="entity">The affected <see cref="Entity"/></param>
        public EntityCollectionChangedEventArgs(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this._entity = entity;
        }

        /// <summary>
        /// The affected <see cref="Entity"/>
        /// </summary>
        public TEntity Entity
        {
            get
            {
                return this._entity;
            }
        }
    }
}
