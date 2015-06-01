using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OpenRiaServices.DomainServices.Serialization
{
    /// <summary>
    /// Represents a EntityAction when communicating between DomainClient and DomainService.
    /// The struct is used to mimic how KeyValuePairs in a dictionary are serialized by DataContract serializer
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [Browsable(false)]
    [DataContract(/*Name= "KeyValueOfstringArrayOfanyTypety7Ep6D1",*/ Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
    public struct KeyValue<TKey, TValue>
    {
        private TKey _key;
        private TValue _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValue{TKey, TValue}"/> struct.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public KeyValue(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [DataMember(IsRequired = true)]
        public TKey Key { get { return _key; } set { _key = value; } }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember(IsRequired = true)]
        public TValue Value { get { return _value; } set { _value = value; } }
    }
}

namespace OpenRiaServices.DomainServices.Server
{
    using Serialization;

    /// <summary>
    /// Collection representing number of EntityAction invocations
    /// </summary>
    public class EntityActionCollection : List<KeyValue<string, object[]>>
    {
        /// <summary>
        /// Adds a KeyValue to the specified list.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, object[] value)
        {
            base.Add(new KeyValue<string, object[]>(key, value));
        }
    }
}
