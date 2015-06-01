using System;
using System.Collections.Specialized;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Interface to handle collection changed events through a weak listener.
    /// </summary>
    internal interface ICollectionChangedListener
    {
        /// <summary>
        /// Called by the <see cref="WeakCollectionChangedListener"/> when a collection
        /// changed event occurs.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event args</param>
        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
    }

    /// <summary>
    /// A collection change listener that subscribes to the source using a weak reference
    /// </summary>
    internal class WeakCollectionChangedListener
    {
        private INotifyCollectionChanged _source;
        private WeakReference _weakListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakCollectionChangedListener"/>
        /// </summary>
        /// <param name="source">The source event source</param>
        /// <param name="listener">The listener event listener</param>
        private WeakCollectionChangedListener(
            INotifyCollectionChanged source,
            ICollectionChangedListener listener)
        {
            this._source = source;
            this._source.CollectionChanged += this.SourceCollectionChanged;
            this._weakListener = new WeakReference(listener);
        }

        /// <summary>
        /// Creates a weak listener if the source implements <see cref="INotifyCollectionChanged"/>
        /// </summary>
        /// <param name="source">The source to subscribe to</param>
        /// <param name="listener">The collection change listener</param>
        /// <returns>A weak listener instance</returns>
        public static WeakCollectionChangedListener CreateIfNecessary(
            object source,
            ICollectionChangedListener listener)
        {
            INotifyCollectionChanged notify = source as INotifyCollectionChanged;
            if (notify != null)
            {
                return new WeakCollectionChangedListener(notify, listener);
            }
            return null;
        }

        /// <summary>
        /// Handles collection changed events raised by the source
        /// </summary>
        /// <param name="sender">The source collection</param>
        /// <param name="e">The event args</param>
        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._weakListener != null)
            {
                ICollectionChangedListener target = this._weakListener.Target as ICollectionChangedListener;
                if (target != null)
                {
                    target.OnCollectionChanged(sender, e);
                }
                else
                {
                    this.Disconnect();
                }
            }
        }

        /// <summary>
        /// Disconnects this listener from the collection change event
        /// </summary>
        internal void Disconnect()
        {
            if (this._source != null)
            {
                this._source.CollectionChanged -= this.SourceCollectionChanged;
                this._source = null;
                this._weakListener = null;
            }
        }
    }
}
