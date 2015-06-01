using System.Collections.Generic;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Represents a custom method invocation on an entity.
    /// </summary>
    public class EntityAction
    {
        private List<object> _parameters;

        /// <summary>
        /// Initializes a new instance of the EntityAction class
        /// </summary>
        /// <param name="name">Name of the entity action</param>
        /// <param name="parameters">The parameters to pass to the entity action</param>
        public EntityAction(string name, params object[] parameters)
        {
            this.Name = name;
            this._parameters = new List<object>();
            if (parameters != null)
            {
                this._parameters.AddRange(parameters);
            }
        }

        /// <summary>
        /// Gets the name of the entity action
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parameters to pass to the entity action
        /// </summary>
        public IEnumerable<object> Parameters
        {
            get
            {
                return this._parameters;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any parameters were associated with this action.
        /// </summary>
        public bool HasParameters
        {
            get
            {
                return (this._parameters.Count > 0);
            }
        }
    }
}
