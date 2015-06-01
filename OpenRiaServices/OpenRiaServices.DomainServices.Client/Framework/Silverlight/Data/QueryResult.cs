using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

#if SERVERFX
namespace OpenRiaServices.DomainServices.Hosting
#else
namespace OpenRiaServices.DomainServices.Client
#endif
{
    /// <summary>
    /// Message type used to communicate query results between server and client.
    /// </summary>
    [DataContract(Namespace = "DomainServices")]
    public abstract class QueryResult
    {
        private int _totalCount;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected QueryResult()
        {
        }

        /// <summary>
        /// Gets or sets the total number of rows for the original query without any paging applied to it.
        /// If the value is -1, the server did not support total-counts.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int TotalCount
        {
            get
            {
                return this._totalCount;
            }
            set
            {
                this._totalCount = value;
            }
        }

        /// <summary>
        /// Gets the root results.
        /// </summary>
        /// <returns>The root results.</returns>
        public abstract IEnumerable<object> GetRootResults();

        /// <summary>
        /// Gets the included results.
        /// </summary>
        /// <returns>The included results.</returns>
        public abstract IEnumerable<object> GetIncludedResults();
    }

    /// <summary>
    /// Message type used to communicate query results between server and client.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    [DataContract(Name = "QueryResultOf{0}", Namespace = "DomainServices")]
    public sealed class QueryResult<T> : QueryResult
    {
        /// <summary>
        /// Initializes a new instance of the QueryResult class
        /// </summary>
        public QueryResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the QueryResult class with the specified
        /// collection of result items.
        /// </summary>
        /// <param name="results">The results</param>
        public QueryResult(IEnumerable<T> results)
        {
            this.RootResults = results;
        }

        /// <summary>
        /// Initializes a new instance of the QueryResult class with the specified
        /// collection of result items and total count.
        /// </summary>
        /// <param name="results">The query results.</param>
        /// <param name="totalCount">The total number of rows based on the input query, but without 
        /// any paging applied to it.</param>
        public QueryResult(IEnumerable<T> results, int totalCount)
        {
            this.RootResults = results;
            this.TotalCount = totalCount;
        }

        /// <summary>
        /// Returns the top-level query results.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<T> RootResults
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the included query results.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<object> IncludedResults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the root results.
        /// </summary>
        /// <returns>The root results.</returns>
        public override IEnumerable<object> GetRootResults()
        {
            return (this.RootResults ?? new T[0]).Cast<object>();
        }

        /// <summary>
        /// Gets the included results.
        /// </summary>
        /// <returns>The included results.</returns>
        public override IEnumerable<object> GetIncludedResults()
        {
            return this.IncludedResults ?? new object[0];
        }
    }
}
