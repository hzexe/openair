using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Class representing the result of a <see cref="DomainClient"/> operation.
    /// </summary>
    public sealed class DomainClientResult
    {
        private int _totalEntityCount;
        private ReadOnlyCollection<Entity> _entities;
        private ReadOnlyCollection<Entity> _allEntities;
        private object _returnValue;
        private IEnumerable<ValidationResult> _validationErrors;

        private DomainClientResult()
        {
        }

        private DomainClientResult(object returnValue, IEnumerable<ValidationResult> validationErrors)
        {
            this._returnValue = returnValue;
            this._validationErrors = new ReadOnlyCollection<ValidationResult>(validationErrors.ToList());
        }

        private DomainClientResult(IEnumerable<Entity> entities, IEnumerable<Entity> allEntities, int totalEntityCount, IEnumerable<ValidationResult> validationErrors)
        {
            this._entities = new ReadOnlyCollection<Entity>(entities.ToList());
            this._allEntities = new ReadOnlyCollection<Entity>(allEntities.ToList());
            this._totalEntityCount = totalEntityCount;
            this._validationErrors = new ReadOnlyCollection<ValidationResult>(validationErrors.ToList());
        }

        /// <summary>
        /// Creates an Invoke operation result.
        /// </summary>
        /// <param name="returnValue">The return value of the Invoke operation.</param>
        /// <param name="validationErrors">Collection of validation errors for the invocation.</param>
        /// <returns>The result.</returns>
        public static DomainClientResult CreateInvokeResult(object returnValue, IEnumerable<ValidationResult> validationErrors)
        {
            if (validationErrors == null)
            {
                throw new ArgumentNullException("validationErrors");
            }
            return new DomainClientResult(returnValue, validationErrors);
        }

        /// <summary>
        /// Creates a Query operation result.
        /// </summary>
        /// <param name="entities">The top level entities returned from the query.</param>
        /// <param name="allEntities">All entities returned from the query, including associated entities.</param>
        /// <param name="totalEntityCount">The total server count of entities.</param>
        /// <param name="validationErrors">Collection of validation errors for the query.</param>
        /// <returns>The result.</returns>
        public static DomainClientResult CreateQueryResult(IEnumerable<Entity> entities, IEnumerable<Entity> allEntities, int totalEntityCount, IEnumerable<ValidationResult> validationErrors)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (allEntities == null)
            {
                throw new ArgumentNullException("allEntities");
            }
            if (validationErrors == null)
            {
                throw new ArgumentNullException("validationErrors");
            }
            return new DomainClientResult(entities, allEntities, totalEntityCount, validationErrors);
        }

        /// <summary>
        /// Gets the top level entity results.
        /// </summary>
        public IEnumerable<Entity> Entities
        {
            get
            {
                return this._entities;
            }
        }

        /// <summary>
        /// Gets all entity results, including associated entities.
        /// </summary>
        public IEnumerable<Entity> AllEntities
        {
            get
            {
                return this._allEntities;
            }
        }

        /// <summary>
        /// Gets the return value of an Invoke operation. Can be null.
        /// </summary>
        public object ReturnValue
        {
            get
            {
                return this._returnValue;
            }
        }

        /// <summary>
        /// Gets the collection of validation errors.
        /// </summary>
        public IEnumerable<ValidationResult> ValidationErrors
        {
            get
            {
                return this._validationErrors;
            }
        }

        /// <summary>
        /// Gets the total server entity count for the original query without any paging applied to it.
        /// Automatic evaluation of the total server entity count requires the <see cref="EntityQuery.IncludeTotalCount"/>
        /// property to be set to <c>true</c>
        /// If the value is -1, the server did not support total-counts.
        /// </summary>
        public int TotalEntityCount
        {
            get
            {
                return this._totalEntityCount;
            }
        }
    }
}
