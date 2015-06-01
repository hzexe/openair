using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents the result of a query operation
    /// </summary>
    public class QueryCompletedResult
    {
        private ReadOnlyCollection<Entity> _entities;
        private ReadOnlyCollection<Entity> _includedEntities;
        private int _totalCount;
        private ReadOnlyCollection<ValidationResult> _validationErrors;

        /// <summary>
        /// Initializes a new instance of the QueryCompletedResult class
        /// </summary>
        /// <param name="entities">The entities returned from the query</param>
        /// <param name="includedEntities">The included entities returned from the query</param>
        /// <param name="totalCount">The total number of rows for the original query without any paging applied to it.</param>
        /// <param name="validationErrors">A collection of validation errors.</param>
        public QueryCompletedResult(IEnumerable<Entity> entities, IEnumerable<Entity> includedEntities, int totalCount, IEnumerable<ValidationResult> validationErrors)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (includedEntities == null)
            {
                throw new ArgumentNullException("includedEntities");
            }
            if (validationErrors == null)
            {
                throw new ArgumentNullException("validationErrors");
            }

            this._entities = entities.ToList().AsReadOnly();
            this._includedEntities = includedEntities.ToList().AsReadOnly();
            this._totalCount = totalCount;
            this._validationErrors = validationErrors.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the entities returned from the query
        /// </summary>
        public IEnumerable<Entity> Entities
        {
            get
            {
                return this._entities;
            }
        }

        /// <summary>
        /// Gets the included entities returned from the query
        /// </summary>
        public IEnumerable<Entity> IncludedEntities
        {
            get
            {
                return this._includedEntities;
            }
        }

        /// <summary>
        /// Gets the total number of rows for the original query without any paging applied to it.
        /// If the value is -1, the server did not support total-counts.
        /// </summary>
        public int TotalCount
        {
            get
            {
                return this._totalCount;
            }
        }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public IEnumerable<ValidationResult> ValidationErrors
        {
            get
            {
                return this._validationErrors;
            }
        }
    }
}