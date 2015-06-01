using System;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// The result of a sucessfully completed submit operation
    /// </summary>
    public class SubmitResult
    {
        private readonly EntityChangeSet _changeSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitResult"/> class.
        /// </summary>
        /// <param name="changeSet">The changeset which was submitted.</param>
        public SubmitResult(EntityChangeSet changeSet)
        {
            _changeSet = changeSet;
        }

        /// <summary>
        /// Gets the changeset which was submitted.
        /// </summary>
        /// <value>
        /// The changeset which was submitted.
        /// </value>
        public EntityChangeSet ChangeSet { get { return _changeSet; } }
    }
}
