using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using OpenRiaServices.DomainServices.Client;

namespace OpenRiaServices.Data.DomainServices
{
    /// <summary>
    /// Observable list of entities. This list stays in sync with its <see cref="Source"/>
    /// collection as well as its backing <see cref="EntitySet"/>.
    /// </summary>
    /// <remarks>
    /// All items added or removed from this list will also be added or removed from the
    /// backing <see cref="OpenRiaServices.DomainServices.Client.EntitySet{T}"/>.
    /// </remarks>
    /// <typeparam name="T">The entity type of this list</typeparam>
    public class EntityList<T> : ObservableCollection<T>, ICollectionChangedListener where T : Entity
    {
        #region Member fields

        private readonly HashSet<T> _entities = new HashSet<T>();
        private readonly EntitySet<T> _entitySet;
        private readonly WeakCollectionChangedListener _weakCollectionChangedLister;

        private IEnumerable<T> _source;

        private bool _updating;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityList{T}"/>
        /// </summary>
        /// <param name="entitySet">The
        /// <see cref="OpenRiaServices.DomainServices.Client.EntitySet{T}"/> that backs this list. All
        /// items added or removed from this list will also be added or removed from the backing
        /// <see cref="EntitySet"/>.
        /// </param>
        public EntityList(EntitySet<T> entitySet)
            : this(entitySet, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityList{T}"/> with the specified
        /// <paramref name="source"/>
        /// </summary>
        /// <param name="entitySet">The
        /// <see cref="OpenRiaServices.DomainServices.Client.EntitySet{T}"/> that backs this list. All
        /// items added or removed from this list will also be added or removed from the backing
        /// <see cref="EntitySet"/>.
        /// </param>
        /// <param name="source">The source collection used to populate this list</param>
        public EntityList(EntitySet<T> entitySet, IEnumerable<T> source)
        {
            if (entitySet == null)
            {
                throw new ArgumentNullException("entitySet");
            }

            this._entitySet = entitySet;
            this._weakCollectionChangedLister =
                WeakCollectionChangedListener.CreateIfNecessary(this._entitySet, this);

            this.Source = source;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the backing <see cref="OpenRiaServices.DomainServices.Client.EntitySet{T}"/>
        /// </summary>
        /// <remarks>
        /// All items added or removed from this list will also be added or removed from the
        /// backing <see cref="OpenRiaServices.DomainServices.Client.EntitySet{T}"/>.
        /// </remarks>
        public EntitySet<T> EntitySet
        {
            get { return this._entitySet; }
        }

        /// <summary>
        /// Gets or sets the source collection used to populate this list
        /// </summary>
        /// <remarks>
        /// Setting the source will <see cref="Collection{T}.Clear"/> this list and add all the items in
        /// the new source. If the new collection is observable, changes to the source will
        /// also be reflected in this list.
        /// </remarks>
        public IEnumerable<T> Source
        {
            get
            {
                return this._source;
            }

            set
            {
                if ((value is EntitySet) || (value is EntityCollection<T>))
                {
                    throw new ArgumentException(Resources.NoESorEC, "value");
                }

                if (this._source != value)
                {
                    // It's ok to use standard event subscription here since we anticipate
                    // the lifetimes of the EntityList and source will be about equal
                    INotifyCollectionChanged notifyingSource = this._source as INotifyCollectionChanged;
                    if (notifyingSource != null)
                    {
                        notifyingSource.CollectionChanged -= this.OnSourceCollectionChanged;
                    }

                    this._source = value;

                    notifyingSource = this._source as INotifyCollectionChanged;
                    if (notifyingSource != null)
                    {
                        notifyingSource.CollectionChanged += this.OnSourceCollectionChanged;
                    }

                    this.UpdateAndIgnoreReentrance(obj => this.ResetToSource(), null);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Takes an updating action and ignores re-entrant callbacks
        /// </summary>
        /// <param name="updateAction">The action to invoke</param>
        /// <param name="obj">The parameter to pass to the action</param>
        private void UpdateAndIgnoreReentrance(Action<object> updateAction, object obj)
        {
            if (this._updating)
            {
                return;
            }

            try
            {
                this._updating = true;
                updateAction(obj);
            }
            finally
            {
                this._updating = false;
            }
        }

        /// <summary>
        /// Resets this list and re-adds all the entities from the <see cref="Source"/>.
        /// </summary>
        private void ResetToSource()
        {
            this._entities.Clear();
            this.Clear();

            if (this.Source != null)
            {
                foreach (T entity in this.Source)
                {
                    this._entities.Add(entity);
                    this.Add(entity);
                }
            }
        }

        /// <summary>
        /// Overridden to ensure new entities are added to the backing <see cref="EntitySet"/>
        /// </summary>
        /// <param name="index">The index of the new entity</param>
        /// <param name="item">The newly added entity</param>
        protected override void InsertItem(int index, T item)
        {
            if (item.EntityState == EntityState.Detached)
            {
                this.UpdateAndIgnoreReentrance(this.AddToEntitySet, item);
            }
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Adds the entity to the <see cref="EntitySet"/>
        /// </summary>
        /// <param name="obj">The entity to add</param>
        private void AddToEntitySet(object obj)
        {
            this.EntitySet.Add((T)obj);
        }

        /// <summary>
        /// Overridden to ensure existing entities are removed from the backing <see cref="EntitySet"/>
        /// </summary>
        /// <param name="index">The index of the entity to remove</param>
        protected override void RemoveItem(int index)
        {
            T item = this[index];
            if (item.EntityState != EntityState.Detached)
            {
                this.UpdateAndIgnoreReentrance(this.RemoveFromEntitySet, item);
            }
            base.RemoveItem(index);
        }

        /// <summary>
        /// Removes the entity from the <see cref="EntitySet"/>
        /// </summary>
        /// <param name="obj">The entity to remove</param>
        private void RemoveFromEntitySet(object obj)
        {
            this.EntitySet.Remove((T)obj);
        }

        /// <summary>
        /// Handles changes to the source collection by adding or removing entities from the list
        /// </summary>
        /// <param name="sender">The source collection</param>
        /// <param name="e">The event args</param>
        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UpdateAndIgnoreReentrance(this.HandleSourceCollectionChanged, e);
        }

        /// <summary>
        /// Adds or removes entities from the list to keep it in sync with the source collection
        /// </summary>
        /// <param name="obj">The collection changed event args</param>
        private void HandleSourceCollectionChanged(object obj)
        {
            NotifyCollectionChangedEventArgs e = (NotifyCollectionChangedEventArgs)obj;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Add the new items
                int index = e.NewStartingIndex;
                foreach (T item in e.NewItems)
                {
                    this._entities.Add(item);
                    this.Insert(index++, item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Remove the old items
                foreach (T item in e.OldItems)
                {
                    this._entities.Remove(item);
                    this.RemoveAt(e.OldStartingIndex);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                // Remove the old items
                foreach (T item in e.OldItems)
                {
                    this._entities.Remove(item);
                    this.RemoveAt(e.OldStartingIndex);
                }

                // And add the new ones
                foreach (T item in e.NewItems)
                {
                    int index = e.NewStartingIndex;
                    this._entities.Add(item);
                    this.Insert(index++, item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.ResetToSource();
            }
        }

        /// <summary>
        /// Handles changes to the <see cref="EntitySet"/> by conditionally adding or removing entities
        /// from the list
        /// </summary>
        /// <param name="sender">The <see cref="EntitySet"/></param>
        /// <param name="e">The event args</param>
        private void OnEntitySetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UpdateAndIgnoreReentrance(this.HandleEntitySetCollectionChanged, e);
        }

        /// <summary>
        /// Adds or removes entities from the list to keep it in sync with the backing <see cref="EntitySet"/>
        /// </summary>
        /// <remarks>
        /// Entities added to the <see cref="EntitySet"/> will only be added to the list if they already
        /// exist in the <see cref="Source"/>. This typically only occurs when an entity is removed and then
        /// the changes are rejected causing the entity to be re-added.
        /// </remarks>
        /// <param name="obj">The collection changed event args</param>
        private void HandleEntitySetCollectionChanged(object obj)
        {
            NotifyCollectionChangedEventArgs e = (NotifyCollectionChangedEventArgs)obj;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Add the new entities if they are in the source collection
                foreach (T item in e.NewItems)
                {
                    if (this._entities.Contains(item))
                    {
                        this.Add(item);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Remove the old entities
                foreach (T item in e.OldItems)
                {
                    this.Remove(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                // Remove the old entities
                foreach (T item in e.OldItems)
                {
                    this.Remove(item);
                }

                // And add the new entities if they are in the source collection
                foreach (T item in e.NewItems)
                {
                    if (this._entities.Contains(item))
                    {
                        this.Add(item);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // Reset and add entities if they are in the source collection
                this.Clear();

                foreach (T item in this.EntitySet)
                {
                    if (this._entities.Contains(item))
                    {
                        this.Add(item);
                    }
                }
            }
        }

        #endregion

        #region ICollectionChangedListener

        /// <summary>
        /// Handles changes to the <see cref="EntitySet"/> by conditionally adding or removing entities
        /// from the list
        /// </summary>
        /// <param name="sender">The <see cref="EntitySet"/></param>
        /// <param name="e">The event args</param>
        void ICollectionChangedListener.OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnEntitySetCollectionChanged(sender, e);
        }

        #endregion
    }
}
