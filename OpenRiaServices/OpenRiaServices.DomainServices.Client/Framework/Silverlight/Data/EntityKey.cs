using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Base class for keys representing unique entity identity, suitable for hashing. 
    /// This class also provides factory methods for the creation of keys.
    /// </summary>
    public abstract class EntityKey
    {
        /// <summary>
        /// Creates an key using the specified key values
        /// </summary>
        /// <typeparam name="T1">First key value type</typeparam>
        /// <typeparam name="T2">Second key value type</typeparam>
        /// <param name="v1">First key value</param>
        /// <param name="v2">Second key value</param>
        /// <returns>The entity key</returns>
        public static EntityKey Create<T1, T2>(T1 v1, T2 v2)
        {
            return new EntityKey<T1, T2>(v1, v2);
        }

        /// <summary>
        /// Creates an key using the specified key values
        /// </summary>
        /// <typeparam name="T1">First key value type</typeparam>
        /// <typeparam name="T2">Second key value type</typeparam>
        /// <typeparam name="T3">Third key value type</typeparam>
        /// <param name="v1">First key value</param>
        /// <param name="v2">Second key value</param>
        /// <param name="v3">Third key value</param>
        /// <returns>The entity key</returns>
        public static EntityKey Create<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
        {
            return new EntityKey<T1, EntityKey<T2, T3>>(v1, new EntityKey<T2, T3>(v2, v3));
        }

        /// <summary>
        /// Creates an key using the specified key values.
        /// </summary>
        /// <param name="keyValues">Array of key values</param>
        /// <returns>The entity key</returns>
        public static EntityKey Create(params object[] keyValues)
        {
            if (keyValues == null)
            {
                throw new ArgumentNullException("keyValues");
            }

            int keyLength = keyValues.Length;
            if (keyLength == 0)
            {
                throw new ArgumentException(Resource.EntityKey_EmptyKeyMembers, "keyValues");
            }

            object keyValue;
            if (keyLength == 1)
            {
                // single part key
                keyValue = keyValues[0];
                VerifyKeyValueNotNull(keyValue);
                return (EntityKey)Activator.CreateInstance(typeof(EntityKey<>).MakeGenericType(keyValue.GetType()), keyValue);
            }

            // We're dealing with a multipart key {v1, v2, v3, v4} of types {T1, T2, T3, T4}
            // Build up a multipart key of the type EntityKey<T1, Entity<T2, EntityType<T3, T4>>>
            object lastKeyValue = keyValues[keyLength - 1];
            VerifyKeyValueNotNull(lastKeyValue);
            keyValue = keyValues[keyLength - 2];
            VerifyKeyValueNotNull(keyValue);
            EntityKey key = (EntityKey)Activator.CreateInstance(typeof(EntityKey<,>).MakeGenericType(keyValue.GetType(), lastKeyValue.GetType()), keyValue, lastKeyValue);
            for (int i = keyLength - 3; i >= 0; i--)
            {
                keyValue = keyValues[i];
                VerifyKeyValueNotNull(keyValue);
                key = (EntityKey)Activator.CreateInstance(typeof(EntityKey<,>).MakeGenericType(keyValue.GetType(), key.GetType()), keyValue, key);
            }

            return key;
        }

        /// <summary>
        /// Verify that the specified key value isn't null, and throw if it is
        /// </summary>
        /// <param name="value">The key value to validate</param>
        internal static void VerifyKeyValueNotNull(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value", Resource.EntityKey_CannotBeNull);
            }
        }

        /// <summary>
        /// Formats the key as a set of key values
        /// </summary>
        /// <returns>The formatted key representation</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            this.FormatKey(sb);
            return "{" + sb.ToString() + "}";
        }

        /// <summary>
        /// Append the key values for this key to the <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to</param>
        internal abstract void FormatKey(StringBuilder sb);

        /// <summary>
        /// Appends the string representation of the specified value
        /// to the <see cref="StringBuilder"/>
        /// </summary>
        /// <typeparam name="T">The key value type</typeparam>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to</param>
        /// <param name="value">The key value to format</param>
        internal static void FormatKeyValue<T>(StringBuilder sb, T value)
        {
            if (sb == null)
            {
                throw new ArgumentNullException("sb");
            }
            if (value == null)
            {
                throw new ArgumentNullException(Resource.EntityKey_CannotBeNull);
            }

            EntityKey entityKey = value as EntityKey;
            if (entityKey != null)
            {
                entityKey.FormatKey(sb);
            }
            else
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(value.ToString());
            }
        }
    }

    /// <summary>
    /// Strongly typed single member key implementation that avoids value boxing.
    /// </summary>
    /// <typeparam name="T">The Type of the key member</typeparam>
    internal class EntityKey<T> : EntityKey, IEquatable<EntityKey<T>>
    {
        private T _v;

        /// <summary>
        /// Constructs a key
        /// </summary>
        /// <param name="v">The key value</param>
        public EntityKey(T v)
        {
            VerifyKeyValueNotNull(v);
            this._v = v;
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return this._v.GetHashCode();
        }

        /// <summary>
        /// Determines if the specified key is equal to
        /// this key
        /// </summary>
        /// <param name="obj">The other key</param>
        /// <returns>True if the keys are equal</returns>
        public override bool Equals(object obj)
        {
            return this.Equals((EntityKey<T>)obj);
        }

        /// <summary>
        /// Recursively build the list of key values
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to</param>
        internal override void FormatKey(StringBuilder sb)
        {
            FormatKeyValue(sb, this._v);
        }

        #region IEquatable<T> Members

        /// <summary>
        /// Determines if the specified key is equal to
        /// this key
        /// </summary>
        /// <param name="other">The other key</param>
        /// <returns>True if the keys are equal</returns>
        public bool Equals(EntityKey<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return EqualityComparer<T>.Default.Equals(this._v, other._v);
        }

        #endregion
    }

    /// <summary>
    /// Strongly typed multipart key implementation that avoids value boxing.
    /// Nested instances of this class can be created for keys with 3 or more
    /// members.
    /// </summary>
    /// <typeparam name="T1">The Type of the first part of the key</typeparam>
    /// <typeparam name="T2">The Type of the second part of the key</typeparam>
    internal class EntityKey<T1, T2> : EntityKey, IEquatable<EntityKey<T1, T2>>
    {
        private T1 _v1;
        private T2 _v2;

        /// <summary>
        /// Constructs a key
        /// </summary>
        /// <param name="v1">The first key value</param>
        /// <param name="v2">The second key value</param>
        public EntityKey(T1 v1, T2 v2)
        {
            VerifyKeyValueNotNull(v1);
            VerifyKeyValueNotNull(v2);
            this._v1 = v1;
            this._v2 = v2;
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return this._v1.GetHashCode() ^ this._v2.GetHashCode();
        }

        /// <summary>
        /// Determines if the specified key is equal to
        /// this key
        /// </summary>
        /// <param name="obj">The other key</param>
        /// <returns>True if the keys are equal</returns>
        public override bool Equals(object obj)
        {
            return this.Equals((EntityKey<T1, T2>)obj);
        }

        /// <summary>
        /// Recursively build the list of key values
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to</param>
        internal override void FormatKey(StringBuilder sb)
        {
            FormatKeyValue(sb, this._v1);
            FormatKeyValue(sb, this._v2);
        }

        #region IEquatable<EntityKey<T1,T2>> Members

        /// <summary>
        /// Determines if the specified key is equal to
        /// this key
        /// </summary>
        /// <param name="other">The other key</param>
        /// <returns>True if the keys are equal</returns>
        public bool Equals(EntityKey<T1, T2> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return EqualityComparer<T1>.Default.Equals(this._v1, other._v1) &&
                   EqualityComparer<T2>.Default.Equals(this._v2, other._v2);
        }

        #endregion
    }
}
