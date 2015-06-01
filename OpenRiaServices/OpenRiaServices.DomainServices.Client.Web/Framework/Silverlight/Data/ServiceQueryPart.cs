using System;
using System.Globalization;
using System.Runtime.Serialization;

#if SERVERFX
namespace OpenRiaServices.DomainServices.Hosting
#else
namespace OpenRiaServices.DomainServices.Client
#endif
{
    /// <summary>
    /// Represents a single query operator to be applied to a query
    /// </summary>
    internal class ServiceQueryPart
    {
        private string _queryOperator;
        private string _expression;

        /// <summary>
        /// Public constructor
        /// </summary>
        public ServiceQueryPart()
        {
        }

#if SERVERFX
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="queryOperator">The query operator</param>
        /// <param name="expression">The query expression</param>
        public ServiceQueryPart(string queryOperator, string expression)
        {
            if (queryOperator == null)
            {
                throw new ArgumentNullException("queryOperator");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (queryOperator != "where" && queryOperator != "orderby" &&
               queryOperator != "skip" && queryOperator != "take")
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Query_InvalidOperator, queryOperator), "queryOperator");
            }

            this._queryOperator = queryOperator;
            this._expression = expression;
        }
#endif

        /// <summary>
        /// Gets or sets the query operator. Must be one of the supported operators : "where", "orderby", "skip", or "take".
        /// </summary>
        public string QueryOperator
        {
            get
            {
                return this._queryOperator;
            }
            set
            {
                this._queryOperator = value;
            }
        }

        /// <summary>
        /// Gets or sets the query expression.
        /// </summary>
        public string Expression
        {
            get
            {
                return this._expression;
            }
            set
            {
                this._expression = value;
            }
        }

        /// <summary>
        /// Returns a string representation of this <see cref="ServiceQueryPart"/>
        /// </summary>
        /// <returns>The string representation of this <see cref="ServiceQueryPart"/></returns>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}={1}", this.QueryOperator, this.Expression);
        }
    }
}
